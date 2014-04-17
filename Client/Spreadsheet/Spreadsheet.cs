using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Xml;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        DependencyGraph cellGraph;
        Dictionary<string, Cell> cellSet;
        public string Filename { get; set; }

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// Constructs a spreadsheet based on an existing file
        /// </summary>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            GetSavedVersion(filepath);
            Filename = filepath;
            if (this.Version.ToUpper() != version.ToUpper())
                throw new SpreadsheetReadWriteException("Version information in constructor does not match file version.");
        }
        
        /// <summary>
        /// Constructs a new, empty spreadsheet
        /// </summary>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cellGraph = new DependencyGraph();
            cellSet = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Constructs a new, empty spreadsheet with no validation or normalization
        /// </summary>x
        public Spreadsheet()
            : base(s => true, s => s, "default")
        {
            cellGraph = new DependencyGraph();
            cellSet = new Dictionary<string, Cell>();
            Changed = false;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<String> GetNamesOfAllNonemptyCells()
        {
            // iterate through the set of cells
            foreach (KeyValuePair<string, Cell> cell in cellSet)
            {
                // yield return each cell that is not empty
                if (! cell.Value.Contents.Equals(""))
                {
                    yield return cell.Value.Name;
                }
            }
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(String name)
        {
            Cell temp;

            //normalize name
            name = Normalize(name);

            // check name for null-string and invalid format
            checkName(name);

            // try to get the cell from the cell set
            if (cellSet.TryGetValue(name, out temp))
            {
                // found the cell, return its contents
                return temp.Contents;
            }
            else
            {
                // cell has not been created (i.e. is empty), return empty string
                return "";
            }
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            double dblValue;
            HashSet<string> returnSet = new HashSet<string>();

            // check for null content
            if (content == null)
            {
                throw new ArgumentNullException("Cannot set cell \"" + name + "\" to a null value.");
            }

            // normalize the name
            Normalize(name);

            // check name for null-string and invalid format
            checkName(name);

            // try to parse the content as a double
            if (Double.TryParse(content, out dblValue))
            {
                // add each cell to the return set
                foreach (string cellname in SetCellContents(name, dblValue))
                    returnSet.Add(cellname);
            }
            // if the first character is '=' then try to parse the content as a Formula
            else if ((content.Length > 0) && (content.ElementAt(0) == '='))
            {
                // remove the leading '=' when trying to parse as a Formula
                Formula formulaValue = new Formula(content.Substring(1, content.Length - 1), Normalize, IsValid);

                // add each cell to the return set
                foreach (string cellname in SetCellContents(name, formulaValue))
                    returnSet.Add(cellname);
            }
            // otherwise, set the content as a string
            else
            {
                // add each cell to the return set
                foreach (string cellname in SetCellContents(name, content))
                    returnSet.Add(cellname);
            }

            Cell editCell;

            // each cell in the return set depends on this cell, so they need to be recalculated
            foreach (string cellname in returnSet)
            {
                if (cellSet.TryGetValue(cellname, out editCell))
                {
                    editCell.NeedToRecalculate = true;
                }
            }

            return returnSet;
        }

        /// <summary>
        /// Sets the contents of a cell directly, with no error checking (used for Server Syncing)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public void DirectSetCell(String name, String content)
        {
            double dblValue;

            // normalize the name
            Normalize(name);

            // check name for null-string and invalid format
            checkName(name);

            // try to parse the content as a double
            if (Double.TryParse(content, out dblValue))
            {
                DirectSet(name, dblValue);
            }
            // if the first character is '=' then try to parse the content as a Formula
            else if ((content.Length > 0) && (content.ElementAt(0) == '='))
            {
                // remove the leading '=' when trying to parse as a Formula
                Formula formulaValue = new Formula(content.Substring(1, content.Length - 1), Normalize, IsValid);
                DirectSet(name, formulaValue);
            }
            // otherwise, set the content as a string
            else
            {
                DirectSet(name, content);
            }
        }

        protected void DirectSet(String name, double number)
        {
            Cell editCell;

            // check name for null-string and invalid format
            checkName(name);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new double value equal to the passed-in value
                editCell.Contents = number;
            }
            else
            {
                // add a newly-created cell
                cellSet.Add(name, new Cell(name, number, Normalize, Lookup));
            }
        }

        protected void DirectSet(String name, String text)
        {
            Cell editCell;

            // check name for null-string and invalid format
            checkName(name);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new text equal to the passed-in value
                editCell.Contents = text;
            }
            // if the string is empty, no need to create a new cell
            else if (text != "")
            {
                // add a newly-created cell
                cellSet.Add(name, new Cell(name, text, Normalize, Lookup));
            }
        }

        protected void DirectSet(String name, Formula formula)
        {
            List<string> formulaVariables = new List<string>();
            Cell editCell;

            // check name for null-string and invalid format
            checkName(name);

            // normalize the formula's variable names
            foreach (string variable in formula.GetVariables())
            {
                formulaVariables.Add(Normalize(variable));
            }

            // update the cell's dependencies
            cellGraph.ReplaceDependees(name, formulaVariables);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new formula equal to the passed-in value
                editCell.Contents = formula;
            }
            else
            {
                // add a newly-created cell since we couldn't find one
                cellSet.Add(name, new Cell(name, formula, Normalize, Lookup));
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, double number)
        {
            HashSet<string> returnSet = new HashSet<string>();
            Cell editCell;

            // check name for null-string and invalid format
            checkName(name);

            // add the cell to the return set
            returnSet.Add(name);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new double value equal to the passed-in value
                editCell.Contents = number;
            }
            else
            {
                // add a newly-created cell
                cellSet.Add(name, new Cell(name, number, Normalize, Lookup));
            }

            // remove the cell's dependees
            cellGraph.ReplaceDependees(name, new List<string>());
            
            // update the cell's return set
            foreach (string cell in GetCellsToRecalculate(name))
            {
                returnSet.Add(cell);
            }

            // the spreadsheet has changed
            Changed = true;

            return returnSet;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, String text)
        {
            HashSet<string> returnSet = new HashSet<string>();
            Cell editCell;
            
            // check for null text; should never be true, but needed in specification
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            // check name for null-string and invalid format
            checkName(name);

            // if the string is empty, no need to store the cell
            if (text == "")
                cellSet.Remove(name);

            // add the cell to the return set
            returnSet.Add(name);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new text equal to the passed-in value
                editCell.Contents = text;
            }
            // if the string is empty, no need to create a new cell
            else if (text != "")
            {
                // add a newly-created cell
                cellSet.Add(name, new Cell(name, text, Normalize, Lookup));
            }

            // cells that currently depend on this cell need to be updated
            foreach (string cell in cellGraph.GetDependents(name))
            {
                returnSet.Add(cell);
            }

            // remove the cell's dependees, since a string cell can't depend on other cells
            cellGraph.ReplaceDependees(name, new List<string>());
            
            // the spreadsheet has changed
            Changed = true;

            // return the set of cells to update
            return returnSet;
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<String> SetCellContents(String name, Formula formula)
        {
            HashSet<string> returnSet = new HashSet<string>();
            List<string> backupDependendees = new List<string>();
            List<string> formulaVariables = new List<string>();
            Cell editCell;

            // check for null Formula; should never be true, but needed in specification
            if (formula == null)
            {
                throw new ArgumentNullException();
            }

            // check name for null-string and invalid format
            checkName(name);

            // add the cell to the return set
            returnSet.Add(name);

            // backup the cell's dependencies in case of a formula exception
            foreach (string cell in cellGraph.GetDependees(name))
            {
                backupDependendees.Add(cell);
            }

            // normalize the formula's variable names
            foreach (string variable in formula.GetVariables())
            {
                formulaVariables.Add(Normalize(variable));
            }

            // update the cell's dependencies
            cellGraph.ReplaceDependees(name, formulaVariables);

            // try to calculate the return set. Undo changes if an exception is thrown
            try
            {
                // get cells for recalculation and check for circular dependencies
                foreach (string cell in GetCellsToRecalculate(name))
                {
                    returnSet.Add(cell);
                }
            }
            catch (Exception e)
            {
                // an exception was thrown by GetCellsToRecalculate, undo changes
                cellGraph.ReplaceDependees(name, backupDependendees);
                
                // rethrow exception
                throw e;
            }

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new formula equal to the passed-in value
                editCell.Contents = formula;
            }
            else
            {
                // add a newly-created cell since we couldn't find one
                cellSet.Add(name, new Cell(name, formula, Normalize, Lookup));
            }

            // the spreadsheet has changed
            Changed = true;
            
            // return the set of cells affected by this change
            return returnSet;
        }


        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<String> GetDirectDependents(String name)
        {
            // test for a null-string name; should never be true but needed in specification
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            // test for illegal variable names; should never be true but needed in specification
            if (!Formula.isLegalVariableName(name))
            {
                throw new InvalidNameException();
            }

            // test for invalid variable names; should never be true but needed in specification
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }

            // yield return each direct dependent of the named cell
            foreach (string cell in cellGraph.GetDependents(name))
            {
                yield return cell;
            }
        }

        /// <summary>
        /// Tests if a name string is null or an invalid variable name
        /// </summary>
        /// <param name="name">String to be tested</param>
        /// <returns>Throws InvalidNameException if invalid</returns>
        private void checkName(String name)
        {
            // test for a null-string name
            if (name == null)
            {
                throw new InvalidNameException();
            }

            // test for illegal variable names
            if (!Formula.isLegalVariableName(name))
            {
                throw new InvalidNameException();
            }

            // test for invalid variable names
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }
            
            // both tests passed, name must be non-null and valid
            return;
        }

        /// <summary>
        /// Helper lookup function for evaluating cell values
        /// </summary>
        /// <param name="name">Name of cell to look up</param>
        /// <returns>Value of the cell, or throws an ArgumentException</returns>
        public double Lookup(String name)
        {
            // temporarily store the object value of the named cell
            Object result = GetCellValue(name);

            // if the result type is double, return it
            if (result.GetType() == typeof(double))
                return (double)result;

            // otherwise, throw an exception indicating its type; unknown type should never execute but exists for debugging purposes
            if (result.GetType() == typeof(string))
                throw new ArgumentException("Unhandled exception: value of cell \"" + name + "\" is of type string.");
            else if (result.GetType() == typeof(FormulaError))
                throw new ArgumentException("Unhandled exception: value of cell \"" + name + "\" is of type FormulaError.");
            else
                throw new ArgumentException("Unhandled exception: value of cell \"" + name + "\" is an unknown type.");
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(String name)
        {
            Cell editCell;

            // normalize the name
            name = Normalize(name);

            // check name for null-string and invalid format
            checkName(name);

            // try to get an existing cell with this name
            if (cellSet.TryGetValue(name, out editCell))
            {
                try
                {
                    return editCell.Value;
                }
                catch (ArgumentException)
                {
                    return "";
                }
            }
            // if none exists, treat it as an empty string
            else
            {
                return "";
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, recalculates and returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public object RecalculateCellValue(String name)
        {
            Cell editCell;

            // normalize the name
            name = Normalize(name);

            // check name for null-string and invalid format
            checkName(name);

            // try to get an existing cell with this name
            if (cellSet.TryGetValue(name, out editCell))
            {
                editCell.NeedToRecalculate = true;
                editCell.CalculateValue();
                return editCell.Value;
            }
            // if none exists, treat it as an empty string
            else
            {
                return "";
            }
        }

        // the AbstractSpreadsheet interface requires this
        public override void Save(string filename)
        {
            Save();
        }

        public void Save()
        {
            // freshly-saved spreadsheet means nothing has changed yet
            Changed = false;
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(String filename)
        {
            try
            {
                // open file for reading
                XmlReader xReader = XmlReader.Create(filename);
                string name, contents;

                // file open was successful; dump existing cell set and dependency graph
                cellGraph = new DependencyGraph();
                cellSet = new Dictionary<string, Cell>();

                try
                {
                    // begin reading spreadsheet tag
                    xReader.ReadToFollowing("spreadsheet");
                    Version = xReader.GetAttribute("version");

                    // loop through all cell tags, creating a Cell object for each
                    while (xReader.ReadToFollowing("cell"))
                    {
                        // get cell name from the file
                        xReader.ReadToFollowing("name");
                        name = xReader.ReadElementContentAsString();

                        //skip whitespace
                        while (xReader.NodeType == XmlNodeType.Whitespace)
                        {
                            xReader.Read();
                        }

                        // get cell contents from the file
                        contents = xReader.ReadElementContentAsString();

                        // create the new cell
                        SetContentsOfCell(name, contents);
                    }

                    // newly-opened spreadsheet means nothing has changed yet
                    Changed = false;

                    // set the current filename
                    Filename = filename;
                }
                catch
                {
                    throw new SpreadsheetReadWriteException("Error reading from spreadsheet file \"" + filename + "\".");
                }
                finally
                {
                    // close the file no matter what, throw an exception if it fails
                    try
                    {
                        xReader.Close();
                    }
                    // this block should never run, but exists for possible future debugging
                    catch
                    {
                        throw new SpreadsheetReadWriteException("Error closing spreadsheet file \"" + filename + "\".");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(SpreadsheetReadWriteException))
                {
                    // rethrow the exception from the inner block
                    throw e;
                }
                else
                {
                    // the file failed to open, throw an exception
                    throw new SpreadsheetReadWriteException("Error opening spreadsheet file \"" + filename + "\".");
                }
            }

            return Version;
        }
    }

    /// <summary>
    /// Represents a spreadsheet cell
    /// </summary>
    public class Cell
    {
        private Func<string, double> LookupDelegate;
        public string Name { get; private set; }
        public Object Contents { get; set; }
        public bool NeedToRecalculate { get; set; }
        private Object cellValue;
        public Object Value
        {
            get { CalculateValue(); return cellValue; }
            private set { cellValue = value; }
        }

        /// <summary>
        /// Constructs a new Cell object
        /// </summary>
        /// <param name="_name">Name of the cell</param>
        /// <param name="_Contents">Contents of the cell</param>
        /// <param name="Normalize">Delegate used to normalize cell name</param>
        /// <param name="Lookup">Delegate used to evaluate cell (if contents are a Formula)</param>
        public Cell(string _name, Object _Contents, Func<string, string> Normalize, Func<string, double> Lookup)
        {
            Name = Normalize(_name);
            Contents = _Contents;
            LookupDelegate = Lookup;
            Value = (double)0;
            NeedToRecalculate = false;
            //NeedToRecalculate = true;
            //CalculateValue();
        }

        /// <summary>
        /// Calculates the value of the cell based on its contents and dependees
        /// </summary>
        public void CalculateValue()
        {
            // check if the cell's dependees or self has changed
            if (NeedToRecalculate)
            {
                // if the cell's contents is a Formula, evaluate it
                if (Contents.GetType() == typeof(Formula))
                {
                    Value = ((Formula)Contents).Evaluate(LookupDelegate);
                }
                // if it's a double or a string, return it
                else
                {
                    Value = Contents;
                }

                // we've finished calculating, so we don't need to recalculate for now
                NeedToRecalculate = false;
            }
        }

        /// <summary>
        /// Writes the cell's contents as a string in the format of the appropriate:
        /// double.ToString
        /// string
        /// "=" + Formula.ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // if the contents are of type double, return the double as a string
            if (Contents.GetType() == typeof(double))
            {
                return Contents.ToString();
            }
            // if the contents are of type string, cast to string and return
            else if (Contents.GetType() == typeof(string))
            {
                return (string)Contents;
            }
            // if the contents are of type Formula, prepend a '=' and return as a string
            else if (Contents.GetType() == typeof(Formula))
            {
                return "=" + Contents.ToString();
            }
            // this should never be reached; it exists in case a non-valid value sneaks by
            else
            {
                throw new NotSupportedException("Unhandled exception: contents of cell \""+ Name + "\" is not of type double, string, or Formula.");
            }
        }
    }
}
