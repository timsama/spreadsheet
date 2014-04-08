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
    public partial class FileView : Form
    {
        // tracks how many Spreadsheet MainForms are open
        private int childcount;
        private MessageHandler msgHand;

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
            openProgressBar.Style = ProgressBarStyle.Marquee;
        }
        // stops the progress bar marquee's animation
        private void StopOpenMarquee()
        {
            openProgressBar.Style = ProgressBarStyle.Blocks;
        }

        // starts the progress bar marquee's animation
        private void StartCreateMarquee()
        {
            createProgressBar.Style = ProgressBarStyle.Marquee;
        }
        // stops the progress bar marquee's animation
        private void StopCreateMarquee()
        {
            createProgressBar.Style = ProgressBarStyle.Blocks;
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
        }

        // to be registered with an event for when the file has been received, string s should contain the spreadsheet XML
        private void Open(String s)
        {
            // unregister so later file openings don't trigger this
            msgHand.FileOpened -= Open;

            // open the file
            Form handle = new MainForm(this);
            childcount++;
            handle.FormClosed += Child_Closed;
            handle.Show();
            this.Hide();
        }

        // to be registered with an event for when an update has been received, string name should contain the cell name, and string contents should contain the cell contents
        private void Update(String name, String contents)
        {
            // unregister so later file openings don't trigger this
            msgHand.Updated -= Update;

            // open the file
            Form handle = new MainForm(this);
            childcount++;
            handle.FormClosed += Child_Closed;
            handle.Show();
            this.Hide();
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
            // add the Open method to the list of event handlers for the message handler
            msgHand.FileOpened += Open;
            msgHand.OpenFile(filename);

            return false;
        }

        // creates a spreadsheet file
        private bool createSpreadsheet(string filename)
        {
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
            String filename = filenameTextBox.Text.Trim();

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
