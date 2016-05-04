using jvChatServer.Core.Networking;
using jvChatServer.Core.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClientSimulator
{
    class Program
    {
        static bool authenticated = false; 


        static void Main(string[] args)
        {
            //Create a new connection to the server 
            InformationClient ic = new InformationClient("127.0.0.1", 1024);

            //Setup events for that client 
            ic.Disconnected += Ic_Disconnected;
            ic.PacketReceived += Ic_PacketReceived;

            //If the connection was made correctly then we need to login 
            if (ic.Startup())
            {
                //Get a username and password here 
                string name, password;

                //Get a username 
                Console.WriteLine("Username: ");
                name = Console.ReadLine();

                //Get a password 
                Console.WriteLine("Password: ");
                password = Console.ReadLine();

                //Create a packet to request a login 
                InformationPacket ip = new InformationPacket(InformationHeader.Login, name + ";" + password);

                //Send the packet 
                ic.SendPacket(ip);

                //While we waiting for authentication... 
                while (!authenticated)
                {
                    //do nothing? 
                }
                
                while(authenticated)
                {
                    string mess;
                    Console.Write("Message: ");
                    mess = Console.ReadLine();

                    Console.WriteLine(ic.SendPacket(new InformationPacket(InformationHeader.Message, name + ";" + mess))); 
                }
                
            }
            else
            {
                Console.WriteLine("Unable to connect to the server, please restart the program."); 
            }

            System.Diagnostics.Process.GetCurrentProcess().WaitForExit();
        }

        private static void Ic_PacketReceived(BaseClient client, jvChatServer.Core.Networking.Packets.iPacket packet)
        {
            InformationPacket ip = (InformationPacket)packet;

            switch (ip.Header)
            {
                case InformationHeader.LoginResponse:
                    if (ip.getBody() == "GOOD")
                    {
                        authenticated = true; 
                        Console.WriteLine("GOOD LOGIN!");
                    }
                    else
                        Console.WriteLine("BAD LOGIN!"); 
                    break;

                case InformationHeader.Message:
                    Console.WriteLine(ip.getBody());
                    break;
                default:
                    break;
            }

        }

        private static void Ic_Disconnected(BaseClient client)
        {
            authenticated = false; 
            Console.WriteLine("Lost connection to the server, please restart the program.");
        }
    }
}
