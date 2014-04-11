using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS
{
    // represents a cell that should be written from the server
    public class SyncCell
    {
        public readonly String Name;
        public readonly String Contents;

        // constructor: never use default constructor because of readonly data members
        public SyncCell(String name, String contents)
        {
            Name = name;
            Contents = contents;
        }
    }

    // represents the a sender/receiver for messages to/from the server

    public class MessageHandler
    {
        // register for this event to be notified when a file has been successfully opened. String contains the XML of the opened file
        public event Action<String> FileOpened;

        // register for this event to be notified when a login has succeeded. List<String> contains a list of filenames as individual strings
        public event Action<List<String>> LoggedIn;

        // register for this event to be notified when a login has failed
        public event Action PasswordRejected;

        // register for this event to be notified when an update has taken place. SyncCell contains the updated cell name and new cell value, or "" if update message was empty
        public event Action<SyncCell> Updated;

        // register for this event to be notified when a save has completed
        public event Action Saved;

        // register for this event to be notified when a server error has occurred. String contains the error message sent by the server
        public event Action<String> ErrorMessage;

        // register for this event to be notified when a resync command has been received. The List contains all cell names and contents that should remain after wiping the spreadsheet
        public event Action<List<SyncCell>> Sync;

        // constructor
        public MessageHandler()
        {

        }

        // called by the StringSocket when a message is received
        public void ReceiveMessage(String message)
        {
            // evaluate message; determine which event to raise, and what values to pass in it
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
            Updated(new SyncCell("B6", "459"));
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

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerSaved()
        {
            Saved();
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerError(string message)
        {
            ErrorMessage(message);
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerSync(List<SyncCell> cells)
        {
            Sync(cells);
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

        // saves the spreadsheet
        public void Save(string filename)
        {
            // send the SAVE command with the filename
            Send("SAVE" + (char)27 + filename + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerSaved();
        }

        // sends a change to the server
        public void EnterChange(int version, SyncCell cell)
        {
            Send("ENTER" + (char)27 + version + (char)27 + cell.Name + (char)27 + cell.Contents + "\n");
        }

        // undoes the last change on the server
        public void Undo(int version)
        {
            Send("UNDO" + (char)27 + version + "\n");
        }

        // disconnects the client on the spreadsheet
        public void Disconnect()
        {
            Send("DISCONNECT\n");
        }

        // sends a resync request to the server
        public void Resync()
        {
            Send("RESYNC\n");
        }
    }
}
