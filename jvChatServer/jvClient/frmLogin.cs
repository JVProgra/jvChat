using jvChatServer.Core.Networking.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jvClient
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            
            //Add event handler for the connection on the login screen 
            Program.Connection.PacketReceived += Connection_PacketReceived;
            Program.Connection.Disconnected += Connection_Disconnected;

            //apply the uuid to the form
            this.txtUUID.Text = Program.Connection.UUID.ToString();

            //Create event handler for on form close
            this.FormClosed += FrmLogin_FormClosed;
        }

        private void FrmLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Remove the event handlers for the packets 
            Program.Connection.PacketReceived -= Connection_PacketReceived;
            Program.Connection.Disconnected -= Connection_Disconnected; 
        }

        private void Connection_Disconnected(jvChatServer.Core.Networking.BaseClient client)
        {
            MessageBox.Show("Disconnected from the server, please restart the program.");
            Environment.Exit(0); 
        }

        private void Connection_PacketReceived(jvChatServer.Core.Networking.BaseClient client, jvChatServer.Core.Networking.Packets.iPacket packet)
        {
            //Convert it to an information packet 
            InformationPacket ip = (InformationPacket)packet; 

            //Based on the header of the packet handle accordingly 
            switch(ip.Header)
            {
                case InformationHeader.LoginResponse:

                    //If the response is received, check the response type
                    if(ip.getBody() == "GOOD")
                    {
                        //Good response, so close the login form and open the chat window 
                        Invoke((MethodInvoker)delegate
                        {
                            //Stop handling the connection on this form 
                            Program.Connection.PacketReceived -= Connection_PacketReceived;
                            Program.Connection.Disconnected -= Connection_Disconnected;

                            //Create a new chat window with the users name 
                            frmChat chat = new frmChat(txtUsername.Text);

                            //Hide this form 
                            this.Hide();

                            //Show the chat program 
                            chat.Show();
                        });

                    }
                    else
                    {
                        //Bad response so notify the user 
                        //bring response to gui thread (it was received on a bg thread) 
                        Invoke((MethodInvoker)delegate
                        {
                            //Clear the password 
                            txtPassword.Clear();

                            //enable the login button 
                            btnLogin.Enabled = true;

                            //Notify the user of the results
                            MessageBox.Show("Invalid login credentials, please try again!", "Invalid login", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                        });
                    }

                    break;
                default:
                    //All other packets are not handled here so do nothing with them 
                    break; 
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            //If no data was entered then do nothing 
            if (txtPassword.Text.Length == 0 || txtUsername.Text.Length == 0)
                return; 

            //Disable the login button to stop duplicate attempts 
            btnLogin.Enabled = false;

            //Send the login attempt to the server 
            Program.Connection.SendPacket(new InformationPacket(InformationHeader.Login, txtUsername.Text + ";" + txtPassword.Text)); 
        }
    }
}
