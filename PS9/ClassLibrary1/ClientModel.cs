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
    public class BoggleClientModel
    {

        // The socket used to communicate with the server.  If no connection has been
        // made yet, this is null.
        private StringSocket socket;

        // Register for this event to be notified when a line of text arrives.
        public event Action<String> IncomingLineEvent;

        // Register for this event to be notified when the server connection is lost
        public event Action ConnectionLostEvent;

        // Register for this event to be notified when there is no server connection
        public event Action NoServerConnectionEvent;

        /// <summary>
        /// Creates a not yet connected client model.
        /// </summary>
        public BoggleClientModel()
        {
            socket = null;
        }

        /// <summary>
        /// Connect to the server at the given hostname and port and with the give name.
        /// </summary>
        public void Connect(string hostname, int port, String name)
        {
            try
            {
                // open a connection and send a play request if the socket object exists
                if (socket == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket.BeginReceive(LineReceived, null);
                }
            }
            catch (SocketException)
            {
                // we were unable to connect to the server, raise a NoServerConnection event
                NoServerConnectionEvent();
            }
        }

        /// <summary>
        /// Send a line of text to the server.
        /// </summary>
        /// <param name="line"></param>
        public void SendMessage(String line)
        {
            try
            {
                // send the message to the server if the socket exists
                if (socket != null)
                {
                    socket.BeginSend(line + "\n", (e, p) => { }, null);
                }
            }
            catch 
            {
                // close the socket due to a failed send
                socket.CloseSocket();
            }
        }

        /// <summary>
        /// Deal with an arriving line of text.
        /// </summary>
        private void LineReceived(String s, Exception e, object p)
        {
            // if we received a socketexception, the server connection dropped
            if (e is SocketException)
            {
                ConnectionLostEvent();
            }
            else
            {

                try
                {

                    // split the string into chunks delimited by whitespace
                    char[] splitChars = { ' ', '\n', '\r' };
                    string[] parts = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

                    // check if the message is a stop command
                    if (!parts[0].Contains("STOP"))
                        socket.BeginReceive(LineReceived, null);

                    // create an incoming line event
                    if (IncomingLineEvent != null)
                    {
                        IncomingLineEvent(s);
                    }
                }
                catch
                {
                    // close the socket due to a problem
                    socket.CloseSocket();
                }
            }
        }
    }
}
