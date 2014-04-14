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
            model.Connect("155.98.111.73", 2500, player_textbox.Text);
            player_textbox.ReadOnly = true;
            Play_button.Visible = false;
            
        }

        private void send_button_Click(object sender, EventArgs e)
        {
            model.SendMessage(send_textbox.Text);
            send_textbox.Text = "";
        }

    }
}
