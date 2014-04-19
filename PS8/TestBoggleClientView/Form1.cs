using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BC
{
    public partial class Form1 : Form
    {
        private TestBoggleClientModel model;

        public Form1()
        {
            InitializeComponent();
            model = new TestBoggleClientModel();
            model.IncomingLineEvent += MessageReceived;
        }

        private void MessageReceived(String line)
        {
            receive_textbox.Invoke(new Action(() => { receive_textbox.Text += line + "\r\n"; }));
        }

        private void Play_button_Click(object sender, EventArgs e)
        {
            model.Connect("lab1-22.eng.utah.edu", 2500, player_textbox.Text);
            player_textbox.ReadOnly = true;
            Play_button.Visible = false;
            
        }

        private void send_button_Click(object sender, EventArgs e)
        {
            String sendtext = send_textbox.Text;

            sendtext = sendtext.Replace('@', (char)27);

            model.SendMessage(sendtext);
            send_textbox.Text = "";
        }

        private void send_textbox_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
