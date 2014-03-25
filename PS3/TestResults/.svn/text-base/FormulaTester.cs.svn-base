using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTester
{
    /// <summary>
    /// A class that represents a series of unit tests for the Formula class as well as functions to normalize
    /// and validate strings representing a formula.
    /// </summary>
    [TestClass]
    public class UnitTester
    {
        /// <summary>
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message.
        /// </summary>
        /// <param name="s">The string to normalize</param>
        /// <returns>A normalized copy of the input string</returns>
        public static string normalize(string s)
        {
            return s;
        }

        /// <summary>
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// </summary>
        /// <param name="s">The string to validate</param>
        /// <returns>True if the string is valid, False if it is not valid</returns>
        public static bool validate(string s)
        {
            return true;
        }

        [TestMethod]
        public void EmptyFormula()
        {
            string s = "";
            
            Formula testFormula = new Formula(s);
        }

        [TestMethod]
        public void NewFormula1()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s);
        }
    }
}
