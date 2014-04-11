using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using BC;

namespace BoggleClient
{
    /// <summary>
    /// Interaction logic for BoggleGameWindow
    /// </summary>
    public partial class BoggleGameWindow : Window
    {
        BoggleClientModel model;
        MainWindow hWnd;
        List<System.Windows.Controls.Button> Buttons;
        string wordplay;
        string p1RunningScore;
        string p2RunningScore;
        string p1Name;
        string p2Name;

        /// <summary>
        /// Constructor for Boggle Game Window
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="playerName"></param>
        /// <param name="ipAddress"></param>
        public BoggleGameWindow(MainWindow _hWnd, String playerName, String ipAddress)
        {
            // initialize the game window
            InitializeComponent();
            
            // hold the main window handle
            hWnd = _hWnd;

            // set up the client model and event handling
            model = new BoggleClientModel();
            model.IncomingLineEvent += MessageReceived;
            model.ConnectionLostEvent += HandleLostConnection;
            model.NoServerConnectionEvent += HandleNoServerConnection;
            
            // if the player name is empty set it to the default "you"
            if (playerName == "")
                playerName = "YOU";

            // connect to the server
            model.Connect(ipAddress, 2000, playerName);
            
            // set the player name
            p1_name_label.Dispatcher.BeginInvoke(new Action(() => { p1_name_label.Text = playerName.ToUpper(); }));
            p1Name = playerName;
            
            // create the button list
            Buttons = CreateButtonList();
        }

        /// <summary>
        /// Handles received messages from the game server
        /// </summary>
        /// <param name="s"></param>
        public void MessageReceived(string s)
        {
            // make everything uppercase
            s = s.ToUpper();

            // split the string into chunks delimited by whitespace
            char[] splitChars = { ' ', '\n', '\r' };
            string[] parts = s.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            // perform different actions depending on the received string
            if ((parts.Length == 4) && (parts[0] == "START"))
            {
                // show this window and hide the connection window
                this.Dispatcher.BeginInvoke(new Action(() => { this.Show(); }));
                hWnd.Dispatcher.BeginInvoke(new Action(() => { hWnd.Hide(); }));

                // initialize the event that waits for an entered word
                enter_word_button.PreviewMouseUp += EnterWord;
                clear_word_button.PreviewMouseUp += ClearWord;
                
                // initialize the event that waits for a clicked boggle board letter button
                foreach (System.Windows.Controls.Button b in Buttons)
                    b.Click += LetterEntered;

                // show this form now that the game will begin
                this.Dispatcher.BeginInvoke(new Action(() => this.Show()));

                // parts[1] is the boardface
                // display the boardface
                string boardface = parts[1];

                // initialize num for the foreach loop
                int num = 0;

                // update the buttons displaying the boardface and check for q's in the boardface
                foreach (System.Windows.Controls.Button b in Buttons)
                {
                    if (boardface[num] != 'Q')
                    {
                        b.Dispatcher.Invoke(new Action(() => { b.Content = boardface[num]; }));
                    }
                    // if there is a q display qu
                    else
                    {
                        b.Dispatcher.Invoke(new Action(() => { b.Content = "Qu"; }));
                    }
                    num++;
                }


                // parts[2] is the game length
                //display game length
                time_count_label.Dispatcher.BeginInvoke(new Action(() => { time_count_label.Text = parts[2]; }));
                
                // parts[3] is the opponent's name
                // if the opponent's name is 'you' make it 'enemy'
                p2Name = parts[3];
                if (p2Name == "YOU")
                    p2Name = "ENEMY";
                p2_name_label.Dispatcher.BeginInvoke(new Action(() => { p2_name_label.Text = p2Name; }));
                
            }
            else if ((parts.Length == 2) && (parts[0] == "TIME"))
            {
                // display the new time
                time_count_label.Dispatcher.BeginInvoke(new Action(() => { time_count_label.Text = parts[1]; }));
            }
            else if ((parts.Length == 3) && (parts[0] == "SCORE"))
            {
                // display the new scores
                p1_score_count_label.Dispatcher.Invoke(new Action(() => { p1_score_count_label.Text = parts[1]; }));
                p2_score_count_label.Dispatcher.Invoke(new Action(() => { p2_score_count_label.Text = parts[2]; }));
                p1RunningScore = parts[1];
                p2RunningScore = parts[2];
            }
            else if ((parts.Length == 1) && (parts[0] == "TERMINATED"))
            {
                // display a message to the user that the game has been interrupted (play again? YES/NO)
                DialogResult result = System.Windows.Forms.MessageBox.Show("The game has been terminated by the other user.  Would you like to play again?","GAME TERMINATED", System.Windows.Forms.MessageBoxButtons.YesNo);

                // if the user wants to play again restart the boggle client
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // start the boggle client again
                    hWnd.Show();
                    hWnd.EnablePlayButton();
                    this.Hide();
                }
                else
                {
                    //quit
                    hWnd.Dispatcher.Invoke(new Action(() => { hWnd.Close(); }));
                    this.Dispatcher.Invoke(new Action(() => { this.Close(); }));
                }

            }
            else if ((parts.Length > 5) && (parts[0] == "STOP"))
            {
                // Create lists from the words summaries
                EndGameData endData = new EndGameData();

                // out variable for TryParse
                int wordnum;
                // keep track of which list the words should be added to
                int listcount = 0;

                // create a string array that is parts without the first element
                string[] findWords = new string[parts.Length-1];
                for (int i = 1; i < parts.Length; i++) { findWords[i - 1] = parts[i]; }

                // iterate through the STOP summary and add the words that follow numbers to the corresponding lists
                foreach (string w in findWords)
                {
                    if (Int32.TryParse(w, out wordnum))
                    {
                        listcount++;
                        switch(listcount)
                        {
                            case 1:
                                endData.ChangeMode(AddMode.P1Legal);
                                break;
                            case 2:
                                endData.ChangeMode(AddMode.P2Legal);
                                break;
                            case 3:
                                endData.ChangeMode(AddMode.Both);
                                break;
                            case 4:
                                endData.ChangeMode(AddMode.P1Illegal);
                                break;
                            case 5:
                                endData.ChangeMode(AddMode.P2Illegal);
                                break;
                        }
                    }
                    else
                    {
                        // it is a word not a number add to list
                        endData.Add(w);
                    }

                }
                // the game has ended, open the game history window, pass it the word lists and the scores and player names
                this.Dispatcher.BeginInvoke(new Action(() => { ShowEndGameWindow(hWnd, endData, p1RunningScore, p2RunningScore, p1Name, p2Name); }));
            }
            else
            {
                // the message must be IGNORING, don't need to do anything
            }
        }

        /// <summary>
        /// Clears the played word string when the clear button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearWord(object sender, MouseButtonEventArgs e)
        {
            // reset button properties
            foreach (System.Windows.Controls.Button b in Buttons)
                b.Dispatcher.BeginInvoke(new Action(() => { b.IsEnabled = true; }));

            // clear the played word string and display it
            wordplay = "";
            entered_word_label.Dispatcher.BeginInvoke(new Action(() => { entered_word_label.Text = ""; }));
        }


        /// <summary>
        /// Event handler for letter entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LetterEntered(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button current = (System.Windows.Controls.Button)e.Source;
            current.Dispatcher.BeginInvoke(new Action(() => { current.IsEnabled = false; }));
            wordplay = wordplay + current.Content;
            entered_word_label.Dispatcher.BeginInvoke(new Action(() => { entered_word_label.Text = wordplay.ToUpper(); }));
        }

        /// <summary>
        /// Sends an entered word to the Server when the enter button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterWord(object sender, MouseButtonEventArgs e)
        {
            // reset button properties
            foreach (System.Windows.Controls.Button b in Buttons)
                b.Dispatcher.BeginInvoke(new Action(() => { b.IsEnabled = true; }));

            // send the played word to the server
            model.SendMessage("WORD " + wordplay);

            // clear the played word string and display it
            wordplay = "";
            entered_word_label.Dispatcher.BeginInvoke(new Action(() => { entered_word_label.Text = ""; }));
        }

        /// <summary>
        /// Adds the buttons representing the boggle board to a list so they can all be updated.
        /// </summary>
        private List<System.Windows.Controls.Button> CreateButtonList()
        {
            List<System.Windows.Controls.Button> buttons = new List<System.Windows.Controls.Button>();
            buttons.Add(bb_button_1);
            buttons.Add(bb_button_2);
            buttons.Add(bb_button_3);
            buttons.Add(bb_button_4);
            buttons.Add(bb_button_5);
            buttons.Add(bb_button_6);
            buttons.Add(bb_button_7);
            buttons.Add(bb_button_8);
            buttons.Add(bb_button_9);
            buttons.Add(bb_button_10);
            buttons.Add(bb_button_11);
            buttons.Add(bb_button_12);
            buttons.Add(bb_button_13);
            buttons.Add(bb_button_14);
            buttons.Add(bb_button_15);
            buttons.Add(bb_button_16);
            return buttons;
        }

        /// <summary>
        /// Creates a game history window and closes the game window
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="endData"></param>
        /// <param name="_p1RunningScore"></param>
        /// <param name="_p2RunningScore"></param>
        /// <param name="_p1Name"></param>
        /// <param name="_p2Name"></param>
        private void ShowEndGameWindow(MainWindow _hWnd, EndGameData endData, string _p1RunningScore, string _p2RunningScore, string _p1Name, string _p2Name)
        {
            GameEndWindow endWindow = new GameEndWindow(_hWnd, endData, _p1RunningScore, _p2RunningScore, _p1Name, _p2Name);
            endWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Event handler for the quit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quit_button_Click(object sender, RoutedEventArgs e)
        {
            QuitGame("Are you sure you want to quit?", MessageBoxButton.YesNo);
        }

        /// <summary>
        /// Event handler for a lost connection to the server
        /// </summary>
        private void HandleLostConnection()
        {
            QuitGame("Server connection lost: shutting down game.", MessageBoxButton.OK);
        }

        /// <summary>
        /// Event handler for when there is no server connection
        /// </summary>
        private void HandleNoServerConnection()
        {
            System.Windows.MessageBox.Show("Server could not be found: please try again.", "Boggle", MessageBoxButton.OK);
            hWnd.Dispatcher.BeginInvoke(new Action(() => { hWnd.Show(); }));
            hWnd.Dispatcher.BeginInvoke(new Action(() => { hWnd.EnablePlayButton(); }));
            this.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
        }

        /// <summary>
        /// Displays a message box before quitting the game and closing the program
        /// </summary>
        /// <param name="s"></param>
        /// <param name="m"></param>
        private void QuitGame(String s, MessageBoxButton m)
        {
            // display a message box prompting the user whether or not to really quit
            MessageBoxResult result = System.Windows.MessageBox.Show(s, "Boggle", m);
            if ((result == MessageBoxResult.Yes) || (result == MessageBoxResult.OK))
                QuitGame();
        }

        /// <summary>
        /// Quits the game and closes the program
        /// </summary>
        private void QuitGame()
        {
            // close the main window and this window
            hWnd.Dispatcher.BeginInvoke(new Action(() => { hWnd.Close(); }));
            this.Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));
        }
    }

    /// <summary>
    /// Enumerates add modes for the EndGameData class.
    /// </summary>
    public enum AddMode { 
        /// <summary>Add new entries to the P1Legal list</summary>
        P1Legal,
        /// <summary>Add new entries to the P2Legal list</summary>
        P2Legal,
        /// <summary>Add new entries to the Both list</summary>
        Both,
        /// <summary>Add new entries to the P1Illegal list</summary>
        P1Illegal,
        /// <summary>Add new entries to the P2Illegal list</summary>
        P2Illegal
    };

    /// <summary>
    /// Represents the lists comprising legal and illegal words used by two players, and legal words used by both
    /// </summary>
    public class EndGameData
    {
        /// <summary>Holds the list of legal words played by player 1</summary>
        public List<string> P1Legal { get; private set; }
        /// <summary>Holds the list of illegal words played by player 1</summary>
        public List<string> P1Illegal { get; private set; }
        /// <summary>Holds the list of legal words played by both players</summary>
        public List<string> Both { get; private set; }
        /// <summary>Holds the list of legal words played by player 2</summary>
        public List<string> P2Legal { get; private set; }
        /// <summary>Holds the list of illegal words played by player 2</summary>
        public List<string> P2Illegal { get; private set; }

        // determines which list to add new entries to
        AddMode mode;

        /// <summary>
        /// Constructor for a new EndGameData object. Defaults to adding to P1Legal
        /// </summary>
        public EndGameData()
        {
            mode = AddMode.P1Legal;
            P1Legal = new List<string>();
            P1Illegal = new List<string>();
            Both = new List<string>();
            P2Legal = new List<string>();
            P2Illegal = new List<string>();
        }

        /// <summary>
        /// Changes which String List will receive new entries
        /// </summary>
        /// <param name="m"></param>
        public void ChangeMode(AddMode m)
        {
            mode = m;
        }

        /// <summary>
        /// Returns the current add mode.
        /// </summary>
        /// <returns></returns>
        public AddMode GetMode()
        {
            return mode;
        }

        /// <summary>
        /// Adds a string to the List specified by the current mode
        /// </summary>
        /// <param name="s"></param>
        public void Add(String s)
        {
            // add the input to a different list, depending on the current mode
            switch (mode)
            {
                case AddMode.P1Legal:
                    P1Legal.Add(s);
                    break;

                case AddMode.P2Legal:
                    P2Legal.Add(s);
                    break;

                case AddMode.Both:
                    Both.Add(s);
                    break;

                case AddMode.P1Illegal:
                    P1Illegal.Add(s);
                    break;

                case AddMode.P2Illegal:
                    P2Illegal.Add(s);
                    break;
            }
        }
    }
}
