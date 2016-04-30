using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;   
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace jvChatServer.Core.Networking
{
    class Server
    {
        //=== Public Accessible Fields ===

        //Publically accessible port number (Read only for external use) 
        public int Port { get; private set; } 

        //Public accessible enabled property (read only for external use) 
        public bool Enabled { get; private set; }

        //=== Private variables ===
        private Socket svrSocket; //Welcome socket 
        private Thread svrThread; //background thread to run the server on 

        //=== Events === 

        //Inbound connection event -> occurs when a new connection is established 
        public delegate void InboundConnectionHandler(ConnectionArgs args);
        public event InboundConnectionHandler InboundConnection; 

        /// <summary>
        /// Creates an instance of the server object used to accept new incoming connections 
        /// </summary>
        /// <param name="port">The port number you want this instance of the server to use.</param>
        public Server(int port)
        {
            //Pass port to the object 
            this.Port = port;

            //Set other fields to safe state 
            Enabled = false;
            svrSocket = null;
            svrThread = null; 
        }

        /// <summary>
        /// Call this method to enable the socket server
        /// </summary>
        /// <returns>Returns true if successfull</returns>
        public bool Start()
        {
            //If the server is already running, don't do anything 
            if (Enabled)
                //Return false to tell the program we did not start another instance 
                return false;

            //Turn the server flag to enabled for our loops and recursion 
            Enabled = true;

            //Create new instance of a svr thread pointed at the "run" method 
            svrThread = new Thread(run);
            svrThread.IsBackground = true;

            //Run the thread 
            svrThread.Start();

            //Return true to indicate that the method executed correctly 
            return true; 
        }

        /// <summary>
        /// This method will be run on a background thread 
        /// </summary>
        private void run()
        {
            try
            {
                //Initialize the server socket using IPv4, Stream and TCP for protocol settings 
                svrSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //Bind the server socket to this computer on the port specified 
                svrSocket.Bind(new IPEndPoint(IPAddress.Any, this.Port)); 

                //If the server was bound correctly, then we can start listening for new inbound connections 
                if(svrSocket.IsBound)
                {
                    //Set the socket to listening state 
                    svrSocket.Listen(0);

                    //Begin accepting new connections here -> will call the acceptConnection method to receive the connection when a new connection is inbound 
                    svrSocket.BeginAccept(acceptConnection, null); 
                }
            }
            catch (Exception ex) //We may not use this.. 
            {
                throw new Exception("An error occured when setting up the server socket. Please make sure that the port is not use and that no firewall is blocking the server application from running.");
                //Write full exception to error log file? 
            }
        }

        /// <summary>
        /// This method is the asynchronus callback for our svrSocket.beginaccept
        /// </summary>
        /// <param name="ir"></param>
        private void acceptConnection(IAsyncResult ir)
        {
            try
            {
                //If our server is still enabled, then we can accept the connection 
                if(Enabled)
                {
                    //Receive the incoming connection and store it in the socket variable "client" 
                    Socket client = svrSocket.EndAccept(ir);


                    //If the event handler for inbound connection is being handled
                    if (InboundConnection != null)
                        //Raise the event with a new connection args using this as the server and our new inbound client as our client socket 
                        //Run event on new thread as ConnectionArgs will be performing a Handshake which may take time 
                        //********* CODE BELOW MAYBE UPDATED AT A LATER TIME TO BE MORE OPTIMIZED ************
                        new Thread(() => { try { InboundConnection(new ConnectionArgs(this, client)); } catch (Exception ex) { } }).Start(); 
                    else
                        //If the event is not set then dispose of the new inbound client as we have no where to send it
                        client.Dispose();


                    //Recursively begin accepting the next connection 
                    svrSocket.BeginAccept(acceptConnection, null);
                }
            }
            catch (Exception ex)
            {
                //Write exception to log file? 
            }
        }

        /// <summary>
        /// Call this method to disable the server socket 
        /// </summary>
        /// <returns>Returns true is stopped successfully or false if it was already closed</returns>
        public bool Stop()
        {
            //If the server is already stopped
            if (!Enabled)
                //Don't do anything and return false 
                return false;

            //If the server thread is still alive 
            if (svrThread.IsAlive)
                //Close it / abort it
                svrThread.Abort();

            //Close the svrSocket as well 
            svrSocket.Dispose(); 

            //Return true to signify that we have stopped the server 
            return true; 
        }
    }
}
