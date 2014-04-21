using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using SpreadsheetUtilities;
using System.Xml;

namespace SS
{
    public partial class MainForm : Form
    {
        private Spreadsheet spreadsheetModel;
        private Dictionary<String, GUILanguage> LangSet;
        private FileView FileViewHandle;
        private MessageHandler msgHand;
        private String filename;
        private String version;
        private bool locked;

        private GUILanguage guiLang;

        /// <summary>
        /// Initialize the form and open a file
        /// </summary>
        /// <param name="filename"></param>
        public MainForm(string _filename, string _version, FileView _fileview)
            : this(_filename)
        {
            // set this spreadsheet's version
            version = _version;

            // set a handle to this form's parent FileView
            FileViewHandle = _fileview;

            // set a handle to a new MessageHandler (each spreadsheet operates on its own socket)
            msgHand = new MessageHandler(_fileview.getMessageHandler());
        }

        /// <summary>
        /// Initialize the form and open a file
        /// </summary>
        /// <param name="filename"></param>
        public MainForm(string _filename)
            : this()
        {
            // save the filename
            filename = _filename;

            //update the cells in the spreadsheet
            updateAllCells();
        }

        /// <summary>
        /// Initialize a new form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            //set the underlying model to the file
            spreadsheetModel = new Spreadsheet(validate, s => s.ToUpper(), "PS6");

            //get GUI languages that are available
            getLanguages();

            //set the GUI language
            guiLang = new GUILanguage();
            setLang("Default");

            //set up some event handlers
            ssPanel.SelectionChanged += displaySelection;
            ssPanel.PreviewKeyDown += ssPanel_PreviewKeyDown;

            // by default, input is not locked
            locked = false;
        }

        /// <summary>
        /// Event handler for when the spreadsheet panel has finished loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ssPanel_Load(object sender, EventArgs e)
        {
            // set up events to respond to the MessageHandler
            msgHand.Saved += handleSaved;
            msgHand.Updated += handleUpdate;
            msgHand.Sync += handleSync;
            msgHand.ErrorMessage += handleErrorMessage;

            // open the file
            msgHand.OpenFile(filename);
        }

        /// <summary>
        /// Check for unsaved changes when closing the spreadsheet
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bool cancel = stopForUnsavedChanges();
            e.Cancel = cancel;
            msgHand.Disconnect();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// KeyDown event listener for the spreadsheet panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ssPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int row;
            int col;
            ssPanel.GetSelection(out col, out row);

            // move left if left arrow key is pressed
            if ((e.KeyCode == Keys.Left) && (col > 0))
            {
                ssPanel.SetSelection(col - 1, row);
            }

            // move up if up arrow key is pressed
            if ((e.KeyCode == Keys.Up) && (row > 0))
            {
                ssPanel.SetSelection(col, row - 1);
            }

            // move right if right arrow key is pressed
            if ((e.KeyCode == Keys.Right) && (col < 25))
            {
                ssPanel.SetSelection(col + 1, row);
            }

            // move down if down arrow key is pressed
            if ((e.KeyCode == Keys.Down) && (row < 99))
            {
                ssPanel.SetSelection(col, row + 1);
            }

            // set arrow keys as input keys
            if ((e.KeyCode == Keys.Left) ||
                (e.KeyCode == Keys.Up) ||
                (e.KeyCode == Keys.Right) ||
                (e.KeyCode == Keys.Down))
            {
                e.IsInputKey = true;
            }
            // Check for delete key
            else if (e.KeyCode == Keys.Delete)
            {
                // try to update the cell model, clearing this cell
                if ((contentsTextBox.Text != "") && (updateCellModel(row, col, "")))
                {
                    // update the status labels
                    displaySelection(ssPanel);
                }

                e.IsInputKey = true;
            }

            displaySelection(ssPanel);
        }

        /// <summary>
        /// Event handler for resizing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            //resize controls along with the form
            this.ssPanel.Width = this.Width - 24;
            this.ssPanel.Height = this.Height - 117;
            this.statusStrip.Width = this.Width - 16;
            this.statusStrip.Top = this.Height - 60;
            this.contentsTextBox.Width = this.Width - 130;
        }

        /// <summary>
        /// Updates the GUI to show the selected cell's information. Modified based on
        /// the demo code provided by Jim.
        /// 
        /// Activates when a different cell is selected.
        /// </summary>
        /// <param name="ss"></param>
        private void displaySelection(SpreadsheetPanel ss)
        {
            int row, col;
            String value;
            ss.GetSelection(out col, out row);
            ss.GetValue(col, row, out value);

            // Update the status labels
            cellLabel.Text = guiLang.cell + (char)('A' + col) + (row + 1);
            valueLabel.Text = guiLang.value + value;
            fileStatusLabel.Text = spreadsheetModel.Changed ? guiLang.unsavedChanges : guiLang.savedChanges ;

            // Fill contentsTextBox with the cell's data, and then set focus on it
            contentsTextBox.Text = getCellModelContents(row, col);

            // put the filename in the title bar if it is non-empty
            Text = guiLang.title + ((spreadsheetModel.Filename == null) ? "" : " - " + spreadsheetModel.Filename);
        }

        /// <summary>
        /// Event handler for text changes in the contents text box. Currently unused
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentsTextBox_TextChanged(object sender, EventArgs e)
        {
            // Check first character for '=' to determine if it's trying to be a Formula
        }

        // starts the progress bar marquee's animation
        private void StartMarquee()
        {
            statusProgressBar.BeginInvoke(new Action(() => { statusProgressBar.Style = ProgressBarStyle.Marquee; }));
        }
        // stops the progress bar marquee's animation
        private void StopMarquee()
        {
            statusProgressBar.BeginInvoke(new Action(() => { statusProgressBar.Style = ProgressBarStyle.Blocks; }));
        }

        /// <summary>
        /// Event handler for key presses within the contents text box. Handles the enter key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check for Enter key
            if (e.KeyCode == Keys.Enter)
            {
                // get current row and column
                int col, row;
                ssPanel.GetSelection(out col, out row);

                // try to update the cell model if new contents are different than current contents
                if (contentsTextBox.Text.Trim().ToUpper().Replace(" ", String.Empty).Replace("\n", "") != getCellModelContents("" + (char)('A' + col) + (row + 1)))
                {
                    if (updateCellModel(row, col, contentsTextBox.Text.Trim().Replace("\n", "")))
                    {
                        // return focus to the spreadsheetPanel
                        ssPanel.BeginInvoke(new Action(() => { ssPanel.Focus(); }));

                        // update the status labels
                        displaySelection(ssPanel);
                    }

                    // we handled the character input
                    //e.Handled = true;
                }
                else
                {
                    // return focus to the spreadsheetPanel
                    ssPanel.BeginInvoke(new Action(() => { ssPanel.Focus(); }));
                }
            }
        }

        /// <summary>
        /// Updates a single cell's visual appearance
        /// </summary>
        /// <param name="cellname"></param>
        /// <param name="value"></param>
        private void updateCellView(string cellname, string value)
        {
            int row;
            int col = cellname.ToCharArray()[0] - 'A';
            int.TryParse(cellname.Substring(1), out row);

            // set cell to input value (row - 1 is because of 1-based indexing)
            ssPanel.SetValue(col, row - 1, value);
        }

        /// <summary>
        /// Event handler for File > New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFile();
        }

        /// <summary>
        /// Event handler for File > Save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        /// <summary>
        /// Event handler for File > Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open();
        }

        /// <summary>
        /// Event handler for File > Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // close this spreadsheet
            this.Close();
        }

        /// <summary>
        /// Gets the languages that are available to use with the GUI
        /// </summary>
        private void getLanguages()
        {
            // initialize the language set
            LangSet = new Dictionary<String, GUILanguage>();

            // create a set of all found GUI languages
            foreach (string filename in Directory.GetFiles(Application.StartupPath))
            {
                if (Regex.IsMatch(filename, ".glx$"))
                {
                    GUILanguage guiLang = new GUILanguage(filename);
                    LangSet.Add(guiLang.languageName, guiLang);
                }
            }

            // if there is more than one, allow the user to choose
            if (LangSet.Count > 1)
            {
                // the options menu is usually hidden, but if other language GUI xmls
                // were detected, then show it so the languages can be accessed
                optionsToolStripMenuItem.Visible = false; // CHANGED FOR SERVER VERSION, WILL NOT SHOW OTHER LANGUAGES NOW

                List<ToolStripItem> languages = new List<ToolStripItem>();

                // add a menu option for each language, and give it a click event
                foreach (KeyValuePair<String, GUILanguage> lang in LangSet)
                {
                    ToolStripItem ts = new ToolStripMenuItem(lang.Key);
                    ts.Click += ts_Click;
                    languageToolStripMenuItem.DropDownItems.Add(ts);
                }
            }
        }

        /// <summary>
        /// Event handler for dynamically-generated language menu choices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ts_Click(object sender, EventArgs e)
        {
            ToolStripItem ts = (ToolStripItem)sender;

            // set the language to the menu item chosen
            setLang(ts.Text);
        }

        /// <summary>
        /// Event handler for when the form loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Event handler for Help > How to use Spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void howToUseSpreadsheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // show a messagebox with instructions on how to use the spreadsheet
            MessageBox.Show(guiLang.howToUseMessage, guiLang.title);
        }

        /// <summary>
        /// Event handler for key presses in the spreadsheet panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ssPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // move focus to contentsTextBox is Enter is pressed
            if (e.KeyCode == Keys.Enter)
                contentsTextBox.BeginInvoke(new Action(() => { contentsTextBox.Focus(); }));

            displaySelection(ssPanel);
        }

        //VIEW ENDS HERE

        /// <summary>
        /// Creates a new file in the current window
        /// </summary>
        private void newFile()
        {
            // show the FileView window to open another spreadsheet
            FileViewHandle.Activate();
            FileViewHandle.ResetControls();
            FileViewHandle.FocusCreateFile();
        }

        /// <summary>
        /// Handles a successful Save event
        /// </summary>
        private void handleSaved()
        {
            // unlock user input
            unlockInput();

            // tell the model it has no unsaved changes
            spreadsheetModel.Save();
        }

        /// <summary>
        /// Handles an ErrorMessage event
        /// </summary>
        private void handleErrorMessage(String message)
        {
            // show message to user
            MessageBox.Show(message);
        }

        /// <summary>
        /// Handles an Update event
        /// </summary>
        public void handleUpdate(String _version, SyncCell cell)
        {
            int tempNew = 0;
            int tempOld = 0;

            Int32.TryParse(_version, out tempNew);
            Int32.TryParse(version, out tempOld);

            // check to see if spreadsheet is out of sync. If so, send the resync command and wait for the sync to happen
            if (tempOld < tempNew - 1)
            {
                msgHand.Resync();
                return;
            }

            // set the new version of the cell
            version = _version;

            // update the cell
            foreach (string vcell in spreadsheetModel.SetContentsOfCell(cell.Name, cell.Contents))
            {
                // try to update the view for each cell
                Object returnValue = spreadsheetModel.GetCellValue(vcell);
                if (returnValue.GetType() == typeof(FormulaError))
                {
                    updateCellView(vcell, ((FormulaError)returnValue).Reason);
                }
                else
                {
                    updateCellView(vcell, returnValue.ToString());
                }
            }

            // recalculate the view
            updateAllCells();

            // allow the user to enter changes again
            unlockInput();
        }

        private void lockInput()
        {
            lock (spreadsheetModel)
            {
                locked = true;
                ssPanel.BeginInvoke(new Action(() => { ssPanel.Enabled = false; }));
                contentsTextBox.BeginInvoke(new Action(() => { contentsTextBox.ReadOnly = true; }));
                StartMarquee();
            }
        }

        private void unlockInput()
        {
            lock (spreadsheetModel)
            {
                locked = false;
                if (ssPanel.IsHandleCreated)
                {
                    ssPanel.BeginInvoke(new Action(() => { ssPanel.Enabled = true; }));
                }
                if (contentsTextBox.IsHandleCreated)
                {
                    contentsTextBox.BeginInvoke(new Action(() => { contentsTextBox.ReadOnly = false; }));
                }
                StopMarquee();
            }
        }

        /// <summary>
        /// Handles a Sync event
        /// </summary>
        private void handleSync(String _version, IEnumerable<SyncCell> cells)
        {
            // set the new version of the cell
            version = _version;

            // clear the model
            spreadsheetModel = new Spreadsheet(validate, s => s.ToUpper(), "PS6");

            // update the model for each cell
            foreach (SyncCell cell in cells)
                spreadsheetModel.DirectSetCell(cell.Name, cell.Contents);

            // put the cursor back in the spreadsheet
            if (ssPanel.IsHandleCreated)
            {
                ssPanel.BeginInvoke(new Action(() => { ssPanel.Focus(); }));
            }

            // recalculate all cells since we dummied some stuff out with zeroes
            hardResetAllCells();

            // allow the user to enter changes again
            unlockInput();
        }

        /// <summary>
        /// Checks for unsaved changes, and prompts the user about them if they are present
        /// </summary>
        /// <returns>Whether or not to stop due to unsaved changes</returns>
        private bool stopForUnsavedChanges()
        {
            // check to see if the spreadsheet has unsaved changes
            if (spreadsheetModel.Changed)
            {
                // ask the user if they want to proceed despite unsaved changes
                DialogResult result = MessageBox.Show(guiLang.unsavedChangesMessage, guiLang.title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                // if they do not want to proceed, return true
                return result == DialogResult.No;
            }

            // otherwise, do not stop for unsaved changes
            return false;
        }

        /// <summary>
        /// Clears the view and then refills it with all non-empty cells
        /// </summary>
        private void updateAllCells()
        {
            // clear all existing cells
            ssPanel.Clear();

            // add all new cells
            foreach (string cell in spreadsheetModel.GetNamesOfAllNonemptyCells())
            {
                updateCellView(cell, spreadsheetModel.GetCellValue(cell).ToString());
            }
        }

        /// <summary>
        /// Clears the view and then refills it with all non-empty cells
        /// </summary>
        private void hardResetAllCells()
        {
            // clear all existing cells
            ssPanel.Clear();

            // add all new cells
            foreach (string cell in spreadsheetModel.GetNamesOfAllNonemptyCells())
            {
                updateCellView(cell, spreadsheetModel.RecalculateCellValue(cell).ToString());
            }
        }

        /// <summary>
        /// Opens a file using an open file dialog
        /// </summary>
        private void open()
        {
            // show the FileView window to open another spreadsheet
            FileViewHandle.Activate();
            FileViewHandle.ResetControls();
        }

        /// <summary>
        /// Tries to save with current filename, uses saveAs if no filename exists
        /// </summary>
        private void save()
        {
            // don't save yet if input is already locked
            if (locked)
                return;

            // lock user input until the save completes
            lockInput();

            // send the save message to the server
            msgHand.Save(this.filename);
            
            // update the status labels and move focus to spreadsheetPanel
            displaySelection(ssPanel);
            ssPanel.Focus();
        }

        /// <summary>
        /// Saves a spreadsheet to the input filename
        /// </summary>
        /// <param name="filename"></param>
        private void saveSpreadsheetModel(string filename)
        {
            spreadsheetModel.Save(filename);
        }

        /// <summary>
        /// Updates the data in a cell in the model
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="text">The data to put in the cell</param>
        /// <returns></returns>
        private bool updateCellModel(int row, int col, string text)
        {
            // send the new cell contents to the server
            return updateCellModel(new SyncCell("" + (char)('A' + col) + (row + 1), text));
        }

        /// <summary>
        /// Updates the data in a cell in the model
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="text">The data to put in the cell</param>
        /// <returns></returns>
        private bool updateCellModel(SyncCell cell)
        {
            if ((cell.Contents.Length > 0) && (cell.Contents[0] == '='))
            {
                // get the name of the first invalid field--if any
                String invalidField = Formula.FindInvalidField(cell.Contents.Substring(1, cell.Contents.Length - 1), s => s.ToUpper(), validate, spreadsheetModel.Lookup);

                // check if all fields in the Formula are valid before trying to send it
                if (invalidField != "")
                {
                    MessageBox.Show(invalidField + " contains a string value, it may not be referenced in a formula.");
                    return false;
                }

                try
                {
                    // remove the leading '=' when trying to parse as a Formula
                    Formula formulaValue = new Formula(cell.Contents.Substring(1, cell.Contents.Length - 1), s => s.ToUpper(), validate);
                }
                catch (FormulaFormatException e)
                {
                    MessageBox.Show(e.Message);
                    return false;
                }
            }

            // if the spreadsheet is locked, do nothing
            if (locked)
                return false;

            // prevent the user from entering any more input until the server responds
            lockInput();

            // send the new cell contents to the server
            msgHand.EnterChange(cell);

            return true;
        }

        /// <summary>
        /// Returns the contents of a cell from the model
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private string getCellModelContents(int row, int col)
        {
            return getCellModelContents("" + (char)('A' + col) + (row + 1));
        }

        private string getCellModelContents(String name)
        {
            Object contents = spreadsheetModel.GetCellContents(name);

            // if the contents are a formula, they need a leading '='
            if (contents.GetType() == typeof(Formula))
                return "=" + contents.ToString();

            return contents.ToString();
        }

        /// <summary>
        /// Validates cell names by verifying that they follow the pattern: 1 letter
        /// followed by 1 or 2 numbers, the first of which cannot be zero
        /// </summary>
        /// <param name="cellname"></param>
        /// <returns></returns>
        private bool validate(string token)
        {
            // check if token is a variable
            if (Regex.IsMatch(token, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
            {
                // if length > 3, then either the column name is two letters (invalid)
                // or the row number is >99 (invalid)
                if (token.Length > 3)
                    return false;

                // if length < 2, then it's either missing a row or column (invalid)
                if (token.Length < 2)
                    return false;

                // check if the first character is a letter and the second and third
                // characters are numbers and correspond to a nonzero value
                if (!Regex.IsMatch(token, "^[a-zA-Z][1-9][0-9]?$"))
                    return false;
            }

            // if it isn't a variable, or didn't fail above, it's valid
            return true;
        }

        /// <summary>
        /// Sets the current language the GUI should show
        /// </summary>
        /// <param name="language"></param>
        private void setLang(string language)
        {
            GUILanguage newLang;

            // Enable IME if not default language
            if (language != "Default")
            {
                this.ImeMode = ImeMode.On;
                LangSet.TryGetValue(language, out newLang);
            }
            else
            // if it is the default, create a language using the default constructor
            {
                newLang = new GUILanguage();
            }

            // set the file menu
            fileToolStripMenuItem.Text = newLang.fileMenu;
            newToolStripMenuItem.Text = newLang.newMenu;
            openToolStripMenuItem.Text = newLang.openMenu;
            saveToolStripMenuItem.Text = newLang.saveMenu;
            closeToolStripMenuItem.Text = newLang.closeMenu;

            // set the options menu
            optionsToolStripMenuItem.Text = newLang.optionsMenu;
            languageToolStripMenuItem.Text = newLang.languageMenu;

            //set the help menu
            helpToolStripMenuItem.Text = newLang.helpMenu;
            howToUseSpreadsheetToolStripMenuItem.Text = newLang.howToUseMenu;

            //set the gui's language to the new language
            guiLang = newLang;

            // update spreadsheet display to new language
            displaySelection(ssPanel);
        }

        // handles the undo menu item
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undo();
        }

        // sends an undo command to the server
        private void undo()
        {
            if (!locked)
            {
                // send the undo command with the spreadsheet's current version
                msgHand.Undo(version);
            }
        }
    }

    /// <summary>
    /// Represents the GUI text in a single language
    /// </summary>
    public class GUILanguage
    {
        // language name
        public readonly string languageName;

        // spreadsheet program name
        public readonly string title;

        // labels and messages
        public readonly string cell;
        public readonly string value;
        public readonly string unsavedChanges;
        public readonly string savedChanges;
        public readonly string unsavedChangesMessage;

        // file menu
        public readonly string fileMenu;
        public readonly string newMenu;
        public readonly string openMenu;
        public readonly string saveMenu;
        public readonly string closeMenu;

        // options menu
        public readonly string optionsMenu;
        public readonly string languageMenu;

        // help menu
        public readonly string helpMenu;
        public readonly string howToUseMenu;
        public readonly string howToUseMessage;

        public GUILanguage()
        {
            // language name
            languageName = "Default";

            // set spreadsheet program name
            title = "Spreadsheet";

            // set labels and messages
            cell = "Cell: ";
            value = "Value: ";
            unsavedChanges = "Unsaved changes";
            savedChanges = "Saved";
            unsavedChangesMessage = "There are unsaved changes that will be lost. Proceed?";

            // set file menu
            fileMenu = "File";
            newMenu = "New";
            openMenu = "Open";
            saveMenu = "Save";
            closeMenu = "Close";

            // set options menu
            optionsMenu = "Options";
            languageMenu = "Language";

            //set help menu
            helpMenu = "Help";
            howToUseMenu = "How to use Spreadsheet";
            howToUseMessage = "Use the arrow keys or mouse to select cells in the spreadsheet. When in the grid, use the Enter key to edit a cell's contents in the text box above, or the Delete key to delete a cell's contents. When in the cell contents box, use Enter to submit the value to the spreadsheet.";
        }

        public GUILanguage(string filename)
        {
            try
            {
                XmlReader xReader = XmlReader.Create(filename);

                // get language name
                xReader.ReadToFollowing("languageName");
                languageName = xReader.ReadElementContentAsString();

                // get program name
                xReader.ReadToFollowing("title");
                title = xReader.ReadElementContentAsString();

                // get labels and messages
                xReader.ReadToFollowing("cell");
                cell = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("value");
                value = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("unsavedChanges");
                unsavedChanges = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("savedChanges");
                savedChanges = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("unsavedChangesMessage");
                unsavedChangesMessage = xReader.ReadElementContentAsString();

                // file menu
                xReader.ReadToFollowing("fileMenu");
                fileMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("newMenu");
                newMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("openMenu");
                openMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("saveMenu");
                saveMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("closeMenu");
                closeMenu = xReader.ReadElementContentAsString();

                // options menu
                xReader.ReadToFollowing("optionsMenu");
                optionsMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("languageMenu");
                languageMenu = xReader.ReadElementContentAsString();

                // help menu
                xReader.ReadToFollowing("helpMenu");
                helpMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("howToUseMenu");
                howToUseMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("howToUseMessage");
                howToUseMessage = xReader.ReadElementContentAsString();
                
                // close the XML reader
                xReader.Close();
            }
            catch
            {

            }
        }
    }
}
