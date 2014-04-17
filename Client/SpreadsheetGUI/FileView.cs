using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace SS
{
    public partial class FileView : Form
    {
        // tracks how many Spreadsheet MainForms are open
        private int childcount;
        private MessageHandler msgHand;
        private String selectedFilename;

        // if FileView is a child form, always pass in its parent's MessageHandler
        public FileView()
        {
            msgHand = new MessageHandler();
            childcount = 0;
            InitializeComponent();
            this.FormClosing += FileView_FormClosing;
        }

        // creates a new FileView using the passed-in MessageHandler
        public FileView(MessageHandler _msgHand)
        {
            msgHand = _msgHand;
            childcount = 0;
            InitializeComponent();
            this.FormClosing += FileView_FormClosing;
        }

        // returns the MessageHandler associated with this FileView
        public MessageHandler getMessageHandler()
        {
            return this.msgHand;
        }

        // resets the controls in the FileView form
        public void ResetControls(IEnumerable<String> files)
        {
            // clear the existing filesListBox
            filesListBox.Items.Clear();

            // insert the new file names into the filesListBox
            foreach (String file in files)
            {
                filesListBox.Items.Add(file);
            }

            // call the normal version of this function
            ResetControls();
        }

        // resets the controls in the FileView form
        public void ResetControls()
        {
            // no file is currently selected
            selectedFilename = "";

            // revert the dialog's appearance
            StopCreateMarquee();
            StopOpenMarquee();
            filesListBox.BeginInvoke(new Action(() => { filesListBox.Enabled = true; }));
            filesListBox.BeginInvoke(new Action(() => { filesListBox.SelectedItems.Clear(); }));
            filenameTextBox.BeginInvoke(new Action(() => { filenameTextBox.Enabled = true; }));
            filenameTextBox.BeginInvoke(new Action(() => { filenameTextBox.Text = ""; }));
            openButton.BeginInvoke(new Action(() => { openButton.Enabled = true; }));
            createButton.BeginInvoke(new Action(() => { createButton.Enabled = true; }));
            
            // select the filesListBox by default
            filesListBox.Focus();
            filesListBox.Select();

            // re-show this dialog
            this.Show();
        }

        // sets focus on the new file textbox
        public void FocusCreateFile()
        {
            // set focus to the filename textbox
            filenameTextBox.Focus();
        }

        // starts the progress bar marquee's animation
        private void StartOpenMarquee()
        {
            openProgressBar.BeginInvoke(new Action(() => { openProgressBar.Style = ProgressBarStyle.Marquee; }));
        }
        // stops the progress bar marquee's animation
        private void StopOpenMarquee()
        {
            openProgressBar.BeginInvoke(new Action(() => { openProgressBar.Style = ProgressBarStyle.Blocks; }));
        }

        // starts the progress bar marquee's animation
        private void StartCreateMarquee()
        {
            createProgressBar.BeginInvoke(new Action(() => { createProgressBar.Style = ProgressBarStyle.Marquee; }));
        }
        // stops the progress bar marquee's animation
        private void StopCreateMarquee()
        {
            createProgressBar.BeginInvoke(new Action(() => { createProgressBar.Style = ProgressBarStyle.Blocks; }));
        }

        // sets the files listbox to contain only the passed-in filenames
        public void SetFilesList(IEnumerable<String> files)
        {
            // clear the current filesListBox
            filesListBox.Items.Clear();

            // add each file in the files collection to the files listbox
            foreach (String file in files)
            {
                filesListBox.Items.Add(file);
            }

            // no file is currently selecte
            selectedFilename = "";
        }

        // to be registered with an event for when a Sync has been received. Version contains the latest version number for the file
        // cells contains the cells to be inserted into the spreadsheet
        private void Sync(String version, IEnumerable<SyncCell> cells)
        {
            // unregister so later file openings don't trigger this
            //msgHand.Sync -= Sync;

            // open the file;
            childcount++;
            this.BeginInvoke(new Action(() => {
                Form handle = new MainForm(selectedFilename, version, this);
                handle.FormClosed += Child_Closed;
                handle.Show();
                this.Hide();
            }));
        }

        // to be registered with an event for when an update has been received. Version contains the latest version number for the file
        // SyncCell contains the cell name and contents (should be blank for all FileView purposes)
        private void Update(String version, SyncCell cell)
        {
            // unregister so later file openings don't trigger this
            //msgHand.Updated -= Update;

            // open the file
            childcount++;
            this.BeginInvoke(new Action(() =>
            {
                Form handle = new MainForm(selectedFilename, version, this);
                handle.FormClosed += Child_Closed;
                handle.Show();
                this.Hide();
            }));
        }

        // handles the form closing -- specifically, turns a Close() into a Hide() if any children remain
        private void FileView_FormClosing(object sender, FormClosingEventArgs e)
        {
            // if no children are left, close this too. Otherwise, cancel the close and hide it instead.
            if (childcount > 0)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        // this is run when any child window is closed; shuts down the program when all child windows have been closed
        private void Child_Closed(object sender, EventArgs e)
        {
            // decrease the running counter of how many child windows are open
            childcount--;

            // if no children are left and this isn't currently displaying, close this too
            if (childcount <= 0 && !this.Visible)
            {
                this.Close();
            }
        }

        // opens a spreadsheet file
        private bool openSpreadsheet(string filename)
        {
            // save the filename for creation of the MainForm later
            selectedFilename = filename;

            // add the Open method to the list of event handlers for the message handler
            msgHand.Updated += Update;
            msgHand.Sync += Sync;
            msgHand.ErrorMessage += handleErrorMessage;
            msgHand.OpenFile(filename);

            return false;
        }

        // handles error messages sent from the server
        private void handleErrorMessage(String message)
        {
            // keep from triggerint this multiple times
            msgHand.ErrorMessage -= handleErrorMessage;

            // show the server's error message
            MessageBox.Show(message);

            // re-enable user input
            StopOpenMarquee();
            filesListBox.BeginInvoke(new Action(() => { filesListBox.Enabled = true; }));
            filenameTextBox.BeginInvoke(new Action(() => { filenameTextBox.Enabled = true; }));
            openButton.BeginInvoke(new Action(() => { openButton.Enabled = true; }));
            createButton.BeginInvoke(new Action(() => { createButton.Enabled = true; }));
        }

        // creates a spreadsheet file
        private bool createSpreadsheet(string filename)
        {
            // save the filename for creation of the MainForm later
            selectedFilename = filename;

            // add the Open method to the list of event handlers for the message handler
            msgHand.Updated += Update;
            msgHand.CreateFile(filename);

            return false;
        }

        // handles the Open Spreadsheet button
        private void openButton_Click(object sender, EventArgs e)
        {
            // if nothing is selected, just return from the function without doing anything
            if (filesListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a spreadsheet to open.");
                return;
            }

            // lock down the display while attempting to open the spreadsheet
            StartOpenMarquee();
            filesListBox.BeginInvoke(new Action(() => { filesListBox.Enabled = false; }));
            filenameTextBox.BeginInvoke(new Action(() => { filenameTextBox.Enabled = false; }));
            openButton.BeginInvoke(new Action(() => { openButton.Enabled = false; }));
            createButton.BeginInvoke(new Action(() => { createButton.Enabled = false; }));

            // attempt to open the spreadsheet
            string filename = filesListBox.SelectedItem.ToString();
            this.BeginInvoke(new Action( () => {openSpreadsheet(filename);}));
        }

        // handles the Create New Spreadsheet button
        private void createButton_Click(object sender, EventArgs e)
        {
            // trim out whitespace and remove any char27s from the filename
            String filename = filenameTextBox.Text.Trim();
            String esc = "" + (char)27;
            filename = filename.Replace(esc, "");

            // if nothing is selected, just return from the function without doing anything
            if (filename.Length == 0)
            {
                MessageBox.Show("Please enter a spreadsheet file name.");
                return;
            }

            // lock down the display while attempting to open the spreadsheet
            StartCreateMarquee();
            filesListBox.BeginInvoke(new Action(() => { filesListBox.Enabled = false; }));
            filenameTextBox.BeginInvoke(new Action(() => { filenameTextBox.Enabled = false; }));
            openButton.BeginInvoke(new Action(() => { openButton.Enabled = false; }));
            createButton.BeginInvoke(new Action(() => { createButton.Enabled = false; }));

            // attempt to open the spreadsheet
            this.BeginInvoke(new Action(() => { createSpreadsheet(filename); }));
        }

        // sets the accept button to be the Create button when fileNameTextBox is selected
        private void filenameTextBox_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = createButton;
        }

        // sets the accept button to be the Open button when filesListBox is selected
        private void filesListBox_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = openButton;
        }        
    }
}
