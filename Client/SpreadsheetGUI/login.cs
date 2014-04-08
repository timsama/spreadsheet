using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SS
{
    public partial class LoginForm : Form
    {
        MessageHandler msgHand;
        FileView fileview;

        public LoginForm()
        {
            msgHand = new MessageHandler();
            fileview = new FileView();
            fileview.FormClosed += Shutdown;

            InitializeComponent();
        }

        // starts the progress bar marquee's animation
        private void StartMarquee()
        {
            loginProgressBar.Style = ProgressBarStyle.Marquee;
        }
        // stops the progress bar marquee's animation
        private void StopMarquee()
        {
            loginProgressBar.Style = ProgressBarStyle.Blocks;
        }

        // event handler for "Log In" button
        private void loginButton_Click(object sender, EventArgs e)
        {
            // disable the password field and show a progress bar to indicate that program is connecting to server
            passwordTextBox.ReadOnly = true;
            StartMarquee();

            // initialize an empty list to hold the available files on the server
            List<string> files = new List<string>();

            // log in using the given password
            login(passwordTextBox.Text);
        }

        // handles the LoggedIn event
        private void HandleLoggedIn(List<String> files)
        {
            // remove event handlers to prevent multiple triggers
            msgHand.LoggedIn -= HandleLoggedIn;
            msgHand.PasswordRejected -= HandlePasswordRejected;

            // stop the marquee animation
            StopMarquee();

            // password was correct; go to file selection dialog
            fileview.SetFilesList(files);
            fileview.Show();

            // hide this window, and close it down completely if it has no more child dialogs and is not visible to the user
            this.Hide();
        }

        // handles the PasswordRejected event
        private void HandlePasswordRejected()
        {
            // remove event handlers to prevent multiple triggers
            msgHand.PasswordRejected -= HandlePasswordRejected;
            msgHand.LoggedIn -= HandleLoggedIn;

            // password was incorrect; tell user so, and reset + re-enable the password field
            StopMarquee();
            MessageBox.Show("Password was incorrect. Please try again.");
            passwordTextBox.ReadOnly = false;
        }

        // logs in using the given password
        private void login(string password)
        {
            // set up event handlers in advance
            msgHand.LoggedIn += HandleLoggedIn;
            msgHand.PasswordRejected += HandlePasswordRejected;

            // send the password to the server
            msgHand.Login(password);
        }

        // shut the program down if its child is shut down
        private void Shutdown(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
