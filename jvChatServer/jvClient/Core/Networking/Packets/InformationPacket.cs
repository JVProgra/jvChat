using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core.Networking.Packets
{
    /// <summary>
    /// Packet command types for the information protocol 
    /// </summary>
    public enum InformationHeader
    {
        Invalid = 0, 
        Login,
        LoginResponse, 
        Message,
        ServerMessage,
        Command
    }

    class InformationPacket : iPacket
    {
        //=== Public Properties === 

        /// <summary>
        /// The command type for the information protocol 
        /// </summary>
        public InformationHeader Header { get; set; }
        /// <summary>
        /// The body of the data in a raw format 
        /// </summary>
        public byte[] Body { get; set; } 
        
        //Default constructor to set everything in a safe state
        public InformationPacket()
        {
            //Invalid packet type because nothing was specified 
            this.Header = InformationHeader.Invalid;
            
            //Create an empty body to help avoid null ptr exceptions 
            this.Body = new byte[] { 0 }; 
        }
        
        //Constructor for byte[] data 
        public InformationPacket(InformationHeader header, byte[] body)
        {
            //Pass the params to the class properties
            this.Header = header; 
            this.Body = body; 
        }
        
        public InformationPacket(InformationHeader header, string body)
        {
            //Pass the header param
            this.Header = header;
            
            //Set the body encoded as acsii 
            setBody(body); 
        }
        
        /// <summary>
        /// This method will convert a string to raw bytes to store in the body 
        /// </summary>
        /// <param name="data">The data in string format </param>
        public void setBody(string data)
        {
            this.Body = Encoding.ASCII.GetBytes(data); 
        }
        
        
        //Returns the body of the packet in ascii string format 
        public string getBody()
        {
            return Encoding.ASCII.GetString(this.Body); 
        }

        //Friend functions for encapsulations / decapsulation
        
        /// <summary>
        /// Call this method to encapsulate a packet 
        /// </summary>
        /// <param name="infoPacket">The packet to encapsulate</param>
        /// <returns>Returns raw bytes of the encoded packet</returns>
        public static byte[] Encapsulate(InformationPacket infoPacket)
        {
            //A raw data type to return 
            byte[] data = null;

            //If the body is not set, set it to empty to avoid null ptr exception 
            if (infoPacket.Body == null)
                infoPacket.Body = new byte[0]; 

            //Create MemoryStream to format packet int 
            using (MemoryStream ms = new MemoryStream())
            {
                //We will be using BinaryFormatters to format the data fields of the packet 
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //Write command header as integer 
                    bw.Write((int)infoPacket.Header);
                    
                    //Write the size of the body 
                    bw.Write(infoPacket.Body.Length);
                    
                    //Write the body of the packet 
                    bw.Write(infoPacket.Body); 
                }

                //Get the formatted packet as raw bytes 
                data = ms.ToArray(); 
            }

            //Return the raw bytes 
            return data; 
        }

        /// <summary>
        /// Call this method to decapsulate a byte[] into a InformationPacket object 
        /// </summary>
        /// <param name="data">The raw data of the packet</param>
        /// <returns>An information packet</returns>
        public static InformationPacket Decapsulate(byte[] data)
        {
            //Create an instance of an information packet to store the data in 
            InformationPacket ip = new InformationPacket();

            try
            {
                //Create a memory stream using the raw data 
                using (MemoryStream ms = new MemoryStream(data))
                {

                    //Try reading the data in binary format 
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        //Get the command header 
                        ip.Header = (InformationHeader)br.ReadInt32();

                        //Read the body data length and body 
                        ip.Body = br.ReadBytes(br.ReadInt32());
                    }
                }
            }
            catch (Exception ex)
            {
                //write to the log file here 

                //An error has occured, set packet command header to invalid so it can be ignored by our server 
                ip.Header = InformationHeader.Invalid;
            }

            //Return the InformationPacket instance here 
            return ip; 
        }
    }
}
