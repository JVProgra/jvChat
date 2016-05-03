using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using jvChatServer.Core.Networking.Packets;
using ChatClientSimulator.Core.Networking;

namespace jvChatServer.Core.Networking
{
    abstract class BaseClient
    {

        //=== Accessible Fields / Properties 
        public ConnectionProtocol Protocol { get; private set; }
        public Guid UUID { get; private set; }
        public bool Enabled { get; private set; }

        // Returns the ip address of the connection (read only) 
        public string IPAddress { get; private set; }
        public int Port { get; private set;  }

        //=== Class Variables === 
        //The socket instance we will use to store our inbound connection 
        private Socket client;
        private Thread recThread; 
        private object sendLocker;
        private object recLocker;
        protected Util.Cryptography Crypto; 

        //=== Event Handlers === 
        public delegate void DisconnectedHandler(BaseClient client);
        public event DisconnectedHandler Disconnected;

        public delegate void ReceivedPacketHandler(BaseClient client, iPacket packet);
        event ReceivedPacketHandler PacketReceived; 

        /// <summary>
        /// Constructor to create instance of new base in bound client 
        /// </summary>
        /// <param name="args">The connection arguements (Source server and inbound client)</param>
        public BaseClient(string server, int port, ConnectionProtocol protocol, Guid uuid)
        {
            //Create a new client socket to use for the connection
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //pass the connection settings
            this.IPAddress = server;
            this.Port = port;
            this.Protocol = protocol;
            this.UUID = uuid; 

            //Intialize any other variables in the class to a safe state
            sendLocker = new object();
            recLocker = new object();
            Crypto = new Util.Cryptography();
            Crypto.PrivateKey = "{JVPROGRAMMING}";
            Crypto.PublicKey = this.UUID.ToString(); 
        }

        /// <summary>
        /// Cal this method to start receiving data from the client 
        /// </summary>
        /// <returns>returns true if the thread was successfully started</returns>
        public bool Startup()
        {
            //If the base socket is already running then return false 
            if (Enabled)
                return false;

            this.Enabled = true; 

            //could try to resolve the server ip address...
            try
            {
                client.Connect(new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), Port));
            }
            catch (Exception ex)
            {
                //log error message here 

                //return false cause we didn't connect 
                return false; 
            }

            //do handshake here 

            //Send the protocol type to the server 
            client.Send(BitConverter.GetBytes((int)this.Protocol));

            //Send the client uuid to the server 
            client.Send(this.UUID.ToByteArray());


            //Create a new bg thread to handle this connections receive data on 
            recThread = new Thread(() =>
            {
                //lambda inline function to create the thread 
                try
                {
                    //Start receiving data asychronusly 
                    this.client.BeginReceive(new byte[] { 0 }, 0, 0, 0, ReceiveData, null);
                }
                catch (Exception ex) //error occured and we were unable to rec data (probably disconnected...) 
                {
                    //Lastly cleanup this object (kind of like an internal dispose to clean up our objects) 
                    Cleanup(); 
                }
            });
            recThread.IsBackground = true;
            recThread.Start(); 

            //Return true as the base client is now running
            return true; 
        }

        /// <summary>
        /// Call this function to send data to the client 
        /// </summary>
        /// <param name="data">The data you wish to send the client</param>
        /// <returns>The amount of bytes sent</returns>
        protected int sendBytes(byte[] data)
        {
            //Lock the sender object so we can only call send once at a time (it will "Queue" the rest of the calls and execute accordingly) 
            lock(sendLocker)
            {
                try
                {
                    //Send the size of the packet 
                    this.client.Send(BitConverter.GetBytes(data.Length));

                    //Send the packet itself and return the length of data sent + 4 (size of packet is an integer; thus + 4 bytes) 
                    return this.client.Send(data) + 4;
                }
                catch (Exception ex)
                {
                    //ERROR LOG HERE 

                    //We are unable to send data and the client may have been disconnected so close the connection (client cant reconnect) 
                    Cleanup(); 
                }
                return 0; 
            }
            
        }

        protected abstract int sendData(byte[] data); 

        //We will use this callback to begin receiving data 
        private void ReceiveData(IAsyncResult ir)
        {
            lock(recLocker)
            {
                try
                {
                    //Create a memory stream to store the buffer of the incoming data 
                    MemoryStream ms = new MemoryStream();

                    //Create some size variables to help calculate how much data is to be and left to be received 
                    int read = 0;
                    int size = 0;

                    //A buffer to use for parts of the incoming data (starting at 4 bytes to receive the "Size" of the incoming packet 
                    byte[] buffer = new byte[4];

                    //Try to receive the amount of data 
                    read = client.Receive(buffer);

                    //invalid packet size received 
                    if (read == 0) 
                        Cleanup();
                    //Valid packet size received 
                    else if (read == 4)
                    {
                        //Convert the size buffer into a valid integer 
                        size = BitConverter.ToInt32(buffer, 0);

                        //Resize the buffer to 8 kb (we will be receiving data 8 kbs at a time if) 
                        buffer = new byte[8192];

                        //While there is data left to receive 
                        while (size > 0)
                        {
                            //receive some of that data and store the amount in the read variable 
                            //conditional operator used to not over receive data (E.g. size is less then buffer 
                            read = client.Receive(buffer, 0, size > buffer.Length ? buffer.Length : size, SocketFlags.None);

                            //Write the buffer to the ms 
                            ms.Write(buffer, 0, read);

                            //Subtract from the total amount of data left to receive 
                            size -= read;
                        }

                        //If the event handler is set for the inbound data 
                        handleInboundData(ms.ToArray()); //handle the data (This will differ depending on the protocol) 
                    }

                    //Clean up the resources used 
                    buffer = null;
                    read = 0;
                    size = 0;
                    ms.Dispose();

                    //If the client is still enabled... 
                    if(Enabled)
                        //Try receiving more data 
                        this.client.BeginReceive(new byte[] { 0 }, 0, 0, 0, ReceiveData, null);
                }
                catch (Exception ex)
                {
                    //Lastly cleanup this object (kind of like an internal dispose to clean up our objects) 
                    Cleanup();
                }
            //END OF LOCKER 
            } 
        }

        public abstract void handleInboundData(byte[] data);

        /// <summary>
        /// Call this method to cleanup and close the existing connection 
        /// </summary>
        public void Cleanup()
        {
            //If the client is still enabled 
            if(Enabled)
            {
                //We have failed to receive data so lets notify the main program (typically means the client has disconnected) 
                if (Disconnected != null)
                    ///Call the event using this object 
                    Disconnected(this);

                //set the object to disabled state 
                Enabled = false;

                //close the thread 
                if (recThread.IsAlive)
                    recThread.Abort();

                //close and dispose the socket 
                this.client.Dispose(); 
            }
        }
    }
}
