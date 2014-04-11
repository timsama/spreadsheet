using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BB;
using CustomNetworking;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace BC
{
    public class BoggleServer
    {
        // declare events we can raise for the Game class
        public event EventHandler<GetWordEventArgs> GetWordEvent;
        public event EventHandler<IgnoreCommandEventArgs> IgnoreCommandEvent;
        public event EventHandler<DisconnectedPlayerEventArgs> DisconnectedPlayerEvent;

        // TcpListener to listen for incoming connections
        private TcpListener server;

        // Locking object across all sockets
        private Object LockSockets;

        // A dictionary of ongoing games
        private Dictionary<int, Game> GameSet;

        // A queue of unpaired players
        private Queue<Player> WeWantToPlay;

        public readonly int GameLength;
        private readonly HashSet<string> boggleDictionary;
        readonly String boardFace;

        /// <summary>
        /// Starts a BoggleServer
        /// </summary>
        /// <param name="args">Game duration (sec), Dictionary file, [Starting boardface]</param>
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                int time;
                int.TryParse(args[0], out time);

                // if a valid boardface was given, use it. Otherwise, start with a random boardface
                if ((args.Length == 3) && (args[2].Length == 16))
                {
                    new BoggleServer(time, args[1], args[2]);
                }
                else
                {
                    new BoggleServer(time, args[1]);
                }

                // wait for the user to input something to keep the console window open
                Console.ReadLine();
            }
            else
            {
                // no arguments were given, return help information
                Console.Clear();
                Console.WriteLine("BoggleServer requires arguments to run from the command line.\n" +
                "Use the following format to enter arguments:\n" +
                "BoggleServer <Game duration in seconds> <Dictionary file name> <Starting Board Face (Optional)>\n" +
                "Example: BoggleServer 60 \"boggle_words.txt\"");
            }
        }

        /// <summary>
        /// Initializes the server to a game length (in seconds) and a dictionary to compare words against
        /// </summary>
        /// <param name="_gameLength"></param>
        /// <param name="dictionary_path"></param>
        /// <param name="_boardFace"></param>
        public BoggleServer(int _gameLength, String dictionary_path)
        {
            // set the gamelength to the input game length (in seconds)
            GameLength = _gameLength;

            // build boggleDictionary from given file
            boggleDictionary = BuildDictionaryFromPath(dictionary_path);

            // initialize the server and set the port to 2000
            server = new TcpListener(IPAddress.Any, 2000);

            // initialize locking object
            LockSockets = new Object();

            // initialize player queue
            WeWantToPlay = new Queue<Player>();

            // initialize the game set
            GameSet = new Dictionary<int, Game>();

            // start listening
            server.Start();
            server.BeginAcceptSocket(PortConnection, null);

            // ignore invalid messages at any point
            IgnoreCommandEvent += Ignore;

            // handle disconnected players
            DisconnectedPlayerEvent += Terminate;
        }


        /// <summary>
        /// Initializes the server to a game length (in seconds), a dictionary to compare words against, and a specific board configuration
        /// </summary>
        /// <param name="_gameLength"></param>
        /// <param name="dictionary_path"></param>
        /// <param name="_boardFace"></param>
        public BoggleServer(int _gameLength, String dictionary_path, String _boardFace)
            : this(_gameLength, dictionary_path)
        {
            boardFace = _boardFace;
        }

        /// <summary>
        /// Deals with clients requesting a connection to the server on port 2000
        /// For every two clients that connect, pair them in a game
        /// </summary>
        /// <param name="ar"></param>
        private void PortConnection(IAsyncResult ar)
        {
            Socket socket = server.EndAcceptSocket(ar);
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);
            ss.BeginReceive(NameReceived, ss);
            server.BeginAcceptSocket(PortConnection, null);
        }

        /// <summary>
        /// Deals with lines of text as they arrive at the server.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void NameReceived(string s, Exception e, object payload)
        {
            Console.WriteLine(s);

            // convert the message to all uppercase
            s = s.ToUpper();

            // check what s is and act accordingly
            if (s.Substring(0, 4) == "PLAY")
            {
                // put the requesting player in a queue
                WeWantToPlay.Enqueue(new Player(s.Substring(5), payload));

                // lock when dequeueing players
                lock (LockSockets)
                {
                    // if queue's size is > 1, pair players and check again
                    while (WeWantToPlay.Count > 1)
                    {
                        // pair the players
                        Player p1 = WeWantToPlay.Dequeue();
                        Player p2 = WeWantToPlay.Dequeue();
                        
                        // create a new game and set up its event handling
                        Game tempGame = new Game(GameLength, p1, p2, boardFace, this);
                        GetWordEvent += tempGame.OnGetWord;

                        // add the game to the gameset
                        GameSet.Add(((StringSocket)p1.Payload).GetHashCode(), tempGame);
                        GameSet.Add(((StringSocket)p2.Payload).GetHashCode(), tempGame);
                    }

                    // start listening for new message inputs
                    ((StringSocket)payload).BeginReceive(MessageReceived, payload);
                }
            }
        }

        /// <summary>
        /// Deals with lines of text as they arrive at the server.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void MessageReceived(string s, Exception e, object payload)
        {
            Console.WriteLine(s);

            // convert the message to all uppercase
            s = s.ToUpper();

            // lock when using the GameSet
            lock (LockSockets)
            {
                // try to get the game associated with this player
                Game currentGame;
                GameSet.TryGetValue(payload.GetHashCode(), out currentGame);

                // check what s is and act accordingly
                if ((s.Length >= 5) && (s.Substring(0, 4) == "WORD"))
                {
                    // set up and raise the GetWordEvent
                    if (GetWordEvent != null)
                    {
                        GetWordEvent(this, new GetWordEventArgs(s.Substring(5), ((StringSocket)payload).GetHashCode()));
                    }
                }
                else if (e is DisconnectedException)
                {
                    // raise a DisconnectedPlayerEvent using the disconnected player's opponent
                    DisconnectedPlayerEvent(this, new DisconnectedPlayerEventArgs(currentGame.GetPlayer(((StringSocket)payload).GetHashCode()).Opponent.Payload));
                }
                else
                {
                    // the input doesn't conform to any of the commands we recognize
                    IgnoreCommandEvent(this, new IgnoreCommandEventArgs(currentGame.GetPlayer(((StringSocket)payload).GetHashCode())));
                }
            
                // keep listening for new message inputs
                ((StringSocket)payload).BeginReceive(MessageReceived, payload);
            }
        }

        /// <summary>
        /// Builds a HashSet of strings from a file representing the dictionary of legal Boggle words
        /// </summary>
        /// <param name="dictionary_path"></param>
        /// <returns></returns>
        public HashSet<string> BuildDictionaryFromPath(string dictionary_path)
        {
            HashSet<string> temp = new HashSet<string>();
            // The file will have one word in the dictionary on every line
            // Add each word in the file to the boggleDictionary
            try
            {
                using (StreamReader sr = new StreamReader(dictionary_path))
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

        /// <summary>
        /// Sends the begin information to the players
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void BeginGame(Game game)
        {
            // send out START commands to the two players
            SendToPlayer(game.P1, "START " + game.bBoard.ToString() + " " + GameLength + " " + game.P2.Name + "\n");
            SendToPlayer(game.P2, "START " + game.bBoard.ToString() + " " + GameLength + " " + game.P1.Name + "\n");
        }

        /// <summary>
        /// Sends the current countdown information to the players
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="secondsRemaining"></param>
        public void Countdown(Player p1, Player p2, int secondsRemaining)
        {
            // send the time remaining in the game to each player
            SendToPlayer(p1, "TIME " + secondsRemaining + "\n");
            SendToPlayer(p2, "TIME " + secondsRemaining + "\n");

        }

        /// <summary>
        /// Sends a score update to the players
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p1score"></param>
        /// <param name="p2score"></param>
        public void UpdateScore(Player p1, Player p2, int p1score, int p2score)
        {
            // send the updated score to the two players
            SendToPlayer(p1, "SCORE " + p1score + " " + p2score + "\n");
            SendToPlayer(p2, "SCORE " + p2score + " " + p1score + "\n");
        }

        /// <summary>
        /// Helper function to send a message to a player
        /// </summary>
        /// <param name="p"></param>
        /// <param name="s"></param>
        private void SendToPlayer(Player p, String s)
        {
            // try to send the string to the player
            try
            {
                ((StringSocket)p.Payload).BeginSend(s, callback, p.Payload);
            }
            catch (SocketException)
            {
                DisconnectedPlayerEvent(this, new DisconnectedPlayerEventArgs((StringSocket)p.Opponent.Payload));
            }
        }

        /// <summary>
        /// Sends a termination message when one player has disconnected
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void Terminate(object caller, DisconnectedPlayerEventArgs e)
        {
            try
            {
                // send the terminated message to the disconnected player's opponent
                e.Socket.BeginSend("TERMINATED\n", callback, e.Socket);
                
                //now close the surviving socket
                System.Threading.Thread.Sleep(50);
                e.Socket.CloseSocket();
            }
            catch (SocketException)
            {
                // both players disconnected, catch error so server doesn't crash
            }
        }

        /// <summary>
        /// Responds to an illegal command from a player
        /// </summary>
        /// <param name="p"></param>
        public void Ignore(Object caller, IgnoreCommandEventArgs e)
        {
            // send a response to an illegal message to the player
            SendToPlayer(e.Player, "IGNORING\n");
        }

        /// <summary>
        /// Sends the final score and game summary to the players
        /// </summary>
        /// <param name="game"></param>
        public void EndGame(Game game)
        {
            // send the final score update
            UpdateScore(game.P1, game.P2, game.CalculateScore(game.P1), game.CalculateScore(game.P2));

            try
            {
                // send the game summary to player 1
                ((StringSocket)game.P1.Payload).BeginSend("STOP " + GetString(game.P1.LegalWords) + " "
                    + GetString(game.P2.LegalWords) + " "
                    + GetString(game.BothLegalWords) + " "
                    + GetString(game.P1.IllegalWords) + " "
                    + GetString(game.P2.IllegalWords) + "\n", ((x, s) => CloseSocketCallback(null, game.P1.Payload)), game.P1.Payload);

                // send the game summary to player 2
                ((StringSocket)game.P2.Payload).BeginSend("STOP " + GetString(game.P2.LegalWords) + " "
                    + GetString(game.P1.LegalWords) + " "
                    + GetString(game.BothLegalWords) + " "
                    + GetString(game.P2.IllegalWords) + " "
                    + GetString(game.P1.IllegalWords) + "\n", ((x, s) => CloseSocketCallback(null, game.P2.Payload)), game.P2.Payload);
            }
            catch (SocketException)
            {
                // catch exception if a client disconnected and do nothing
            }

            // remove this game from the set of active games. Only the first statement is needed,
            // but for the sake of thoroughness, both are called
            GameSet.Remove(game.P1.Payload.GetHashCode());
            GameSet.Remove(game.P2.Payload.GetHashCode());

        }

        /// <summary>
        /// Returns a count of the items in a list, and then enumerates the list. Everything is returned in a single string
        /// </summary>
        /// <param name="list">A list of strings comprising the set we want to convert to string</param>
        /// <returns></returns>
        private string GetString(IEnumerable<string> list)
        {
            int count = 0;
            string returnString = "";

            // if the list is empty, return a count of zero
            if (list != null)
            {
                // increase the count and add each item in the set to the return list
                foreach (string item in list)
                {
                    count++;
                    returnString += " " + item;
                }
            }

            // return the count as a string, followed by each item in the set
            return count.ToString() + returnString;
        }

        /// <summary>
        /// Returns whether or not a string is contained in the Boggle Dictionary.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool IsInDictionary(String s)
        {
            return boggleDictionary.Contains(s);
        }

        /// <summary>
        /// Unimplemented delegate to use as a placeholder
        /// </summary>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void callback(Exception e, object payload)
        {
            //System.Threading.Thread.Sleep(2000);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Callback used when the a socket disconnects and the sockets need to be closed
        /// </summary>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void CloseSocketCallback(Exception e, object payload)
        {
            // close the socket
            //System.Threading.Thread.Sleep(5000);
            ((StringSocket)payload).CloseSocket();
            //TODO: Figure out how to not use a sleep here
        }

        /// <summary>
        /// Method used to close the server
        /// </summary>
        public void CloseServer()
        {
            // close the socket
            //System.Threading.Thread.Sleep(5000);
            this.server.Stop();
        }
    }


    /// <summary>
    /// Represents an event where a command to be ignored has been entered
    /// </summary>
    public class IgnoreCommandEventArgs : EventArgs
    {
        public readonly Player Player;

        /// <summary>
        /// Sets the socket object, then calls the base constructor
        /// </summary>
        /// <param name="_socket"></param>
        public IgnoreCommandEventArgs(Player _player)
            : base()
        {
            Player = _player;
        }
    }

    /// <summary>
    /// Represents an event where a word was received from a player
    /// </summary>
    public class GetWordEventArgs : EventArgs
    {
        public readonly String Word;
        public readonly int Hashcode;

        /// <summary>
        /// Sets the word and player information, then calls the base constructor
        /// </summary>
        /// <param name="_word"></param>
        /// <param name="_player"></param>
        public GetWordEventArgs(String _word, int _hashcode)
            : base()
        {
            Word = _word;
            Hashcode = _hashcode;
        }
    }

    /// <summary>
    /// Represents an event where a player has disconnected
    /// </summary>
    public class DisconnectedPlayerEventArgs : EventArgs
    {
        public readonly StringSocket Socket;

        /// <summary>
        /// Sets the socket of the disconnected player, then calls the base constructor
        /// </summary>
        /// <param name="_socket"></param>
        public DisconnectedPlayerEventArgs(object _socket)
            : base()
        {
            Socket = (StringSocket)_socket;
        }
    }


}
