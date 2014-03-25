using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        DependencyGraph cellGraph;
        Dictionary<string, Cell> cellSet;

        /// <summary>
        /// Constructs a new, empty spreadsheet
        /// </summary>
        public Spreadsheet()
        {
            cellGraph = new DependencyGraph();
            cellSet = new Dictionary<string, Cell>();
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

            // check name for null-string and invalid format
            try
            {
                checkName(name);
            }
            catch
            {
                throw new InvalidNameException();
            }

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
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<String> SetCellContents(String name, double number)
        {
            HashSet<string> returnSet = new HashSet<string>();
            Cell editCell;

            // check name for null-string and invalid format
            try
            {
                checkName(name);
            }
            catch
            {
                throw new InvalidNameException();
            }

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
                cellSet.Add(name, new Cell(name, number));
            }

            // remove the cell's dependees and update its return set
            cellGraph.ReplaceDependees(name, new List<string>());
            foreach (string cell in GetCellsToRecalculate(name))
            {
                // the named cell has already been added at the beginning, don't add it twice
                if (cell != name)
                {
                    returnSet.Add(cell);
                }
            }

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
        public override ISet<String> SetCellContents(String name, String text)
        {
            HashSet<string> returnSet = new HashSet<string>();
            Cell editCell;
            
            // check for null text
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            // check name for null-string and invalid format
            try
            {
                checkName(name);
            }
            catch
            {
                throw new InvalidNameException();
            }

            // add the cell to the return set
            returnSet.Add(name);

            // try to get an existing cell with this name. If none exists, create one
            if (cellSet.TryGetValue(name, out editCell))
            {
                // set the new text equal to the passed-in value
                editCell.Contents = text;
            }
            else
            {
                // add a newly-created cell
                cellSet.Add(name, new Cell(name, text));
            }

            // remove the cell's dependees and update its return set
            cellGraph.ReplaceDependees(name, new List<string>());
            foreach (string cell in GetCellsToRecalculate(name))
            {
                // the named cell has already been added at the beginning, don't add it twice
                if (cell != name)
                {
                    returnSet.Add(cell);
                }
            }

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
        public override ISet<String> SetCellContents(String name, Formula formula)
        {
            HashSet<string> returnSet = new HashSet<string>();
            List<string> backupDependendees = new List<string>();
            Cell editCell;

            // check for null Formula
            if (formula == null)
            {
                throw new ArgumentNullException();
            }

            // check name for null-string and invalid format
            try
            {
                checkName(name);
            }
            catch
            {
                throw new InvalidNameException();
            }

            // add the cell to the return set
            returnSet.Add(name);

            // backup the cell's dependencies in case of a formula exception
            foreach (string cell in cellGraph.GetDependees(name))
            {
                backupDependendees.Add(cell);
            }

            // update the cell's dependencies
            cellGraph.ReplaceDependees(name, formula.GetVariables());

            // try to calculate the return set. Undo changes if an exception is thrown
            try
            {
                // get cells for recalculation and check for circular dependencies
                foreach (string cell in GetCellsToRecalculate(name))
                {
                    // the named cell has already been added at the beginning, don't add it twice
                    if (cell != name)
                    {
                        returnSet.Add(cell);
                    }
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
                cellSet.Add(name, new Cell(name, formula));
            }

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
            // check name for null-string and invalid format
            checkName(name);

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
        /// <returns>True if valid, throws either ArgumentNullException or InvalidNameException if invalid</returns>
        private void checkName(String name)
        {
            // test for a null-string name
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            // test for invalid variable names
            if (!Formula.isLegalVariableName(name))
            {
                throw new InvalidNameException();
            }
            
            // both tests passed, name must be non-null and valid
            return;
        }
    }

    public class Cell
    {
        public string Name { get; private set; }
        public Object Contents {get; set;}
        Object Value;

        public Cell(string _name, Object _Contents)
        {
            Name = _name;
            Contents = _Contents;
            Value = "";
        }
    }
}
