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
        private int clientVersion;
        private SyncCell CellToSend;

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

            // enter the same password
            Login(Password);
        }

        // called by the StringSocket when a message is received
        // evaluate message to determine which event to raise, and what values to pass in it
        public void ReceiveMessage(String message, Exception e, object payload)
        {
            System.Windows.Forms.MessageBox.Show("Message: " + message + "\n Version: " + clientVersion);

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
                keyword = message;
            }

            // trigger a different event based on what keyword has been received
            switch (keyword)
            {
                case "FILELIST":
                    TriggerLoggedIn(message);
                    break;
                case "INVALID":
                    TriggerPasswordRejected();
                    break;
                case "ERROR":
                    TriggerErrorMessage(message);
                    break;
                case "UPDATE":
                    TriggerUpdated(message);
                    break;
                case "SAVED":
                    TriggerSaved();
                    break;
                case "SYNC":
                    TriggerSync(message);
                    break;
            }
        }

        // prepares and executes a Sync event by calling TriggerUpdated after some initial processing
        public void TriggerSync(String message)
        {
            // declare the escape delimiter
            char[] esc = new char[1];
            esc[0] = (char)27;

            // get the server's version number
            String version = message.Substring(0, message.IndexOf(esc[0]));

            // parse the server version as an int and make it the new version of the spreadsheet
            int serverVersion = 0;
            Int32.TryParse(version, out serverVersion);
            clientVersion = serverVersion;

            // run the Sync as an update
            TriggerUpdated(message);

            // if there is still a cell to send (TriggerUpdated will nullify CellToSend if the sync changed that cell), then resend
            if (CellToSend != null)
            {
                EnterChange(CellToSend);
            }
        }

        // prepares and executes an Updated event (triggers a Sync for Updates with multiple cells, because they are identical)
        public void TriggerUpdated(String message)
        {
            // declare the escape delimiter
            char[] esc = new char[1];
            esc[0] = (char)27;

            // declare the collection of SyncCells to use in the spreadsheet
            List<SyncCell> cells = new List<SyncCell>();

            // get the server's version number
            String version = "";
            if (message.Length > 0 && message.IndexOf(esc[0]) > 0)
            {
                version = message.Substring(0, message.IndexOf(esc[0]));
                message = message.Substring(message.IndexOf(esc[0]) + 1);
            }
            else
            {
                version = message;
                message = "";
            }

            // parse the server version as an int
            int serverVersion = 0;
            Int32.TryParse(version, out serverVersion);

            // if the server version is more than 1 greater than the client version, we missed an update somewhere, so send a resync command and don't bother updating here
            if (serverVersion > (clientVersion + 1))
            {
                Resync();
                clientVersion = serverVersion;
                return;
            }
            else
            {
                clientVersion = serverVersion;
            }

            // keep track of whether name or contents are currently being specified
            bool IsName = true;
            String name = "";

            // iterate over the message to fill the SyncCells
            foreach (String token in message.Split(esc))
            {
                // every other token is a name
                if (IsName)
                {
                    name = token;
                }
                else
                {
                    cells.Add(new SyncCell(name, token));
                }

                IsName = !IsName;
            }

            // if this update/sync modified a cell that we tried to modify, either our change worked (and there's no point in resending) or the pre-our-change version
            // of that cell changed, and the change should not go through
            if (CellToSend != null)
            {
                foreach (SyncCell cell in cells)
                {
                    if (cell.Name == CellToSend.Name)
                    {
                        CellToSend = null;
                        break;
                    }
                }
            }

            // call updated for single cell updates; multi cell updates only happen upon file opening or resyncs, so they can be handled like a sync
            if (cells.Count == 1)
            {
                Updated(version, cells.First());
            }
            else
            {
                Sync(version, cells);
            }

            return;
        }

        // prepares and executes a LoggedIn event
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

        // prepares and executes a PasswordRejected event
        public void TriggerPasswordRejected()
        {
            if (PasswordRejected != null)
                PasswordRejected();
        }

        // prepares and executes a Saved event
        public void TriggerSaved()
        {
            if(Saved != null)
                Saved();
        }

        // prepares and executes an ErrorMessage event
        public void TriggerErrorMessage(string message)
        {
            if (ErrorMessage != null)
                ErrorMessage(message);
        }

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
                if (e != null)
                    ErrorMessage(e.Message);
                else 
                    ErrorMessage("Server is not available. Please try again.");
            }
        }

        // sends the given message
        private void Send(string message)
        {
            // remove \e and \n (could be entered by user), but not (char)27s entered by the system
            message = message.Replace("\\e", "");
            message = message.Replace("\\n", "");
            message = message.Replace("\n", "") +"\n";

            if (outSocket != null)
            {
                outSocket.BeginSend(message, (e, o) => { }, null);
                outSocket.BeginReceive(ReceiveMessage, null);
            }
            // DEBUG: REMOVE BEFORE RELEASE
            //System.Windows.Forms.MessageBox.Show(message);
        }

        // opens the named spreadsheet
        public void OpenFile(string filename)
        {
            // send the OPEN command with the filename
            Send("OPEN" + (char)27 + filename + "\n");
        }

        // creates a new spreadsheet
        public void CreateFile(string filename)
        {
            // send the CREATE command with the filename
            Send("CREATE" + (char)27 + filename + "\n");
        }

        // saves the spreadsheet
        public void Save(string filename)
        {
            // send the SAVE command with the filename
            Send("SAVE" + (char)27 + filename + "\n");
        }

        // sends a change to the server
        public void EnterChange(SyncCell cell)
        {
            // save the cell to send out in case it needs to be resent
            CellToSend = cell;

            // send the ENTER command with the version number, and the cell to change
            Send("ENTER" + (char)27 + clientVersion + (char)27 + cell.Name + (char)27 + cell.Contents + "\n");

            // DEBUG: REMOVE BEFORE RELEASE
            //TriggerUpdated(version + (char)27 + cell.Name + (char)27 + cell.Contents);
        }

        // undoes the last change on the server
        public void Undo(String version)
        {
            // send the UNDO command with the version number
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
