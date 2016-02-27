using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientGUI : Form
    {
        private ClientSocket socket;
        private Thread thread;
        public ClientGUI(string host, string name)
        {
            InitializeComponent();
            socket = new ClientSocket();
            socket.Start(IPAddress.Parse(host), 3306);
            //socket.Start("127.0.0.1", 11000);
            //socket.Start(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], 11000);
            thread = new Thread(GetMessage);
            thread.Start();
            socket.SendMessage(name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            socket.SendMessage(textBox1.Text);
            textBox1.Text = "";
        }

        private void textBox1_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                socket.SendMessage(textBox1.Text);
                textBox1.Text = "";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.textBox2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Invoke(d, new object[] {text});
            }
            else
            {
                this.textBox2.Text = (textBox2.Text+text);
                textBox2.AppendText(Environment.NewLine);
            }
            
        }

        private void GetMessage()
        {
            while (true)
            {
                var data = socket.CheckForMessage();
                if (data.Equals(""))
                    continue;
                SetText(data);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            socket.SendMessage("!c");
            thread.Abort();
            Application.Exit();
        }
    }
}
