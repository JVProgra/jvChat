using jvChatServer.Core.Networking;
using jvChatServer.Core.Networking.Packets;
using jvChatServer.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core
{
    class ChatController
    {
        //=== Publically accessible properties / Fields ===

        //Our instance the the chat server 
        public Server Server { get; private set; }

        //=== Private class variables ===

        //Pending connections trying to authenticate as users 
        private List<InformationClient> PendingConnections;

        //Active user connections that have authenticated 
        private Dictionary<User, InformationClient> ActiveConnections;

        //Local database of users 
        private UserManager UserManager; 

        /// <summary>
        /// Default constructor, server will use port 7777
        /// </summary>
        public ChatController()
        {
            //Create a server object on port 7777 
            this.Server = new Server(7777);

            //Initialize the private class variables 
            Init();
        }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="serverPort">Specify the server port number</param>
        public ChatController(int serverPort, string userDatabasePath)
        {
            //Create a server object on the port specified 
            this.Server = new Server(serverPort);

            //Initialize the private class variables 
            Init(userDatabasePath);
        }

        //This method is called to setup all the private class variables 
        private void Init(string userDatabase = "users.db")
        {
            //Setup the connection queues 
            this.PendingConnections = new List<InformationClient>();
            this.ActiveConnections = new Dictionary<User, InformationClient>();

            //Setup and load the user database 
            this.UserManager = Users.UserManager.FromFile(userDatabase);

            //Setup events 
            this.Server.InboundConnection += Server_InboundConnection;
        }

        //Tis method is used to handle incoming connection attempts 
        private void Server_InboundConnection(ConnectionArgs args)
        {
            //Invalid connection attempt was made 
            if (!args.isValid)
            {
                //So close the attempt and exit the event 
                args.Client.Dispose();
                return;
            }

            //Based on the protocol type of the connection, handle it accordingly by using the appropriate protocol wrapper 
            switch (args.Protocol)
            {
                //Information protocol for commands and text messages 
                case ConnectionProtocol.Information:

                    //Create a new information client handler 
                    InformationClient ic = new InformationClient(args);
                    
                    //Setup events for the new object 
                    ic.Disconnected += Disconnected;
                    ic.PacketReceived += PacketReceived;

                    //Add the information client to the pending authentication list 
                    PendingConnections.Add(ic); 

                    //Start handling the connection as an information client 
                    ic.Startup();
                    
                    //Exit event now cause there is nothing more to do 
                    return;
                //All other protocol values are currently not handled or are considered invalid 
                case ConnectionProtocol.Audio:
                case ConnectionProtocol.Video:
                case ConnectionProtocol.Invalid:
                default:
                    //So close the connection and exit 
                    args.Client.Dispose();
                    return; 
            }
        }

        //This event handles new incoming packets
        private void PacketReceived(BaseClient client, Networking.Packets.iPacket packet)
        {
            //Depending on the protocol type we will need to handle the packets differently 
            switch (client.Protocol)
            {
                //If it's a information protocol... 
                case ConnectionProtocol.Information:

                    //Convert our client and packet to information formats 
                    InformationClient ic = (InformationClient)client;
                    InformationPacket ip = (InformationPacket)packet;

                    //Based on the information packet type we need to handle data accordingly 
                    switch (ip.Header)
                    {
                        case InformationHeader.Login:

                            

                            break; 
                        //All other packet types are not currently being handled 
                        default:
                            //Get rid of packet data 
                            ip = null; 
                            break;
                    }


                    break;
                //All other protocols are not currently being handled 
                case ConnectionProtocol.Audio:
                case ConnectionProtocol.Video:
                default:
                    break; 
            }

        }

        private void Disconnected(BaseClient client)
        {
            throw new NotImplementedException();
        }
    }
}
