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
    public partial class frmChat : Form
    {
        //This is the username of the agent that logged in 
        private string agentName = string.Empty;

        public frmChat(string username = "default")
        {
            InitializeComponent();

            //Setup event handlers for the connection
            Program.Connection.Disconnected += Connection_Disconnected;
            Program.Connection.PacketReceived += Connection_PacketReceived;

            //Assign the username to the form
            this.agentName = username;
        }

        private void Connection_PacketReceived(jvChatServer.Core.Networking.BaseClient client, jvChatServer.Core.Networking.Packets.iPacket packet)
        {
            //Cast the packet to information (the only type of protocol  we are currently handling...) 
            InformationPacket ip = (InformationPacket)packet;

            //This can be used for parsing data if needed 
            string[] args;

            //Based on the type of packet handle the information accordingly 
            Invoke((MethodInvoker)delegate
            {
                switch (ip.Header)
                {
                    case InformationHeader.Message:

                        //new incoming message so lets get the user name and message 
                        args = ip.getBody().Split(';');

                        //Output the name and message (can colour code stuff here too  
                        rtxtMessages.Text += "\n" + args[0] + ": " + args[1];

                        rtxtMessages.Select(rtxtMessages.Text.Length - 1, 1);
                        rtxtMessages.ScrollToCaret();
                        break;
                    case InformationHeader.ServerMessage:
                        rtxtMessages.Text += "\nMessage From Server: " + ip.getBody();
                        break;
                    default:
                        //unhandled packet here 
                        break;
                }
            });

        }

        private void Connection_Disconnected(jvChatServer.Core.Networking.BaseClient client)
        {
            Invoke((MethodInvoker)delegate
            {
                MessageBox.Show("Disconnected from the server, please reconnect!");

                Environment.Exit(0); 
            });
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            //If the user is typing a message and presses the enter button 
            if(e.KeyCode == Keys.Enter)
            {
                //Perform a click on the send button... 
                btnSend.PerformClick();
                txtMessage.Text = "";
            }

            //remove any weird spaces at the beginning 
            txtMessage.Text = txtMessage.Text.TrimStart();
        }

        private void frmChat_Load(object sender, EventArgs e)
        {
            //Select the txtMessage 
            this.txtMessage.Select();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //If there is no message to send then exit 
            if (txtMessage.Text.Length == 0)
                return;

            //Trim the message
            txtMessage.Text = txtMessage.Text.Trim();

            //Else make a new packet and send the message 
            Program.Connection.SendPacket(new InformationPacket(InformationHeader.Message, agentName + ";" + txtMessage.Text));

            //Append a copy of the local message here too
            //rtxtMessages.Text += "ME: " + txtMessage.Text;

            //Reset the message field so it's ready for a new message 
            txtMessage.Text = "";
        }
    }
}
