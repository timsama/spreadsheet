using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BC;
using System.Net.Sockets;

namespace ModelUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RunTests()
        {
            ConnectionTest();
            LostConnectionTest();
        }
        
        public void ConnectionTest()
        {
            // start the boggle server
            BoggleServer bs = new BoggleServer(20, "boggle_words.txt", "ABCDEFGHIJKLMNOP");
            
            // initialize two client models
            BoggleClientModel model1 = new BoggleClientModel();
            BoggleClientModel model2 = new BoggleClientModel();
            
            // set up event handling for the first line
            model1.IncomingLineEvent += GetStart;

            // set up event handling for time statements
            model1.IncomingLineEvent += GetTime;

            // set up event handling for a sent message
            model1.IncomingLineEvent += CheckMessage;

            // set up event handling for a dropped connection
            model1.ConnectionLostEvent += DroppedConnection;

            // connect the two client models
            model1.Connect("localhost", 2000, "TEST1");
            model2.Connect("localhost", 2000, "TEST2");
            
            // wait for 3 seconds for a connection
            System.Threading.Thread.Sleep(3000);

            // send a word we know will be rejected
            model1.SendMessage("WORD SDFDSHSGFDSFsdfsdf");

            // wait for 3 seconds for a connection
            System.Threading.Thread.Sleep(3000);

            // close the server
            bs.CloseServer();
        }

        public void LostConnectionTest()
        {
            // start the boggle server
            BoggleServer bs = new BoggleServer(20, "boggle_words.txt", "ABCDEFGHIJKLMNOP");

            // initialize two client models
            BoggleClientModel model1 = new BoggleClientModel();
            BoggleClientModel model2 = new BoggleClientModel();

            // set up event handling for the first line
            model1.IncomingLineEvent += GetStart;

            // set up event handling for time statements
            model1.IncomingLineEvent += GetTime;

            // set up event handling for a sent message
            model1.IncomingLineEvent += CheckMessage;

            // set up event handling for a dropped connection
            model1.ConnectionLostEvent += DroppedConnection;

            // connect the two client models
            model1.Connect("localhost", 2000, "TEST1");
            model2.Connect("localhost", 2000, "TEST2");

            // simulate a dropped server connection
            bs.CloseServer();

            // wait for 3 seconds for a connection
            System.Threading.Thread.Sleep(3000);

            // send a word we know will be rejected
            model1.SendMessage("WORD SDFDSHSGFDSFsdfsdf");

            // wait for 3 seconds for a connection
            System.Threading.Thread.Sleep(3000);
        }

        public void GetStart(String s)
        {
            Assert.AreEqual("START ABCDEFGHIJKLMNOP 20 TEST2", s);
        }

        public void GetTime(String s)
        {
            if (!s.Contains("START") && !s.Contains("SCORE") && !s.Contains("STOP"))
                Assert.IsTrue(s.Contains("TIME"));
        }

        public void CheckMessage(String s)
        {
            if (!s.Contains("TIME") && !s.Contains("START") && !s.Contains("STOP"))
                Assert.AreEqual("SCORE -1 0", s);
        }

        public void DroppedConnection()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void NoConnectionTest()
        {
            // initialize two client models
            BoggleClientModel model1 = new BoggleClientModel();

            model1.NoServerConnectionEvent += NoConnection;

            // connect the two client models
            model1.Connect("localhost", 2000, "TEST1");
            
            // wait for 3 seconds for a connection
            System.Threading.Thread.Sleep(3000);
        }

        public void NoConnection()
        {
            Assert.IsTrue(true);
        }
    }
}
