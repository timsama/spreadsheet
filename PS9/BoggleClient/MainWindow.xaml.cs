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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BC;

namespace BoggleClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BoggleGameWindow bWnd;

        /// <summary>
        /// Creates a new connection window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            play_button.PreviewMouseUp += StartGame;
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartGame(object sender, MouseButtonEventArgs e)
        {
            // don't start two games from a double click
            play_button.PreviewMouseUp -= StartGame;
            
            // tell user we are connecting to the server
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.Text = "Connecting to Server"; }));
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.FontSize = 12; }));

            // create the game window (which will remain hidden until the START command is received
            this.Dispatcher.BeginInvoke(new Action(() => {bWnd = new BoggleGameWindow(this, player_name_textbox.Text, ip_address_textbox.Text); }));
            
            // tell the user we are waiting for an opponent to connect
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.Text = "Waiting for Opponent"; }));
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.FontSize = 12; }));
            player_name_textbox.Dispatcher.BeginInvoke(new Action(() => { player_name_textbox.IsReadOnly = true; }));
            ip_address_textbox.Dispatcher.BeginInvoke(new Action(() => { ip_address_textbox.IsReadOnly = true; }));
        }

        /// <summary>
        /// Re-enables the play button
        /// </summary>
        public void EnablePlayButton()
        {
            // re-enable the play button
            play_button.PreviewMouseUp += StartGame;

            // reset the UI elements to the way they are upon load
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.Text = "PLAY BOGGLE"; }));
            play_boggle_label.Dispatcher.BeginInvoke(new Action(() => { play_boggle_label.FontSize = 18; }));
            player_name_textbox.Dispatcher.BeginInvoke(new Action(() => { player_name_textbox.IsReadOnly = false; }));
            ip_address_textbox.Dispatcher.BeginInvoke(new Action(() => { ip_address_textbox.IsReadOnly = false; }));
        }

        /// <summary>
        /// Handles when the cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            // close the game window if one is open
            if (bWnd != null)
                bWnd.Dispatcher.BeginInvoke(new Action(() => { bWnd.Close(); }));
            
            // close this window
            this.Close();
        }
    }
}
