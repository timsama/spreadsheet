using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CustomNetworking;
using BC;

namespace BC
{
    public class TestBoggleClientModel
    {

        // The socket used to communicate with the server.  If no connection has been
        // made yet, this is null.
        private StringSocket socket;

        // Register for this event to be motified when a line of text arrives.
        public event Action<String> IncomingLineEvent;

        /// <summary>
        /// Creates a not yet connected client model.
        /// </summary>
        public TestBoggleClientModel()
        {
            socket = null;
        }

        /// <summary>
        /// Connect to the server at the given hostname and port and with the give name.
        /// </summary>
        public void Connect(string hostname, int port, String name)
        {
            if (socket == null)
            {
                TcpClient client = new TcpClient(hostname, port);
                socket = new StringSocket(client.Client, UTF8Encoding.Default);
                socket.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                socket.BeginReceive(LineReceived, null);
            }
        }

        /// <summary>
        /// Send a line of text to the server.
        /// </summary>
        /// <param name="line"></param>
        public void SendMessage(String line)
        {
            String esc = "" + (char)27;
            String newline = "" + '\n';
            line = line.Replace("\\e", esc);
            line = line.Replace("\\n", newline);
            try
            {
                if (socket != null)
                {
                    socket.BeginSend(line + "\n", (e, p) => { }, null);
                }
            }
            catch 
            {
                socket.CloseSocket();
            }
        }

        /// <summary>
        /// Deal with an arriving line of text.
        /// </summary>
        private void LineReceived(String s, Exception e, object p)
        {
            try
            {
                if (IncomingLineEvent != null)
                {
                    IncomingLineEvent(s);
                }
                if (! s.Contains("STOP"))
                    socket.BeginReceive(LineReceived, null);
            }
            catch
            {
                socket.CloseSocket();
            }
        }
    }
}
