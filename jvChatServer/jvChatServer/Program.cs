using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a new chat controller / manager 
            jvChatServer.Core.ChatController controller = new Core.ChatController(1024, "users.db");

            //Start the chat server 
            controller.Start();

            //Will be adding a the ability to execute server commands via the console window here 
            while (true)
            {
                //just a space holder for now 
                Console.ReadLine(); 
            }

        }

    }
}
