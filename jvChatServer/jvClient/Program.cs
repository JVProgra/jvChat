using jvChatServer.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jvClient
{
    static class Program
    {
        public static InformationClient Connection; 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Before the main launches the GUI connect to the server 
            Connection = new InformationClient("127.0.0.1", 1024);

            //Try to connect 
            if(!Connection.Startup())
            {
                //If it fails tell the user 
                MessageBox.Show("Unable to connect to the server, please contact the system administrator.");

                //Exit the program 
                Environment.Exit(1);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmLogin());
        }
    }
}
