using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SS
{
    public partial class LoginForm : Form
    {
        MessageHandler msgHand;
        FileView fileview;

        public LoginForm()
        {
            fileview = null;
            msgHand = null;

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
            // proceed only if input is validated
            if (validateLogin())
            {
                // disable the password field and show a progress bar to indicate that program is connecting to server
                passwordTextBox.ReadOnly = true;
                StartMarquee();

                // initialize an empty list to hold the available files on the server
                List<string> files = new List<string>();

                // set up message handler if one hasn't been set up already
                if (msgHand == null)
                    msgHand = new MessageHandler(ipTextBox.Text, portTextBox.Text, passwordTextBox.Text);

                // set up the fileview if one hasn't been set up already
                if (fileview == null)
                {
                    fileview = new FileView(msgHand);
                    fileview.FormClosed += Shutdown;
                }

                // log in using the given password
                login(passwordTextBox.Text);
            }
        }

        // validates inputs to the login form. We can't assume the server password will be non-empty; all we know is that it can't include the escape character
        bool validateLogin()
        {
            // first, make sure the password doesn't contain the escape character (char27)
            bool passwordValid = ! passwordTextBox.Text.Contains((char)27);

            // declare regex matching patterns
            String ipPattern = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b"; // source: http://www.regular-expressions.info/examples.html
            String portPattern = @"[\d]{1,4}";

            // declare the actual regexes
            Regex ipRegex = new Regex(ipPattern);
            Regex portRegex = new Regex(portPattern);
            
            // test the ip address and port to make sure they're properly formatted
            bool ipValid = ipRegex.IsMatch(ipTextBox.Text);
            bool portValid = portRegex.IsMatch(portTextBox.Text);

            // show error message based on what, if anything, is invalid
            if (!(passwordValid && ipValid && portValid))
            {
                String errorMessage = "There was a problem validating input in the following fields:\n";
                errorMessage += passwordValid ? " - Password (the escape character is not allowed)\n" : "";
                errorMessage += ipValid ? " - IP Address (valid addresses range from 0.0.0.0 to 255.255.255.255)\n" : "";
                errorMessage += portValid ? " - Port Number (valid ports range from 0 to 9999)\n" : "";
                errorMessage += "\nPlease correct the fields and try again.";

                MessageBox.Show(errorMessage);

                return false;
            }

            // if we got here, all input was valid
            return true;
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

            // set up socket connection
            msgHand.Connect(ipTextBox.Text, portTextBox.Text);

            // send the password to the server
            msgHand.Login(password);
        }

        // shut the program down if its child is shut down
        private void Shutdown(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ipTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // the key input is valid if it's numbers 0-9, the period, or backspace (which for some reason has a char value)
            bool valid = ((e.KeyChar == '0') || (e.KeyChar == '1') || (e.KeyChar == '2') || (e.KeyChar == '3') || (e.KeyChar == '4') || (e.KeyChar == '5')
                || (e.KeyChar == '6') || (e.KeyChar == '7') || (e.KeyChar == '8') || (e.KeyChar == '9') || (e.KeyChar == '.') || (e.KeyChar == (char)8));

            // if the key is invalid, act like we already handled it so it doesn't get input. Otherwise, let it continue
            e.Handled = !valid;
        }

        private void portTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // the key input is valid if it's numbers 0-9 or backspace (which for some reason has a char value)
            bool valid = ((e.KeyChar == '0') || (e.KeyChar == '1') || (e.KeyChar == '2') || (e.KeyChar == '3') || (e.KeyChar == '4') || (e.KeyChar == '5')
                || (e.KeyChar == '6') || (e.KeyChar == '7') || (e.KeyChar == '8') || (e.KeyChar == '9') || (e.KeyChar == (char)8));

            // if the key is invalid, act like we already handled it so it doesn't get input. Otherwise, let it continue
            e.Handled = !valid;
        }
    }
}
