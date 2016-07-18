using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core.Networking
{
    /// <summary>
    /// These are the different protocols our chat software will be able to use
    /// </summary>
    public enum ConnectionProtocol
    {
        Invalid = 0,
        Information,
        Audio,
        Video
    }

    class ConnectionArgs
    {
        /// <summary>
        /// The server socket in which the connection is coming from
        /// </summary>
        public Server Server { get; private set; }
        /// <summary>
        /// The new inbound connection
        /// </summary>
        public Socket Client { get; private set; }

        /// <summary>
        /// This field stores the protocol type currently running 
        /// </summary>
        public ConnectionProtocol Protocol { get; private set; }

        /// <summary>
        /// Unique user identifier -> For id and public password of the connection 
        /// </summary>
        public Guid UUID { get; private set; }

        /// <summary>
        /// If the client is not equal to null, then the connection was good and the handshake was succesfull 
        /// </summary>
        public bool isValid { get { return Client != null; } }

        /// <summary>
        /// Creates a new instance of the connection args object 
        /// </summary>
        /// <param name="server">The source of the inbound connection</param>
        /// <param name="client">The new inbound connection</param>
        public ConnectionArgs(Server server, Socket client)
        {
            //Pass the parameters 
            this.Server = server;
            this.Client = client;

            //Call the handshaker method to detemine the protocol
            handshaker(); 
        }

        /// <summary>
        /// This method is called during construction to form the handshake and determine the connection type 
        /// </summary>
        private void handshaker()
        {
            //This is the information we need to receive from the client before a connection can establish 
            byte[] protocol = new byte[4];
            byte[] guid = new byte[16];

            //4 bytes id 
            //16 bytes for guid of uuid 

            //Receive the protocol type first 
            if(this.Client.Receive(protocol) == 4)
            {
                //If 4 bytes were received into protcol, try to receive the guid as well 
                if(this.Client.Receive(guid) == 16)
                {
                    //If the guid was received correctly, lets convert the protocol and guid and store them in the class variables 
                    this.Protocol = (ConnectionProtocol)BitConverter.ToInt32(protocol, 0);
                    this.UUID = new Guid(guid);

                    //***Could add possible redundency check here to check if the protocol is a valid number***

                    //Now exit the method 
                    return;
                }
            }

            //Handshake failed, close the connection 
            this.Client.Dispose();
            this.Client = null; //set the client to null as this is no longer a valid connection 
        }
    }
}
