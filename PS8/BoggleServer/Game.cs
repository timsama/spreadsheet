using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BB;

namespace BC
{
    /// <summary>
    /// Represents an ongoing game of Boggle
    /// </summary>
    public class Game
    {
        // create a Timer to count the remaining seconds of game
        Timer countdown = new Timer();
        // create a Timer to end the game when time runs out
        Timer gameEnd = new Timer();

        // keeps track of the start time of the game
        int startTime;

        // length of game in seconds
        int length;

        // Handle of the game server
        BoggleServer bServer;

        // BoggleBoard to represent the state of the gameboard
        public BoggleBoard bBoard { get; private set; }

        // Players 1 and 2
        public readonly Player P1;
        public readonly Player P2;

        // Set of legal words both players have entered
        public HashSet<string> BothLegalWords { get; private set; }

        /// <summary>
        /// Constructor to start a new game
        /// </summary>
        /// <param name="_length">Length of the game, in seconds</param>
        /// <param name="_p1">Player 1 object</param>
        /// <param name="_p2">Player 2 object</param>
        /// <param name="BoardFace">Configuration of the board face</param>
        public Game(int _length, Player _p1, Player _p2, String BoardFace, BoggleServer _bServer)
        {
            length = _length;
            P1 = _p1;
            P2 = _p2;
            startTime = (int)DateTime.Now.TimeOfDay.TotalSeconds;

            // check for null or empty boardface
            if (BoardFace == null || BoardFace == "")
            {
                bBoard = new BoggleBoard();
            }
            else
            {
                bBoard = new BoggleBoard(BoardFace);
            }

            // set the calling server to the passed-in server
            bServer = _bServer;

            // set the players as each others' opponents
            P1.Opponent = P2;
            P2.Opponent = P1;

            BothLegalWords = new HashSet<string>();

            // start the game on the server
            bServer.BeginGame(this);

            // stop the timers if a player gets disconnected
            bServer.DisconnectedPlayerEvent += StopTimers;

            // count down and execute every second
            countdown = new Timer(1000);
            countdown.Elapsed += Countdown;
            countdown.Start();

            // count down to the end of the game (convert from seconds to milliseconds)
            gameEnd = new Timer(bServer.GameLength * 1000);
            gameEnd.Elapsed += EndGame;
            gameEnd.Start();
        }

        /// <summary>
        /// Event handler that is called every second to update timers for the players
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Countdown(object source, ElapsedEventArgs e)
        {
            // send the current countdown information to the players
            bServer.Countdown(P1, P2, length + (startTime - (int)e.SignalTime.TimeOfDay.TotalSeconds));
        }

        /// <summary>
        /// Event handler for when the game timer runs out
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void EndGame(object source, ElapsedEventArgs e)
        {
            // send a game summary to the players and unregisters time-based events
            bServer.EndGame(this);
            countdown.Elapsed -= Countdown;
            gameEnd.Elapsed -= EndGame;
        }

        /// <summary>
        /// Event handler to stop timers when a player is disconnected
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void StopTimers(object source, DisconnectedPlayerEventArgs e)
        {
            // stop the timers so messages do not continue to be sent
            countdown.Elapsed -= Countdown;
            gameEnd.Elapsed -= EndGame;
        }

        /// <summary>
        /// Event handler for getting a word from a player
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnGetWord(object source, GetWordEventArgs e)
        {
            // get the player from the hashcode
            Player thisPlayer = GetPlayer(e.Hashcode);

            // if the player is in another game, we don't need to do anything
            if (thisPlayer == null)
                return;

            // all countable words must be at least 3 characters long
            if (e.Word.Length < 3)
                return;

            // check to see if the player's word can be formed on the current board face and is contained in the Boggle dictionary
            if (bBoard.CanBeFormed(e.Word) && bServer.IsInDictionary(e.Word))
            {
                // if we're here, the word is valid. Determine which list to put it in
                if (thisPlayer.Opponent.LegalWords.Contains(e.Word))
                {
                    // word is in both players' sets, so remove it from the opponent and add it to the both players set
                    BothLegalWords.Add(e.Word);
                    thisPlayer.Opponent.LegalWords.Remove(e.Word);
                }
                else
                {
                    // word is only in this player's list, so add it to their legal words set
                    thisPlayer.LegalWords.Add(e.Word);
                }
            }
            else
            {
                // word is invalid or not in dictionary, add it to illegal words set
                thisPlayer.IllegalWords.Add(e.Word);
            }

            // send updated scores to the players
            bServer.UpdateScore(P1, P2, CalculateScore(P1), CalculateScore(P2));
        }

        /// <summary>
        /// Returns a players score by calculating the points for legal words and then subtracting points for illegal words
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int CalculateScore(Player p)
        {
            return p.LegalWords.GetScore() - p.IllegalWords.Count;
        }

        /// <summary>
        /// Returns the player associated with a given hashcode (obtained from the player's StringSocket.GetHashCode())
        /// </summary>
        /// <param name="hashcode"></param>
        /// <returns></returns>
        public Player GetPlayer(int hashcode)
        {
            // check which player the hashcode is associated with and return it. Returns null if a player not in this game is specified.
            if (P1.Payload.GetHashCode() == hashcode)
            {
                return P1;
            }
            else if (P2.Payload.GetHashCode() == hashcode)
            {
                return P2;
            }
            else
            {
                return null;
            }
        }

        /*
         Event handlers
         BeginGame -- Send START to each client (done)
         OnGetWord -- Game got a word from player 1/2 (done)
         OnDisconnect -- A player disconnected prematurely
         OnIllegalMessage -- A client sent a bad message
         Countdown -- Sends TIME to each client (done)
         EndGame -- When the timer runs out (done)
         * 
         * 
         * 
         Helper methods
         UpdateScore -- Changes score, and sends SCORE to each client (when score changes)
         Summary -- Sends STOP to each client (at end of game)
         CheckWord -- Check for vailidity with the gameboard, and then with the dictionary
                  
         */
    }

    /// <summary>
    /// Represents a player of a boggle game
    /// </summary>
    public class Player
    {
        // Player's name
        public readonly String Name;

        // Opponent Player
        public Player Opponent { get; set; }

        // a unique-identifier for the player
        public readonly Object Payload;

        // lists of the legal and illegal words the player has entered
        public LegalWordSet LegalWords;
        public HashSet<string> IllegalWords;

        public Player(String _name, Object _payload)
        {
            Name = _name;
            Payload = _payload;
            LegalWords = new LegalWordSet();
            IllegalWords = new HashSet<string>();
        }
    }

    /// <summary>
    /// Represents a set of Legal Words
    /// </summary>
    public class LegalWordSet : HashSet<String>
    {
        private int score;

        /// <summary>
        /// Constructor for a set of legal words
        /// </summary>
        public LegalWordSet()
        {
            score = 0;
        }

        /// <summary>
        /// Returns the current total score of all of the words in the set
        /// </summary>
        /// <returns></returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        /// Overrides the base Add of HashSet to keep a running total of the score of all words in the set
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public new bool Add(String s)
        {
            bool AddedToSet = base.Add(s);

            // if the word is unique, then add a score for it to the total
            if (AddedToSet)
            {
                // score is dependent on the length of the word
                if (s.Length < 5)
                    score += 1;
                else if (s.Length == 5)
                    score += 2;
                else if (s.Length == 6)
                    score += 3;
                else if (s.Length == 7)
                    score += 5;
                else
                    score += 11;
            }

            return AddedToSet;
        }

        /// <summary>
        /// Overrides the base Remove of HashSet to keep a running total of the score of all words in the set
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public new bool Remove(String s)
        {
            bool RemovedFromSet = base.Remove(s);

            // if the word was in the set, subtract its score from the total
            if (RemovedFromSet)
            {
                // score is dependent on the length of the word
                if (s.Length < 5)
                    score -= 1;
                else if (s.Length == 5)
                    score -= 2;
                else if (s.Length == 6)
                    score -= 3;
                else if (s.Length == 7)
                    score -= 5;
                else
                    score -= 11;
            }

            return RemovedFromSet;
        }
    }
}
