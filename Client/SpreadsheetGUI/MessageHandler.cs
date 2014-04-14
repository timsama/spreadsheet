using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNetworking;
using System.Net;
using System.Net.Sockets;

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
        TcpClient client;
        private StringSocket outSocket;
        public readonly String IpAddress, PortNumber, Password;

        // register for this event to be notified when a login has succeeded. List<String> contains a list of filenames as individual strings
        public event Action<List<String>> LoggedIn;

        // register for this event to be notified when a login has failed
        public event Action PasswordRejected;

        // register for this event to be notified when an update has taken place. String contains the updated version number
        // SyncCell contains the updated cell name and new cell value, or "" if update message was empty
        public event Action<String, SyncCell> Updated;

        // register for this event to be notified when a save has completed
        public event Action Saved;

        // register for this event to be notified when a server error has occurred. String contains the error message sent by the server
        public event Action<String> ErrorMessage;

        // register for this event to be notified when a resync command has been received. String contains the updated version number
        // The List contains all cell names and contents that should remain after wiping the spreadsheet
        public event Action<String, List<SyncCell>> Sync;

        // default constructor
        public MessageHandler()
        {

        }

        // destructor
        ~MessageHandler()
        {
            if (client != null)
                client.Close();
        }

        // constructor
        public MessageHandler(String _ipAddress, String _portNumber, String _password)
        {
            IpAddress = _ipAddress;
            PortNumber = _portNumber;
            Password = _password;
        }

        // copy constructor (creates a MessageHandler on a different socket)
        public MessageHandler(MessageHandler m)
        {
            IpAddress = m.IpAddress;
            PortNumber = m.PortNumber;
            Password = m.Password;

            // connect to the same server
            Connect(IpAddress, PortNumber);

            // enter the same password
            Login(Password);
        }

        // called by the StringSocket when a message is received
        // evaluate message to determine which event to raise, and what values to pass in it
        public void ReceiveMessage(String message, Exception e, object payload)
        {
            //System.Windows.Forms.MessageBox.Show(message);

            // get the keyword (i.e. the first word of the message)
            String keyword;

            // check to see if the message is escape-delimited or a single keyword (i.e. no char27 between the beginning of the message and the \n)
            if (message.Contains((char)27))
            {
                keyword = message.Substring(0, message.IndexOf((char)27));

                // shorten the string to no longer include the keyword
                message = message.Substring(message.IndexOf((char)27) + 1);
            }
            else
            {
                keyword = message.Substring(0, message.IndexOf('\n'));
                message = "\n";
            }

            //System.Windows.Forms.MessageBox.Show(keyword);

            switch (keyword)
            {
                case "FILELIST":
                    TriggerLoggedIn(message);
                    break;
                case "INVALID":
                    TriggerPasswordRejected();
                    break;
                case "ERROR":
                    ErrorMessage(message);
                    break;
            }

            //outSocket.BeginReceive(ReceiveMessage, null);
        }

        // connects a StringSocket to the server
        public void Connect(String ipAddress, String portNumber)
        {
            // DEBUG: REMOVE BEFORE RELEASE
            // System.Windows.Forms.MessageBox.Show("Connecting to server " + ipAddress + ":" + portNumber);
        }

        /********************************/
        /* BEGIN DEBUG TRIGGERS SECTION */
        /********************************/

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerUpdated(String version, SyncCell cell)
        {
            Updated(version, cell);
        }

        // FOR DEBUG USE ONLY, THIS WILL BE REMOVED
        public void TriggerLoggedIn(String message)
        {
            List<String> filenames = new List<string>();

            // loop while there are still files in the list
            while (message.Contains((char)27))
            {
                // add first filename in the message to the list of filenames
                filenames.Add(message.Substring(0, message.IndexOf((char)27)));

                // remove the filename from the message
                message = message.Substring(message.IndexOf((char)27) + 1);
            }

            // there is only one or zero file left in the list

            String newline = "" + '\n';
            String file = message.Replace(newline, "");
                if (file.Trim().Length > 0)
                    filenames.Add(file);

            // call the login event if it has any listeners
            if(LoggedIn != null)
                LoggedIn(filenames);
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
        public void TriggerSync(String version, List<SyncCell> cells)
        {
            Sync(version, cells);
        }

        /******************************/
        /* END DEBUG TRIGGERS SECTION */
        /******************************/

        // logs into the server
        public void Login(string password)
        {
            try
            {
                int port;
                Int32.TryParse(PortNumber, out port);

                // start the TCP client
                if (client == null)
                    client = new TcpClient(IpAddress, port);

                // set up the StringSockets
                if (outSocket == null)
                    outSocket = new StringSocket(client.Client, UTF8Encoding.Default);
                
                // send the PASSWORD command with the password
                outSocket.BeginSend("PASSWORD" + (char)27 + password + "\n", (e, o) => { }, null);

                // start listening for reponses
                outSocket.BeginReceive(ReceiveMessage, null);
            }
            catch (SocketException e)
            {
                ErrorMessage(e.Message);
                //ErrorMessage("Server is not available. Please try again.");
            }
        }

        // sends the given message
        private void Send(string message)
        {
            // remove \e and \n (could be entered by user), but not (char)27s entered by the system
            message = message.Replace("\\e", "");
            message = message.Replace("\\n", "");
            message = message.Replace("\n", "") +"\n";

            outSocket.BeginSend(message, (e, o) => { }, null);
            outSocket.BeginReceive(ReceiveMessage, null);

            // DEBUG: REMOVE BEFORE RELEASE
            //System.Windows.Forms.MessageBox.Show(message);
        }

        // opens the named spreadsheet
        public void OpenFile(string filename)
        {
            // send the OPEN command with the filename
            Send("OPEN" + (char)27 + filename + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerUpdated("90", new SyncCell("", ""));
        }

        // creates a new spreadsheet
        public void CreateFile(string filename)
        {
            // send the CREATE command with the filename
            Send("CREATE" + (char)27 + filename + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            TriggerUpdated("0", new SyncCell("", ""));
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
        public void EnterChange(String version, SyncCell cell)
        {
            Send("ENTER" + (char)27 + version + (char)27 + cell.Name + (char)27 + cell.Contents + "\n");


            // DEBUG: REMOVE BEFORE RELEASE
            int temp = 0;
            Int32.TryParse(version, out temp);

            TriggerUpdated((temp + 1).ToString(), cell);
        }

        // undoes the last change on the server
        public void Undo(String version)
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
