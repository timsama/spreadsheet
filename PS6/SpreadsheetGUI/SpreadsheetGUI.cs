﻿using System;
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

        private GUILanguage guiLang;

        /// <summary>
        /// Initialize the form and open a file
        /// </summary>
        /// <param name="filename"></param>
        public MainForm(string filename)
        {
            InitializeComponent();

            //set the underlying model to the file
            spreadsheetModel = new Spreadsheet(filename, validate, s => s.ToUpper(), "PS6");

            //get GUI languages that are available
            getLanguages();

            //set the GUI language
            guiLang = new GUILanguage();
            setLang("Default");

            //set up some event handlers
            ssPanel.SelectionChanged += displaySelection;
            ssPanel.PreviewKeyDown += ssPanel_PreviewKeyDown;
            
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
        }

        /// <summary>
        /// Check for unsaved changes when closing the spreadsheet
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = stopForUnsavedChanges();
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
                // clear the contentsTextBox
                // contentsTextBox.Text = "";

                // try to update the cell model, clearing this cell
                if (updateCellModel(row, col, ""))
                {
                    // update the status labels
                    displaySelection(ssPanel);
                }

                e.IsInputKey = true;
            }

            displaySelection(ssPanel);
        }        

        private void ssPanel_Load(object sender, EventArgs e)
        {

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
            this.contentsTextBox.Width = this.Width - 40;
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

                // try to update the cell model
                if (updateCellModel(row, col, contentsTextBox.Text))
                {
                    // return focus to the spreadsheetPanel
                    ssPanel.Focus();

                    // update the status labels
                    displaySelection(ssPanel);
                }

                // we handled the character input
                e.Handled = true;
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
        /// Event handler for File > Save As
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
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
        /// Event handler for File > Open in New Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openInNewWindow();
        }

        /// <summary>
        /// Event handler for File > Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                optionsToolStripMenuItem.Visible = true;

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
                contentsTextBox.Focus();

            displaySelection(ssPanel);
        }

        //VIEW ENDS HERE

        /// <summary>
        /// Creates a new file in the current window
        /// </summary>
        private void newFile()
        {
            if (stopForUnsavedChanges())
                return;

            // create a blank spreadsheet
            spreadsheetModel = new Spreadsheet(validate, s => s.ToUpper(), "PS6");

            // update the view
            updateAllCells();
            displaySelection(ssPanel);
            openFileDialog1.FileName = "";
            saveFileDialog1.FileName = "";
            
            // move focus to spreadsheetPanel
            ssPanel.Focus();
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
        /// Opens a file using an open file dialog
        /// </summary>
        private void open()
        {
            if (stopForUnsavedChanges())
                return;

            // create a filtered OpenFileDialog
            openFileDialog1.Filter = "Spreadsheet files (*.ss)|*.ss|All files (*.*)|*.*";
            openFileDialog1.ShowDialog(this);

            // if filename is non-blank open the selected file
            if (openFileDialog1.FileName != "")
                spreadsheetModel = new Spreadsheet(openFileDialog1.FileName, validate, s => s.ToUpper(), "PS6");

            // update the view
            updateAllCells();
            displaySelection(ssPanel);

            // move focus to spreadsheetPanel
            ssPanel.Focus();
        }

        /// <summary>
        /// Opens a spreadsheet in a new window using an open file dialog
        /// </summary>
        private void openInNewWindow()
        {
            // create a filtered OpenFileDialog
            openFileDialog1.Filter = "Spreadsheet files (*.ss)|*.ss|All files (*.*)|*.*";
            openFileDialog1.ShowDialog(this);

            // if filename is non-blank create a new window, opening the named file
            if (openFileDialog1.FileName != "")
                ViewApplicationContext.getAppContext().RunForm(new MainForm(openFileDialog1.FileName));
        }

        /// <summary>
        /// Tries to save with current filename, uses saveAs if no filename exists
        /// </summary>
        private void save()
        {
            // if no filename already exists, treat as saveAs. Otherwise, save file
            if ((spreadsheetModel.Filename == null) || (spreadsheetModel.Filename == ""))
                saveAs();
            else
                spreadsheetModel.Save(spreadsheetModel.Filename);
            
            // update the status labels and move focus to spreadsheetPanel
            displaySelection(ssPanel);
            ssPanel.Focus();
        }

        /// <summary>
        /// Saves a spreadsheet by using a save file dialog
        /// </summary>
        private void saveAs()
        {
            // set up the filter to add .ss if (*.ss) is selected and it's not already present
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "Spreadsheet files (*.ss)|*.ss|All files (*.*)|*.*";

            // show the save file dialog
            saveFileDialog1.ShowDialog(this);

            // if filename is non-blank save the spreadsheet
            if (saveFileDialog1.FileName != "")
            {
                saveSpreadsheetModel(saveFileDialog1.FileName);
                spreadsheetModel.Filename = saveFileDialog1.FileName;
            }

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
            List<string> returnSet = new List<string>();

            // try to update the model with the new content
            try
            {
                foreach (string cell in spreadsheetModel.SetContentsOfCell("" + (char)('A' + col) + (row + 1), text))
                {
                    // try to update the view for each cell
                    Object returnValue = spreadsheetModel.GetCellValue(cell);
                    if (returnValue.GetType() == typeof(FormulaError))
                    {
                        MessageBox.Show(((FormulaError)returnValue).Reason);
                        updateCellView(cell, ((FormulaError)returnValue).Reason);
                    }
                    else
                    {
                        updateCellView(cell, returnValue.ToString());
                    }
                }
            }
            catch (CircularException)
            {
                // handle circular exception
                MessageBox.Show("The entered formula contains a circular dependency. Please modify it and try again.");
            }
            catch (Exception e)
            {
                // handle all other exceptions
                MessageBox.Show(e.Message);
                return false;
            }

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
            Object contents = spreadsheetModel.GetCellContents("" + (char)('A' + col) + (row + 1));

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
            openInNewWindowToolStripMenuItem.Text = newLang.openInNewWindowMenu;
            saveToolStripMenuItem.Text = newLang.saveMenu;
            saveAsToolStripMenuItem.Text = newLang.saveAsMenu;
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
        public readonly string openInNewWindowMenu;
        public readonly string saveMenu;
        public readonly string saveAsMenu;
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
            openInNewWindowMenu = "Open in New Window";
            saveMenu = "Save";
            saveAsMenu = "Save As...";
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
                xReader.ReadToFollowing("openInNewWindowMenu");
                openInNewWindowMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("saveMenu");
                saveMenu = xReader.ReadElementContentAsString();
                xReader.ReadToFollowing("saveAsMenu");
                saveAsMenu = xReader.ReadElementContentAsString();
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
