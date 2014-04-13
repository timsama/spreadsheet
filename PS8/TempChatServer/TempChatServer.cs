using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CustomNetworking;

namespace BC
{
    public class TempChatServer
    {
        // Listens for incoming connections
        private TcpListener server;

        // One StringSocket per connected client
        private List<StringSocket> allSockets;

        // the name associated with the socket
        private List<string> user_names;

        /// <summary>
        /// Launches a chat server that listens on port 5000
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            new TempChatServer(5000);
            Console.ReadLine();
        }

        /// <summary>
        /// Creates a ChatServer that listens for connections on the provided port
        /// </summary>
        public TempChatServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            allSockets = new List<StringSocket>();
            user_names = new List<string>();
            server.Start();
            server.BeginAcceptSocket(ConnectionReceived, null);
        }

        /// <summary>
        /// Deals with connection requests
        /// </summary>
        private void ConnectionReceived(IAsyncResult ar)
        {
            Socket socket = server.EndAcceptSocket(ar);
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);
            ss.BeginReceive(NameReceived, ss);
            server.BeginAcceptSocket(ConnectionReceived, null);
        }

        /// <summary>
        /// Receives the first line of text from the client, which contains the name of the remote
        /// user.  Uses it to compose and send back a welcome message.
        /// </summary>
        private void NameReceived(String name, Exception e, object p)
        {
            StringSocket ss = (StringSocket)p;
            lock (allSockets)
            {
                allSockets.Add(ss);
                user_names.Add(name);
                ss.BeginSend("START " + name + "\r\n", (ee, pp) => { }, null);
                ss.BeginReceive(IncomingCallback, ss);
            }
        }

        /// <summary>
        /// Deals with lines of text as they arrive at the server.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="e"></param>
        /// <param name="ss"></param>
        private void IncomingCallback(String line, Exception e, object ss)
        {
            Console.WriteLine(line);
            lock (allSockets)
            {
                //                foreach (StringSocket s in allSockets)
                for (int i = 0; i < allSockets.Count; i++)
                {
                    allSockets[i].BeginSend(line + "\r\n", (ex, py) => { }, null);
                }
            }
            ((StringSocket)ss).BeginReceive(IncomingCallback, ss);
        }


    }
}
