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

namespace StringSocketTester
{


    /// <summary>
    ///This is a test class for StringSocketTest and is intended
    ///to contain all StringSocketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringSocketTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A simple test for BeginSend and BeginReceive
        ///</summary>
        [TestMethod()]
        public void Test1()
        {
            new Test1Class().run(4001);
        }

        public class Test1Class
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private ManualResetEvent mre2;
            private String s1;
            private object p1;
            private String s2;
            private object p2;

            // Timeout used in test case
            private static int timeout = 2000000000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);
                    mre2 = new ManualResetEvent(false);

                    // Make two receive requests
                    receiveSocket.BeginReceive(CompletedReceive1, 1);
                    receiveSocket.BeginReceive(CompletedReceive2, 2);

                    // Now send the data.  Hope those receive requests didn't block!
                    String msg = "Hello world\nThis is a test\n";
                    foreach (char c in msg)
                    {
                        sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                    }

                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("Hello world", s1);
                    Assert.AreEqual(1, p1);

                    Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
                    Assert.AreEqual("This is a test", s2);
                    Assert.AreEqual(2, p2);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // This is the callback for the first receive request.  We can't make assertions anywhere
            // but the main thread, so we write the values to member variables so they can be tested
            // on the main thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                s1 = s;
                p1 = payload;
                mre1.Set();
            }

            // This is the callback for the second receive request.
            private void CompletedReceive2(String s, Exception o, object payload)
            {
                s2 = s;
                p2 = payload;
                mre2.Set();
            }

        }

        /// <summary>
        ///A simple test for BeginSend and BeginReceive
        ///</summary>
        [TestMethod()]
        public void Test2()
        {
            new Test2Class().run(4001);
        }

        public class Test2Class
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private String s1;
            private object p1;

            // Timeout used in test case
            private static int timeout = 2000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);

                    // Make two receive requests
                    receiveSocket.BeginReceive(CompletedReceive1, 1);

                    // Now send the data.  Hope those receive requests didn't block!
                    String msg = "Hello world\n";
                    foreach (char c in msg)
                    {
                        sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                    }

                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("Hello world", s1);
                    Assert.AreEqual(1, p1);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // This is the callback for the first receive request.  We can't make assertions anywhere
            // but the main thread, so we write the values to member variables so they can be tested
            // on the main thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                s1 = s;
                p1 = payload;
                mre1.Set();
            }
        }

        /// <summary>
        ///A simple test for BeginSend and BeginReceive
        ///</summary>
        [TestMethod()]
        public void NoNewline()
        {
            new NoNewLineClass().run(4001);
        }

        public class NoNewLineClass
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private String s1;
            private object p1;

            // Timeout used in test case
            private static int timeout = 2000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);

                    // Make two receive requests
                    receiveSocket.BeginReceive(CompletedReceive1, 1);

                    // Now send the data.  Hope those receive requests didn't block!
                    String msg1 = "Hello world";
                    String msg2 = "\nVolleyball\n";

                    sendSocket.BeginSend(msg1, (e, o) => { }, null);
                    sendSocket.BeginSend(msg2, (e, o) => { }, null);
 
                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("Hello world", s1);
                    Assert.AreEqual(1, p1);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // This is the callback for the first receive request.  We can't make assertions anywhere
            // but the main thread, so we write the values to member variables so they can be tested
            // on the main thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                s1 = s;
                p1 = payload;
                mre1.Set();
            }
        }



        /// <summary>
        /// Tim Winchester and Aundrea Hargroder
        ///A test sending 100 strings "0\n" through "100\n" and asserting that the strings are received in the same order.
        ///</summary>
        [TestMethod()]
        public void TW_AH_Test()
        {
            new TW_AH_TestClass().run(4001);
        }

        public class TW_AH_TestClass
        {
            // Data that is shared across threads
            List<String> ReceiveList;
            List<String> CompareList;
            List<String> SendList;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                ReceiveList = new List<string>();
                CompareList = new List<string>();
                SendList = new List<string>();

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // Make receive requests
                    for (int n = 0; n <= 99; n++)
                    {
                        BeginReceieve(receiveSocket, n);
                    }

                    // Now send the data.  Hope those receive requests didn't block!
                    for (int n = 0; n <= 99; n++)
                    {
                        BeginSend(sendSocket, n);
                    }

                    // Wait for the messages to arrive. If you have long sleeps in your code, you should increase this
                    Thread.Sleep(6000);

                    // Make sure the lines were received properly.
                    for (int n = 0; n <= 99; n++)
                    {
                        Assert.AreEqual(CompareList[n], ReceiveList[n]);
                    }
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }

            }

            /// <summary>
            /// Calls BeginSend.  Adds strings to list in order that they are sent.
            /// </summary>
            /// <param name="sendSocket"></param>
            /// <param name="n"></param>
            private void BeginSend(StringSocket sendSocket, int n)
            {
                sendSocket.BeginSend(n.ToString() + "\n", (e, o) => { }, null);
                SendList.Add(n.ToString());
            }

            /// <summary>
            /// Calls BeginReceive and adds strings in order they were received.
            /// </summary>
            /// <param name="receiveSocket"></param>
            /// <param name="n"></param>
            private void BeginReceieve(StringSocket receiveSocket, int n)
            {
                receiveSocket.BeginReceive(ReceiveCallback, null);
                CompareList.Add(n.ToString());
            }

            /// <summary>
            /// Callback created for test. Adds strings to list in order received.
            /// </summary>
            /// <param name="s"></param>
            /// <param name="e"></param>
            /// <param name="payload"></param>
            private void ReceiveCallback(String s, Exception e, Object payload)
            {
                ReceiveList.Add(s);
            }
        }

        //Derek Moore and Nick Martin

        [TestMethod]
        public void SendingAfterReceivingTest()
        {
            new SARTestClass().run(4001);
        }

        /// <summary>
        /// BeginSend and BeginReceive in various ordering
        /// </summary>
        public class SARTestClass
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private String s1;
            private object p1;
            private ManualResetEvent mre2;
            private String s2;
            private object p2;
            private ManualResetEvent mre3;
            private String s3;
            private object p3;
            private ManualResetEvent mre4;
            private String s4;
            private object p4;

            // Timeout used in test case
            private static int timeout = 2000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);
                    mre2 = new ManualResetEvent(false);
                    mre3 = new ManualResetEvent(false);
                    mre4 = new ManualResetEvent(false);

                    // Make two receive request to begin with
                    receiveSocket.BeginReceive(CompletedReceive1, 1);
                    receiveSocket.BeginReceive(CompletedReceive2, 2);

                    // Now send the data.  Hope the receive request didn't block!
                    String msg = "Hello world.\nThis should be fine\neven though there are five messages here."
                        + "\n";
                    sendSocket.BeginSend(msg, (e, o) => { }, null);

                    // Make another receive request
                    receiveSocket.BeginReceive(CompletedReceive3, 3);

                    // Send another line
                    sendSocket.BeginSend("It should send them back\n", (e, o) => { }, null);

                    // Make final receive requests
                    receiveSocket.BeginReceive(CompletedReceive4, 4);

                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("Hello world.", s1);
                    Assert.AreEqual(1, p1);

                    Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
                    Assert.AreEqual("This should be fine", s2);
                    Assert.AreEqual(2, p2);

                    Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
                    Assert.AreEqual("even though there are five messages here.", s3);
                    Assert.AreEqual(3, p3);

                    Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
                    Assert.AreEqual("It should send them back", s4);
                    Assert.AreEqual(4, p4);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // These are the callbacks for the receive request.  We can't make assertions anywhere
            // but the main thread, so we write the values to member variables so they can be tested
            // on the main thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                s1 = s;
                p1 = payload;
                mre1.Set();
            }

            private void CompletedReceive2(string s, Exception e, object payload)
            {
                s2 = s;
                p2 = payload;
                mre2.Set();
            }

            private void CompletedReceive3(string s, Exception e, object payload)
            {
                s3 = s;
                p3 = payload;
                mre3.Set();
            }

            private void CompletedReceive4(string s, Exception e, object payload)
            {
                s4 = s;
                p4 = payload;
                mre4.Set();
            }
        }




        /// <summary>
        /// This test calls a lengthy callback to verify that each
        /// time the user's callback is called, it is on a new thread.
        /// 
        /// Disclaimer: We haven't gotten this to pass with our code yet
        /// because we're not done implementing it, but we believe this should
        /// be a valid test.
        /// 
        /// -Dasha Pruss and Annie Cherkaev
        ///</summary>
        [TestMethod()]
        public void SlowCallbackTest()
        {
            new SlowCallbackClass().run(4001);
        }

        public class SlowCallbackClass
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private ManualResetEvent mre2;
            private ManualResetEvent mre3;
            private String s1;
            private object p1;
            private String s2;
            private object p2;
            private String s3;
            private object p3;

            // Timeout used in test case
            private static int timeout = 2000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);
                    mre2 = new ManualResetEvent(false);
                    mre3 = new ManualResetEvent(false);

                    // Make two receive requests
                    receiveSocket.BeginReceive(CompletedReceive1, 1);
                    receiveSocket.BeginReceive(CompletedReceive2, 2);
                    receiveSocket.BeginReceive(CompletedReceive3, 3);

                    // Now send the data.  Hope those receive requests didn't block!
                    String msg = "So long\nAnd thanks\nFor all the fish!\n";
                    foreach (char c in msg)
                    {
                        sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                    }

                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("So long", s1);
                    Assert.AreEqual(1, p1);

                    Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
                    Assert.AreEqual("And thanks", s2);
                    Assert.AreEqual(2, p2);

                    Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
                    Assert.AreEqual("For all the fish!", s3);
                    Assert.AreEqual(3, p3);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // This is the callback for each receive request. It contains a lengthy loop
            // that will take a significant amount of time to complete. This should confirm
            // that each callback is called on a separate thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                mre1.Set();

                s1 = s;
                p1 = payload;

                int i = (int)p1;

                // Make a loop that will take a significant amount of time to complete
                for (int j = 0; j < Math.Pow(10000, i); j++)
                {
                    for (int k = j; k > 0; k--)
                    {
                        int l = k * j;
                    }
                }
            }

            // This is the callback for each receive request. It contains a lengthy loop
            // that will take a significant amount of time to complete. This should confirm
            // that each callback is called on a separate thread.
            private void CompletedReceive2(String s, Exception o, object payload)
            {
                mre2.Set();
                s2 = s;
                p2 = payload;

                int i = (int)p1;

                // Make a loop that will take a significant amount of time to complete
                for (int j = 0; j < Math.Pow(10000, i); j++)
                {
                    for (int k = j; k > 0; k--)
                    {
                        int l = k * j;
                    }
                }
            }

            // This is the callback for each receive request. It contains a lengthy loop
            // that will take a significant amount of time to complete. This should confirm
            // that each callback is called on a separate thread.
            private void CompletedReceive3(String s, Exception o, object payload)
            {
                mre3.Set();
                s3 = s;
                p3 = payload;

                int i = (int)p1;

                // Make a loop that will take a significant amount of time to complete
                for (int j = 0; j < Math.Pow(10000, i); j++)
                {
                    for (int k = j; k > 0; k--)
                    {
                        int l = k * j;
                    }
                }
            }

        }




        /// <summary>
        /// Send and Receive a very long message at once with newline at end of message.
        /// </summary>
      [TestMethod()]
      public void Test2_TheBaconIpsumTest()
      {
          new BaconTestClass().run(4001);
      }

      public class BaconTestClass
      {
          // Data that is shared across threads
          private ManualResetEvent mre1;
          private String s1;
          private object p1;

          // Timeout used in test case
          private static int timeout = 5000;

          public void run(int port)
          {
              // Create and start a server and client.
              TcpListener server = null;
              TcpClient client = null;

              try
              {
                  server = new TcpListener(IPAddress.Any, port);
                  server.Start();
                  client = new TcpClient("localhost", port);

                  // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                  // method here, which is OK for a test case.
                  Socket serverSocket = server.AcceptSocket();
                  Socket clientSocket = client.Client;

                  // Wrap the two ends of the connection into StringSockets
                  StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                  StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                  // This will coordinate communication between the threads of the test cases
                  mre1 = new ManualResetEvent(false);

                  // Make two receive requests
                  receiveSocket.BeginReceive(CompletedReceive1, 1);

                  // Now send the data.  Hope those receive requests didn't block!
                  String msg = "";

                  // ---- Losaunne and Sam Additional Unit Test shtufffsss ----------------
                  // Uses the StreamReader to read in an additional bacon.txt file that contains
                  // 75 paragraphs of meat (Bacon Ipsum). This will be a stress test because the
                  // code below strips out all line breaks so it will be one single BeginSend
                  // call. A "\n" is tacked on at the end to make sure the BeginReceive method
                  // completes.
                  try
                  {
                      using (StreamReader sr = new StreamReader("bacon.txt"))
                      {
                          while (!sr.EndOfStream)
                          {
                              String temp = sr.ReadLine().Trim();
                              if (temp.Length > 2)
                              {
                                  String tempEnd = temp.Substring(temp.Length - 2);
                                  if (tempEnd == "\n")
                                      temp = temp.Remove(0, temp.Length - 2);
                              }
                              if (temp != "\n")
                                  msg = msg + " " + temp;
                          }
                          msg = msg + "\n";
                          sr.Close();
                      }
                  }
                  catch (Exception e)
                  {
                      msg = "There was an error somewhere (good luck): " + e;
                  }
                  // ---------------------------------------------------------------------


                  sendSocket.BeginSend(msg, (e, o) => { }, null);


                  // Make sure the lines were received properly.
                  Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                  Assert.AreEqual(msg, s1+'\n');
                  Assert.AreEqual(1, p1);
              }
              finally
              {
                  server.Stop();
                  client.Close();
              }
          }

          // This is the callback for the first receive request.  We can't make assertions anywhere
          // but the main thread, so we write the values to member variables so they can be tested
          // on the main thread.
          private void CompletedReceive1(String s, Exception o, object payload)
          {
              s1 = s;
              p1 = payload;
              mre1.Set();
          }
      }

        /// <summary>
        /// Send and receive a very long block of text with a newline at the end
        /// character by character.
        /// </summary>
      [TestMethod()]
      public void BaconCharByCharIpsumTest()
      {
          new BaconCharByCharClass().run(4001);
      }

      public class BaconCharByCharClass
      {
          // Data that is shared across threads
          private ManualResetEvent mre1;
          private String s1;
          private object p1;

          // Timeout used in test case
          private static int timeout = 5000;

          public void run(int port)
          {
              // Create and start a server and client.
              TcpListener server = null;
              TcpClient client = null;

              try
              {
                  server = new TcpListener(IPAddress.Any, port);
                  server.Start();
                  client = new TcpClient("localhost", port);

                  // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                  // method here, which is OK for a test case.
                  Socket serverSocket = server.AcceptSocket();
                  Socket clientSocket = client.Client;

                  // Wrap the two ends of the connection into StringSockets
                  StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                  StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                  // This will coordinate communication between the threads of the test cases
                  mre1 = new ManualResetEvent(false);

                  // Make two receive requests
                  receiveSocket.BeginReceive(CompletedReceive1, 1);

                  // Now send the data.  Hope those receive requests didn't block!
                  String msg = "";

                  // ---- Losaunne and Sam Additional Unit Test shtufffsss ----------------
                  // Uses the StreamReader to read in an additional bacon.txt file that contains
                  // 75 paragraphs of meat (Bacon Ipsum). This will be a stress test because the
                  // code below strips out all line breaks so it will be one single BeginSend
                  // call. A "\n" is tacked on at the end to make sure the BeginReceive method
                  // completes.
                  try
                  {
                      using (StreamReader sr = new StreamReader("bacon.txt"))
                      {
                          while (!sr.EndOfStream)
                          {
                              String temp = sr.ReadLine().Trim();
                              if (temp.Length > 2)
                              {
                                  String tempEnd = temp.Substring(temp.Length - 2);
                                  if (tempEnd == "\n")
                                      temp = temp.Remove(0, temp.Length - 2);
                              }
                              if (temp != "\n")
                                  msg = msg + " " + temp;
                          }
                          msg = msg + "\n";
                          sr.Close();
                      }
                  }
                  catch (Exception e)
                  {
                      msg = "There was an error somewhere (good luck): " + e;
                  }
                  // ---------------------------------------------------------------------

                  foreach (char c in msg)
                  {
                      sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                  }

                  // Make sure the lines were received properly.
                  Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                  Assert.AreEqual(msg, s1 + '\n');
                  Assert.AreEqual(1, p1);
              }
              finally
              {
                  server.Stop();
                  client.Close();
              }
          }

          // This is the callback for the first receive request.  We can't make assertions anywhere
          // but the main thread, so we write the values to member variables so they can be tested
          // on the main thread.
          private void CompletedReceive1(String s, Exception o, object payload)
          {
              s1 = s;
              p1 = payload;
              mre1.Set();
          }
      }

      /// <summary>
      ///Test sending and receiving a short message with frequent new line symbols.
      ///</summary>
      [TestMethod()]
      public void MultipleNewLinesTest()
      {
          new MultipleNewLinesClass().run(4002);
      }

      public class MultipleNewLinesClass
      {
          // Data that is shared across threads
          private ManualResetEvent mre1;
          private ManualResetEvent mre2;
          private ManualResetEvent mre3;
          private ManualResetEvent mre4;
          private String s1;
          private object p1;
          private String s2;
          private object p2;
          private String s3;
          private object p3;
          private String s4;
          private object p4;

          // Timeout used in test case
          private static int timeout = 2000;

          public void run(int port)
          {
              // Create and start a server and client.
              TcpListener server = null;
              TcpClient client = null;

              try
              {
                  server = new TcpListener(IPAddress.Any, port);
                  server.Start();
                  client = new TcpClient("localhost", port);

                  // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                  // method here, which is OK for a test case.
                  Socket serverSocket = server.AcceptSocket();
                  Socket clientSocket = client.Client;

                  // Wrap the two ends of the connection into StringSockets
                  StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                  StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                  // This will coordinate communication between the threads of the test cases
                  mre1 = new ManualResetEvent(false);
                  mre2 = new ManualResetEvent(false);
                  mre3 = new ManualResetEvent(false);
                  mre4 = new ManualResetEvent(false);

                  // Make two receive requests
                  receiveSocket.BeginReceive(CompletedReceive1, 1);
                  receiveSocket.BeginReceive(CompletedReceive2, 2);
                  receiveSocket.BeginReceive(CompletedReceive3, 3);
                  receiveSocket.BeginReceive(CompletedReceive4, 4);

                  // Now send the data.  Hope those receive requests didn't block!
                  String msg = "t3st\nThis is a test\n\nThis is a test\n";
                  sendSocket.BeginSend(msg, (e, o) => { }, null);     //Here we send a long message as opposed to the individual chars in the original test.
                  //We will actually be sending multiple "messages" because we will sending multiple '\n' delimeters, because of
                  //this we will need to call the same amount of .beginReceives as the number of messages sent.


                  // Make sure the lines were received properly.
                  Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                  Assert.AreEqual("t3st", s1);
                  Assert.AreEqual(1, p1);

                  Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
                  Assert.AreEqual("This is a test", s2);
                  Assert.AreEqual(2, p2);

                  Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");    //This part of the test ensures that a SS can send and receive a blank string (""). 
                  Assert.AreEqual("", s3);                                                //A poorly implemented SS could encounter strange end-behavior from this.
                  Assert.AreEqual(3, p3);

                  Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
                  Assert.AreEqual("This is a test", s4);
                  Assert.AreEqual(4, p4);
              }
              finally
              {
                  server.Stop();
                  client.Close();
              }
          }

          // This is the callback for the first receive request.  We can't make assertions anywhere
          // but the main thread, so we write the values to member variables so they can be tested
          // on the main thread.
          private void CompletedReceive1(String s, Exception o, object payload)
          {
              s1 = s;
              p1 = payload;
              mre1.Set();
          }

          // This is the callback for the second receive request.
          private void CompletedReceive2(String s, Exception o, object payload)
          {
              s2 = s;
              p2 = payload;
              mre2.Set();
          }

          // This is the callback for the third receive request.
          private void CompletedReceive3(String s, Exception o, object payload)
          {
              s3 = s;
              p3 = payload;
              mre3.Set();
          }

          // This is the callback for the fourth receive request.
          private void CompletedReceive4(String s, Exception o, object payload)
          {
              s4 = s;
              p4 = payload;
              mre4.Set();
          }
      }

      /// <summary>
      ///Test sending an empty message.
      ///</summary>
      [TestMethod()]
      public void NullMessageTest()
      {
          new NullMessageTestClass().run(4001);
      }

      public class NullMessageTestClass
      {
          // Data that is shared across threads
          private ManualResetEvent mre1;
          private String s1;
          private object p1;

          // Timeout used in test case
          private static int timeout = 2000;

          public void run(int port)
          {
              // Create and start a server and client.
              TcpListener server = null;
              TcpClient client = null;

              try
              {
                  server = new TcpListener(IPAddress.Any, port);
                  server.Start();
                  client = new TcpClient("localhost", port);

                  // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                  // method here, which is OK for a test case.
                  Socket serverSocket = server.AcceptSocket();
                  Socket clientSocket = client.Client;

                  // Wrap the two ends of the connection into StringSockets
                  StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                  StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                  // This will coordinate communication between the threads of the test cases
                  mre1 = new ManualResetEvent(false);

                  // Make two receive requests
                  receiveSocket.BeginReceive(CompletedReceive1, 1);

                  // Now send the data.  Hope those receive requests didn't block!
                  String msg = "\n";
                  foreach (char c in msg)
                  {
                      sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                  }

                  // Make sure the lines were received properly.
                  Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                  Assert.AreEqual("", s1);
                  Assert.AreEqual(1, p1);
              }
              finally
              {
                  server.Stop();
                  client.Close();
              }
          }

          // This is the callback for the first receive request.  We can't make assertions anywhere
          // but the main thread, so we write the values to member variables so they can be tested
          // on the main thread.
          private void CompletedReceive1(String s, Exception o, object payload)
          {
              s1 = s;
              p1 = payload;
              mre1.Set();
          }
      }

        /// <summary>
        /// Try to send each line of the bacon text as a new message for a stress test.
        /// </summary>
      [TestMethod()]
      public void BaconStressTest()
      {
          new BaconStressClass().run(4001);
      }

      public class BaconStressClass
      {
          // Data that is shared across threads
          private ManualResetEvent mre1;
          private String s1;
          private object p1;

          // Timeout used in test case
          private static int timeout = 5000;

          public void run(int port)
          {
              // Create and start a server and client.
              TcpListener server = null;
              TcpClient client = null;

              try
              {
                  server = new TcpListener(IPAddress.Any, port);
                  server.Start();
                  client = new TcpClient("localhost", port);

                  // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                  // method here, which is OK for a test case.
                  Socket serverSocket = server.AcceptSocket();
                  Socket clientSocket = client.Client;

                  // Wrap the two ends of the connection into StringSockets
                  StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                  StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                  // This will coordinate communication between the threads of the test cases
                  mre1 = new ManualResetEvent(false);

                  // Make two receive requests

                  // Now send the data.  Hope those receive requests didn't block!
                  String msg = "";

                  //Read each line of the text file until the string is larger than 1024 characters
                  //then Receive and Send each string as a new message until the end of the file.
                  try
                  {
                      using (StreamReader sr = new StreamReader("bacon.txt"))
                      {
                          while (!sr.EndOfStream)
                          {
                              String temp = msg + sr.ReadLine().Trim();
                              if (temp.Length > 1024)
                              {
                                  receiveSocket.BeginReceive(CompletedReceive1, 1);
                                  sendSocket.BeginSend(temp+"\n", (e, o) => { }, null);
                                  msg = "";
                              }
                              else
                                  msg = msg + temp;
                          }
                          sr.Close();
                      }
                  }
                  catch (Exception e)
                  {
                      msg = "There was an error somewhere (good luck): " + e;
                  }
                  // ---------------------------------------------------------------------


                  


                  // Make sure the lines were received properly.
                  Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                  Assert.AreEqual(1, p1);
              }
              finally
              {
                  server.Stop();
                  client.Close();
              }
          }

          // This is the callback for the first receive request.  We can't make assertions anywhere
          // but the main thread, so we write the values to member variables so they can be tested
          // on the main thread.
          private void CompletedReceive1(String s, Exception o, object payload)
          {
              s1 = s;
              p1 = payload;
              mre1.Set();
          }
      }
    }
}
