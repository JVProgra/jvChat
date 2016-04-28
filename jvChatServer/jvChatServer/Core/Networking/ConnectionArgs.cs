using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core.Networking
{
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
        /// Creates a new instance of the connection args object 
        /// </summary>
        /// <param name="server">The source of the inbound connection</param>
        /// <param name="client">The new inbound connection</param>
        public ConnectionArgs(Server server, Socket client)
        {
            this.Server = server;
            this.Client = client; 
        }
    }
}
