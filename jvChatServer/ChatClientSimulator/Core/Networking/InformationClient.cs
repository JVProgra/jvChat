using ChatClientSimulator.Core.Networking;
using jvChatServer.Core.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core.Networking
{
    class InformationClient : BaseClient
    {
        /// <summary>
        /// Event handler invoked when information packets are received which will then be handled by the connection controller 
        /// </summary>
        public event ReceivedPacketHandler PacketReceived;
        
        //Constructor  
        public InformationClient(string server, int port) : base(server, port, ConnectionProtocol.Information, new Guid())
        {
            //Line above passes connection args to parent class for initialization 
        }

        /// <summary>
        /// This method will handle incoming raw data and perform encryption/decompression/decoding before raising an event with an information packet 
        /// </summary>
        /// <param name="data">The raw data of the packet</param>
        public override void handleInboundData(byte[] data)
        {
            //If the event is being handled 
            if(PacketReceived != null)
            {
                //Perform Decryption here 
                Crypto.Decrypt(data);

                //Perform Decompression here 

                //Perform decapsulation process and raise event 
                PacketReceived(this, InformationPacket.Decapsulate(data));
            }
        }

        /// <summary>
        /// Call this method to send a new information packet
        /// </summary>
        /// <param name="infoPacket">The information packet to send</param>
        /// <returns>The length of the packet sent</returns>
        public int SendPacket(InformationPacket infoPacket)
        {
            //Encode the packet into raw data and send it 
            return sendData(InformationPacket.Encapsulate(infoPacket));
        }

        protected override int sendData(byte[] data)
        {
            //If the data is null then exit and send nothing (probably an invalid packet was attempted to be sent) 
            if (data == null)
                return 0;

            //perform compression 

            //perform encryption 
            Crypto.Encrypt(data);

            return base.sendBytes(data); 
        }
    }
}
