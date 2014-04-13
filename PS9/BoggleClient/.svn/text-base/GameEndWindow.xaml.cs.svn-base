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

namespace BoggleClient
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class GameEndWindow : Window
    {
        MainWindow hWnd;
        private List<string> p1Legal;
        private List<string> p1Illegal;
        private List<string> both;
        private List<string> p2Legal;
        private List<string> p2Illegal;
        private string p1FinalScore;
        private string p2FinalScore;
        private string p1Name;
        private string p2Name;

        /// <summary>
        /// Constructor for a new GameEndWindow
        /// </summary>
        /// <param name="_hWnd"></param>
        /// <param name="endData"></param>
        /// <param name="_p1RunningScore"></param>
        /// <param name="_p2RunningScore"></param>
        /// <param name="_p1Name"></param>
        /// <param name="_p2Name"></param>
        public GameEndWindow(MainWindow _hWnd, EndGameData endData, string _p1RunningScore, string _p2RunningScore, string _p1Name, string _p2Name)
        {
            InitializeComponent();
            p1Legal = endData.P1Legal;
            p1Illegal = endData.P1Illegal;
            both = endData.Both;
            p2Legal = endData.P2Legal;
            p2Illegal = endData.P2Illegal;
            p1FinalScore = _p1RunningScore;
            p2FinalScore = _p2RunningScore;
            p1Name = _p1Name.ToUpper();
            p2Name = _p2Name.ToUpper();
            hWnd = _hWnd;

            // update scoreboard
            UpdateScoreboard();
            // update word lists
            UpdateLists();
        }

        /// <summary>
        /// Updates the game summary lists
        /// </summary>
        private void UpdateLists()
        {
            FlowDocument p1WordsFlowDocument = new FlowDocument();
            // add player 1 legal words to a list
            List p1_legal = new List();
            foreach (string s in p1Legal)
            {
                Paragraph words = new Paragraph(new Run(s));
                p1_legal.ListItems.Add(new ListItem(words));
                p1_legal.Foreground = Brushes.Green;
                p1WordsFlowDocument.Blocks.Add(p1_legal);
            }
            // add player 1 illegal words to a list
            List p1_illegal = new List();
            foreach (string s in p1Illegal)
            {
                Paragraph words = new Paragraph(new Run(s));
                p1_illegal.ListItems.Add(new ListItem(words));
                p1_illegal.Foreground = Brushes.Red;
                p1WordsFlowDocument.Blocks.Add(p1_illegal);
            }
 

            // update words text box for both players
            FlowDocument bothWordsFlowDocument = new FlowDocument();
            // add words played by both to list
            List both_legal = new List();
            foreach (string s in both)
            {
                Paragraph words = new Paragraph(new Run(s));
                both_legal.ListItems.Add(new ListItem(words));
                bothWordsFlowDocument.Blocks.Add(both_legal);
            }

            FlowDocument p2WordsFlowDocument = new FlowDocument();
            // add player 2 legal words to a list
            List p2_legal = new List();
            foreach (string s in p2Legal)
            {
                Paragraph words = new Paragraph(new Run(s));
                p2_legal.ListItems.Add(new ListItem(words));
                p2_legal.Foreground = Brushes.Green;
                p2WordsFlowDocument.Blocks.Add(p2_legal);
            }
            // add player 2 illegal words to a list
            List p2_illegal = new List();
            foreach (string s in p2Illegal)
            {
                Paragraph words = new Paragraph(new Run(s));
                p2_illegal.ListItems.Add(new ListItem(words));
                p2_illegal.Foreground = Brushes.Red;
                p2WordsFlowDocument.Blocks.Add(p2_illegal);
            }

            // update the view
            p2_words_textbox.Dispatcher.BeginInvoke(new Action(() => { p2_words_textbox.Document = p2WordsFlowDocument; }));
            both_words_textbox.Dispatcher.BeginInvoke(new Action(() => { both_words_textbox.Document = bothWordsFlowDocument; }));
            p1_words_textbox.Dispatcher.BeginInvoke(new Action(() => { p1_words_textbox.Document = p1WordsFlowDocument; }));
        }

        /// <summary>
        /// Updates the scoreboard
        /// </summary>
        private void UpdateScoreboard()
        {
            p1_name_label.Dispatcher.BeginInvoke(new Action(() => { p1_name_label.Text = p1Name; }));
            p2_name_label.Dispatcher.BeginInvoke(new Action(() => { p2_name_label.Text = p2Name; }));
            p1_score_count_label.Dispatcher.BeginInvoke(new Action(() => { p1_score_count_label.Text = p1FinalScore; }));
            p2_score_count_label.Dispatcher.BeginInvoke(new Action(() => { p2_score_count_label.Text = p2FinalScore; }));

            // update result display based on final scores
            if (Int32.Parse(p1FinalScore) > Int32.Parse(p2FinalScore))
                result_label.Dispatcher.BeginInvoke(new Action(() => { result_label.Text = "YOU WON!"; }));
            else if (Int32.Parse(p1FinalScore) == Int32.Parse(p2FinalScore))
                result_label.Dispatcher.BeginInvoke(new Action(() => { result_label.Text = "YOU TIED!"; }));
            else
                result_label.Dispatcher.BeginInvoke(new Action(() => { result_label.Text = "YOU LOST!"; }));
        }

        /// <summary>
        /// Event handler for the play again button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void play_again_button_Click(object sender, RoutedEventArgs e)
        {
            hWnd.Show();
            hWnd.EnablePlayButton();
            this.Close();
        }

        /// <summary>
        /// Event handler for the quit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quit_button_Click(object sender, RoutedEventArgs e)
        {
            hWnd.Close();
            this.Close();
        }
    }
}
