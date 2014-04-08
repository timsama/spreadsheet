using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS
{
    public class MessageHandler
    {
        // register for this event to be notified when a file has been successfully opened. String contains the XML of the opened file
        public event Action<String> FileOpened;

        // register for this event to be notified when a login has succeeded. List<String> contains a list of filenames as individual strings
        public event Action<List<String>> LoggedIn;

        // register for this event to be notified when a login has failed
        public event Action PasswordRejected;

        // register for this event to be notified when an update has taken place. String contains the updated cell name and new cell value, or "" if update message was empty
        public event Action<String, String> Updated;

        // constructor
        public MessageHandler()
        {

        }

        /********************************/
        /* BEGIN DEBUG TRIGGERS SECTION */
        /********************************/

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerFileOpened()
        {
            FileOpened("<spreadsheet></spreadsheet>");
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerUpdated()
        {
            Updated("B6", "459");
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerLoggedIn()
        {
            List<String> filelist = new List<string>();

            filelist.Add("File1");
            filelist.Add("CherryCoke");
            filelist.Add("LastFile");

            LoggedIn(filelist);
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerPasswordRejected()
        {
            PasswordRejected();
        }

        /******************************/
        /* END DEBUG TRIGGERS SECTION */
        /******************************/

        // sends the given message
        private void Send(string message)
        {
            // DEBUG: REMOVE BEFORE RELEASE
            System.Windows.Forms.MessageBox.Show(message);
        }

        // logs into the server
        public void Login(string password)
        {
            // send the PASSWORD command with the password
            Send("PASSWORD" + (char)27 + password + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerLoggedIn();
        }

        // opens the named spreadsheet
        public void OpenFile(string filename)
        {
            // send the OPEN command with the filename
            Send("OPEN" + (char)27 + filename + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerFileOpened();
        }

        // creates a new spreadsheet
        public void CreateFile(string filename)
        {
            // send the CREATE command with the filename
            Send("CREATE" + (char)27 + filename + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerUpdated();
        }
    }
}
