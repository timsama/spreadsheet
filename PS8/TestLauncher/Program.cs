using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using System.Threading;
using BC;

namespace BC
{
    class Program
    {
        /// <summary>
        /// Launches a chat server and two chat clients
        /// </summary>
        static void Main(string[] args)
        {
            new BoggleServer(25, "boggle_words.txt");
            new Thread(() => BoggleClientView.Main()).Start();
        }
    }
}
