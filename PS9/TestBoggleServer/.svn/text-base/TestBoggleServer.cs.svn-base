using CustomNetworking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BC;

namespace TestBoggleServer
{
    [TestClass]
    public class TestBoggleServer
    {
        static BoggleServer bs = new BoggleServer(9, "boggle_words.txt", "AKULQCOMSOREITFE");

        ///</summary>
        [TestMethod()]
        public void Test1()
        {
            new Test1Class().run(2000);
            new Test2Class().run(2000);
            bs.CloseServer();
            bs = new BoggleServer(9, "boggle_words.txt");
        }

        [TestMethod()]
        public void NullPlayer()
        {
            new NullPlayerTestClass().run();
        }

        public class NullPlayerTestClass
        {
            public void run()
            {
                // set up dummy client info
                TcpClient client = new TcpClient("localhost", 2000);
                StringSocket socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                StringSocket socket2 = new StringSocket(client.Client, UTF8Encoding.Default);

                Game g = new Game(1, new Player("p1", socket1), new Player("p2", socket2), "aaaaaaaaaaaaaaaa", bs);

                // try to get a nonexistant player
                Assert.IsNull(g.GetPlayer(0));

                // try to send a word on behalf of a nonexistant player
                GetWordEventArgs e = new GetWordEventArgs("test", 0);
                g.OnGetWord(this, e);

                // assert that because the word was not associated with a player, the score does not change
                Assert.AreEqual(0, g.CalculateScore(g.P1));
                Assert.AreEqual(0, g.CalculateScore(g.P2));
            }
        }

        [TestMethod()]
        public void ScoreZero()
        {
            new ScoreZeroTestClass().run();
        }

        public class ScoreZeroTestClass
        {
            public void run()
            {
                // set up dummy client info
                TcpClient client = new TcpClient("localhost", 2000);
                StringSocket socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                StringSocket socket2 = new StringSocket(client.Client, UTF8Encoding.Default);

                Game g = new Game(1, new Player("p1", socket1), new Player("p2", socket2), "aaaaaaaaaaaaaaaa", bs);

                // wait for the game to end
                Thread.Sleep(1000);

                // assert that both players' scores are zero
                Assert.AreEqual(0, g.CalculateScore(g.P1));
                Assert.AreEqual(0, g.CalculateScore(g.P2));
            }
        }

        [TestMethod()]
        public void NullBoardface()
        {
            new NullBoardfaceTestClass().run();
        }

        public class NullBoardfaceTestClass
        {
            public void run()
            {
                // set up dummy client info
                TcpClient client = new TcpClient("localhost", 2000);
                StringSocket socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                StringSocket socket2 = new StringSocket(client.Client, UTF8Encoding.Default);

                Game g = new Game(1, new Player("p1", socket1), new Player("p2", socket2), null, bs);

                // wait for the game to end
                Thread.Sleep(1000);

                // assert that both players' scores are zero
                Assert.AreEqual(0, g.CalculateScore(g.P1));
                Assert.AreEqual(0, g.CalculateScore(g.P2));
            }
        }

        ///</summary>
        [TestMethod()]
        public void Disconnect()
        {
            new DisconnectTestClass().run(2000);
        }

        public class Test1Class
        {
            // Data that is shared across threads
            private List<string> s1 = new List<string>();
            String sendBlock1;
            private List<string> s2 = new List<string>();

            StringSocket socket1;
            StringSocket socket2;

            public void run(int port)
            {
                List<string> p1_played_words = BuildListFromPath("AKULQCOMSOREITFE_p1_played_words.txt");
                List<string> p2_played_words = BuildListFromPath("AKULQCOMSOREITFE_p2_played_words.txt");
                List<string> both_legal_words = BuildListFromPath("AKULQCOMSOREITFE_both_legal_words.txt");
                List<string> p1_illegal_words = BuildListFromPath("AKULQCOMSOREITFE_p1_illegal_words.txt");
                List<string> p2_illegal_words = BuildListFromPath("AKULQCOMSOREITFE_p2_illegal_words.txt");
                List<string> p1_legal_words = BuildListFromPath("AKULQCOMSOREITFE_p1_legal_words.txt");
                List<string> p2_legal_words = BuildListFromPath("AKULQCOMSOREITFE_p2_legal_words.txt");

                // Connect the two players
                Connect1("localhost", 2000, "Player1");
                Connect2("localhost", 2000, "Player2");

                // Now send the data the words form the p1 played words list as one string block
                foreach (string s in p1_played_words)
                {
                    sendBlock1 = sendBlock1 + "Word " + s + "\n";
                }

                SendMessage(sendBlock1, socket1);

                // Send an invalid command from player 2
                SendMessage("invalid", socket2);

                // Send the words from the p2 played words list
                foreach (string s in p2_played_words)
                {
                    SendMessage("word " + s, socket2);
                }

                // Give the test time to finish
                System.Threading.Thread.Sleep(11000);

                //Make sure player 2's invalid command was ignored
                Assert.IsTrue(s2.Contains("IGNORING"));

                // Make sure the last messge containing the STOP command and the summary of words has the correct 
                // number of words in each list for player 1:
                // legal words played by both players = 8
                // p1 illegal words = 8
                // p1 legal words = 12
                // p2 legal words = 6
                // p2 illegal words = 9
                int wordnum;
                List<int> correct = new List<int>();
                correct.Add(15);
                correct.Add(6);
                correct.Add(12);
                correct.Add(13);
                correct.Add(9);

                List<int> toCheck = new List<int>();

                // Split the STOP message into seperate strings by whitespace
                string[] message = s1[s1.Count - 1].Split(' ');

                //iterate through and find the integers and see if each integer is equal
                // to the corresponding integer in the correct list
                foreach (string s in message)
                {
                    if (Int32.TryParse(s, out wordnum))
                    {
                        toCheck.Add(wordnum);
                    }
                }

                int count = 0;
                //check to see if the created list is equal to the correct list
                foreach (int i in correct)
                {
                    Assert.AreEqual(i, toCheck[count]);
                    count++;
                }

                // Close the sockets and the server
                socket1.CloseSocket();
                socket2.CloseSocket();
            }

            /// <summary>
            /// Send a line of text to the server.
            /// </summary>
            /// <param name="line"></param>
            public void SendMessage(String line, StringSocket socket)
            {
                try
                {
                    if (socket != null)
                    {
                        socket.BeginSend(line + "\n", (e, p) => { }, socket);
                    }
                }
                catch
                {
                    socket.CloseSocket();
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect1(string hostname, int port, String name)
            {
                if (socket1 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket1.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket1.BeginReceive(LineReceived1, null);
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect2(string hostname, int port, String name)
            {
                if (socket2 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket2 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket2.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket2.BeginReceive(LineReceived2, null);
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived1(String s, Exception e, object p)
            {
                try
                {
                    s1.Add(s);

                    if (!s.Contains("STOP"))
                        socket1.BeginReceive(LineReceived1, null);
                }
                catch
                {
                    socket1.CloseSocket();
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived2(String s, Exception e, object p)
            {
                try
                {
                    s2.Add(s);

                    if (!s.Contains("STOP"))
                        socket2.BeginReceive(LineReceived2, null);
                }
                catch
                {
                    socket2.CloseSocket();
                }
            }

            /// <summary>
            /// Builds a list of strings from a file containing one word on each line
            /// </summary>
            /// <param name="file_path"></param>
            /// <returns></returns>
            private static List<string> BuildListFromPath(string file_path)
            {

                List<string> temp = new List<string>();
                // The file will have one word in the dictionary on every line
                // Add each word in the file to the boggleDictionary
                try
                {
                    using (StreamReader sr = new StreamReader(file_path))
                    {
                        while (!sr.EndOfStream)
                        {
                            temp.Add((sr.ReadLine().Trim()).ToUpper());
                        }
                        sr.Close();
                    }
                }
                catch (Exception e)
                {
                    // the file could not be read
                    throw e;
                }
                return temp;
            }
        }

        public class DisconnectTestClass
        {
            // Data that is shared across threads
            private List<string> s1 = new List<string>();
            private List<string> s2 = new List<string>();

            StringSocket socket1;
            StringSocket socket2;

            public void run(int port)
            {
                // Connect the two players
                Connect1("localhost", 2000, "Player1");
                Connect2("localhost", 2000, "Player2");

                // Close the sockets before the game has time to end
                socket1.CloseSocket();
                socket2.CloseSocket();

                // Give the test time to finish
                System.Threading.Thread.Sleep(1000);
            }

            /// <summary>
            /// Send a line of text to the server.
            /// </summary>
            /// <param name="line"></param>
            public void SendMessage(String line, StringSocket socket)
            {
                try
                {
                    if (socket != null)
                    {
                        socket.BeginSend(line + "\n", (e, p) => { }, socket);
                    }
                }
                catch
                {
                    socket.CloseSocket();
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect1(string hostname, int port, String name)
            {
                if (socket1 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket1.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket1.BeginReceive(LineReceived1, null);
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect2(string hostname, int port, String name)
            {
                if (socket2 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket2 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket2.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket2.BeginReceive(LineReceived2, null);
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived1(String s, Exception e, object p)
            {
                try
                {
                    s1.Add(s);

                    if (!s.Contains("STOP"))
                        socket1.BeginReceive(LineReceived1, null);
                }
                catch
                {
                    socket1.CloseSocket();
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived2(String s, Exception e, object p)
            {
                try
                {
                    s2.Add(s);

                    if (!s.Contains("STOP"))
                        socket2.BeginReceive(LineReceived2, null);
                }
                catch
                {
                    socket2.CloseSocket();
                }
            }

            /// <summary>
            /// Builds a list of strings from a file containing one word on each line
            /// </summary>
            /// <param name="file_path"></param>
            /// <returns></returns>
            private static List<string> BuildListFromPath(string file_path)
            {

                List<string> temp = new List<string>();
                // The file will have one word in the dictionary on every line
                // Add each word in the file to the boggleDictionary
                try
                {
                    using (StreamReader sr = new StreamReader(file_path))
                    {
                        while (!sr.EndOfStream)
                        {
                            temp.Add((sr.ReadLine().Trim()).ToUpper());
                        }
                        sr.Close();
                    }
                }
                catch (Exception e)
                {
                    // the file could not be read
                    throw e;
                }
                return temp;
            }
        }

        public class Test2Class
        {
            // Data that is shared across threads
            private List<string> s1 = new List<string>();
            private List<string> s2 = new List<string>();

            StringSocket socket1;
            StringSocket socket2;

            public void run(int port)
            {
                List<string> both_legal_words = BuildListFromPath("AKULQCOMSOREITFE_both_legal_words.txt");

                // Connect the two players
                Connect1("localhost", 2000, "Player1");
                Connect2("localhost", 2000, "Player2");

                // Have player 1 send a few words
                foreach (string s in both_legal_words)
                {
                    SendMessage("word " + s, socket2);
                }

                // Disconnect player 2
                socket2.CloseSocket();

                // Give the test some time to finish
                System.Threading.Thread.Sleep(3000);

                //Make sure the last message player 1 got was TERMINATED
                Assert.IsTrue(s1.Contains("TERMINATED"));
            }


            /// <summary>
            /// Send a line of text to the server.
            /// </summary>
            /// <param name="line"></param>
            public void SendMessage(String line, StringSocket socket)
            {
                try
                {
                    if (socket != null)
                    {
                        socket.BeginSend(line + "\n", (e, p) => { }, socket);
                    }
                }
                catch
                {
                    socket.CloseSocket();
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect1(string hostname, int port, String name)
            {
                if (socket1 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket1 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket1.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket1.BeginReceive(LineReceived1, null);
                }
            }

            /// <summary>
            /// Connect to the server at the given hostname and port and with the give name.
            /// </summary>
            public void Connect2(string hostname, int port, String name)
            {
                if (socket2 == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket2 = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket2.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket2.BeginReceive(LineReceived2, null);
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived1(String s, Exception e, object p)
            {
                try
                {
                    s1.Add(s);

                    if (!s.Contains("STOP"))
                        socket1.BeginReceive(LineReceived1, null);
                }
                catch
                {
                    socket1.CloseSocket();
                }
            }

            /// <summary>
            /// Deal with an arriving line of text.
            /// </summary>
            private void LineReceived2(String s, Exception e, object p)
            {
                try
                {
                    s2.Add(s);

                    if (!s.Contains("STOP"))
                        socket2.BeginReceive(LineReceived2, null);
                }
                catch
                {
                    socket2.CloseSocket();
                }
            }

            /// <summary>
            /// Builds a list of strings from a file containing one word on each line
            /// </summary>
            /// <param name="file_path"></param>
            /// <returns></returns>
            private static List<string> BuildListFromPath(string file_path)
            {

                List<string> temp = new List<string>();
                // The file will have one word in the dictionary on every line
                // Add each word in the file to the boggleDictionary
                try
                {
                    using (StreamReader sr = new StreamReader(file_path))
                    {
                        while (!sr.EndOfStream)
                        {
                            temp.Add((sr.ReadLine().Trim()).ToUpper());
                        }
                        sr.Close();
                    }
                }
                catch (Exception e)
                {
                    // the file could not be read
                    throw e;
                }
                return temp;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void NoDictionary()
        {
            bs.BuildDictionaryFromPath("doesn't exist.lol");
        }

        public void BoggleServerMain()
        {
            string[] args = new string[3];

            args[0] = "20";
            args[1] = "boggle_words.txt";
            args[2] = "aaaaaaaaaaaaaaaa";

            BoggleServer.Main(args);
        }
    }
}

