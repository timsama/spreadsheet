using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        /// Lookup function for testing purposes
        /// </summary>
        /// <param name="s">Variable name</param>
        /// <returns>Made up value for variable</returns>
        public static double dummyLookup(String s)
        {
            int temp;
            int.TryParse(s.Substring(s.Length - 1, 1), out temp);
            return s.Substring(0, 1).CompareTo("j") * temp / 10.0;
        }

        /// <summary>
        /// Lookup function that always throws an ArgumentException for testing purposes
        /// </summary>
        /// <param name="s">Variable name</param>
        /// <returns>Made up value for variable</returns>
        public static double hatesEverythingLookup(String s)
        {
            throw new ArgumentException();
        }

        /// <summary>
        /// Lookup function that always throws a PlatformNotSupportedException for testing purposes
        /// </summary>
        /// <param name="s">Variable name</param>
        /// <returns>Made up value for variable</returns>
        public static double throwsTheWrongExceptionLookup(String s)
        {
            throw new PlatformNotSupportedException();
        }

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
        /// A lower-case version of normalize() for testing purposes
        /// </summary>
        /// <param name="s">The string to normalize</param>
        /// <returns>A normalized copy of the input string</returns>
        public static string normalizeLowercase(string s)
        {
            return normalize(s).ToLower();
        }

        /// <summary>
        /// An upper-case version of normalize() for testing purposes
        /// </summary>
        /// <param name="s">The string to normalize</param>
        /// <returns>A normalized copy of the input string</returns>
        public static string normalizeUppercase(string s)
        {
            return normalize(s).ToUpper();
        }

        /// <summary>
        /// A normalize function that always throws an ArgumentException for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string hatesEverythingNormalize(string s)
        {
            throw new ArgumentException();
        }

        /// <summary>
        /// A normalize function that always throws an ExecutionEngineException for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string throwsTheWrongExceptionNormalize(string s)
        {
            throw new ExecutionEngineException();
        }

        /// <summary>
        /// A version of normalize() that ruins variables for testing purposes
        /// </summary>
        /// <param name="s">The string to normalize</param>
        /// <returns>A normalized copy of the input string</returns>
        public static string badNormalize(string s)
        {
            return "1" + normalize(s).ToUpper();
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

        /// <summary>
        /// A validator function that always throws an ArgumentException for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool hatesEverythingValidate(string s)
        {
            throw new ArgumentException();
        }

        /// <summary>
        /// A validator function that always throws a DuplicateWaitObjectException for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool throwsTheWrongExceptionValidate(string s)
        {
            throw new DuplicateWaitObjectException();
        }

        /// <summary>
        /// A validator function that always returns false for testing purposes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool alwaysFalseValidate(string s)
        {
            return false;
        }

        [TestMethod]
        public void EmptyFormula()
        {
            // tests if constructor accepts an empty formula (it should not)
            string s = "";

            try
            {
                Formula testFormula = new Formula(s, normalize, validate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("Constructor accepted an empty forumla--violates spec.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it handled correctly
                Assert.AreEqual(typeof(FormulaFormatException), e.GetType(), "An exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void BrokenNormalizerPrelim()
        {
            // this verifies that the string used will work with a correct normalizer
            // so the string is proven valid if BrokenNormalizer fails
            string s = "x3 - 8 * kkq1";

            new Formula(s, normalize, validate).Evaluate(dummyLookup);
        }

        [TestMethod]
        public void BrokenNormalizer()
        {
            // this tests if the constructor properly handles a normalizer that
            // makes a valid formula invalid
            string s = "x3 - 8 * kkq";

            try
            {
                new Formula(s, badNormalize, validate).Evaluate(dummyLookup);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("Constructor accepted invalid variable names generated by a bad normalizer.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it handled correctly
                Assert.AreEqual(typeof(FormulaFormatException), e.GetType(), "An exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NormalizeException1()
        {
            // testing exception handling for a normalizer that always throws ArgumentException
            string s = "x1+x2";

            try
            {
                Formula testFormula = new Formula(s, hatesEverythingNormalize, validate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("An exception was thrown by the normalizer, but was buried.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it handled correctly
                Assert.AreEqual(typeof(FormulaFormatException), e.GetType(), "An exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NormalizeException2()
        {
            // testing exception handling for a normalizer that always throws ExecutionEngineException
            string s = "x1+x2";

            try
            {
                Formula testFormula = new Formula(s, throwsTheWrongExceptionNormalize, validate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("An exception was thrown by the normalizer, but it was buried.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it's probably turning all
                // exceptions into FormulaFormatExceptions
                Assert.AreNotEqual(typeof(FormulaFormatException), e.GetType(), "An ExecutionEngineException was thrown by the normalizer (abnormal behavior that should not be handled), but was converted into a FormulaFormatException.");
            }
        }

        [TestMethod]
        public void ValidatorException1()
        {
            // testing exception handling for a validator that always throws ArgumentException
            string s = "x1+x2";

            try
            {
                Formula testFormula = new Formula(s, normalize, hatesEverythingValidate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("An exception was thrown by the validator, but it was buried.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it handled correctly
                Assert.AreEqual(typeof(FormulaFormatException), e.GetType(), "An exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void ValidatorException2()
        {
            // testing exception handling for a validator that always throws DuplicateWaitObjectException
            string s = "x1+x2";

            try
            {
                Formula testFormula = new Formula(s, normalize, throwsTheWrongExceptionValidate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("An exception was thrown by the validator, but it was buried.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it's probably turning all
                // exceptions into FormulaFormatExceptions
                Assert.AreNotEqual(typeof(FormulaFormatException), e.GetType(), "A DuplicateWaitObjectException was thrown by the validator (abnormal behavior that should not be handled), but was converted into a FormulaFormatException.");
            }
        }

        [TestMethod]
        public void ValidatorRejection()
        {
            // testing exception handling for a validator that always returns false
            string s = "x1+x2";

            try
            {
                Formula testFormula = new Formula(s, normalize, alwaysFalseValidate);

                // if the above code failed to throw an exception, this test fails
                Assert.Fail("Constructor failed to throw an exception when the validator rejected the formula.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it handled correctly
                Assert.AreEqual(typeof(FormulaFormatException), e.GetType(), "An exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula1()
        {
            // testing creation of a valid formula
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);   
        }

        [TestMethod]
        public void NewFormula2()
        {
            // testing creation of an invalid formula (due to illegal variable name)
            try
            {
                string s = "1x+x2";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("\"1x\" is not a legal variable name: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula3()
        {
            // testing creation of an invalid formula (due to malformed parenthesis)
            try
            {
                string s = "(x+x2))";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An extra right parenthesis was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula4()
        {
            // testing creation of an invalid formula (due to malformed parenthesis)
            try
            {
                string s = "(x+)x2)";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An extra right parenthesis was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula5()
        {
            // testing creation of an invalid formula (due to malformed parenthesis)
            try
            {
                string s = "(x(+x2)";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An extra left parenthesis was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula6()
        {
            // testing creation of an invalid formula (due to malformed parenthesis)
            try
            {
                string s = "((x+x2)";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An extra left parenthesis was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula7()
        {
            // testing creation of an invalid formula (due to doubled operator)
            try
            {
                string s = "(x++x2)";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An extra operator was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula8()
        {
            // testing creation of an invalid formula (due to illegal symbol)
            try
            {
                string s = "(x+@+x2)";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("An illegal symbol was present: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula9()
        {
            // testing creation of an invalid formula (due to ending in an operator)
            try
            {
                string s = "x+x2+";

                Formula testFormula = new Formula(s, normalize, validate);
                Assert.Fail("The formula ended in an operator: an exception should have been thrown.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.AreEqual(e.GetType(), typeof(FormulaFormatException), "An Exception was thrown, but it was not of type FormulaFormatException.");
            }
        }

        [TestMethod]
        public void NewFormula10()
        {
            // testing creation of a single double as a Formula
            string s = "78";

            Formula testFormula = new Formula(s, normalize, validate);

        }

        [TestMethod]
        public void Evaluate1()
        {
            // testing addition after addition
            Assert.AreEqual(6.0, (double)new Formula("1+2+3", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate2()
        {
            // testing subtraction after addition
            Assert.AreEqual(0.0, (double)new Formula("1+2-3", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate3()
        {
            // testing addition after subtraction
            Assert.AreEqual(2.0, (double)new Formula("1-2+3", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate4()
        {
            // testing subtraction after subtraction
            Assert.AreEqual(-4.0, (double)new Formula("1-2-3", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate5()
        {
            // testing multiplication after subtraction in parentheses
            Assert.AreEqual(-5.0, (double)new Formula("(1-2*3)", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate6()
        {
            // testing multiplication before parentheses
            Assert.AreEqual(-10.0, (double)new Formula("2*(1-2*3)", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate7()
        {
            // testing division before parentheses
            Assert.AreEqual(-2.0, (double)new Formula("10/(1-2*3)", normalize, validate).Evaluate(dummyLookup), 0.00001);
        }

        [TestMethod]
        public void Evaluate8()
        {
            // testing handling of a failed variable lookup
            Assert.AreEqual(typeof(FormulaError), new Formula("x10/(1-2*3)", normalize, validate).Evaluate(hatesEverythingLookup).GetType());
        }

        [TestMethod]
        public void Evaluate9()
        {
            // testing handling of a failed variable lookup that throws the wrong exception
            try
            {
                // if the below code failed to throw an exception, or generated a FormulaError
                // based on a PlatformNotSupportedException, this test fails
                Assert.AreNotEqual(typeof(FormulaError), new Formula("x10/(1-2*3)", normalize, validate).Evaluate(throwsTheWrongExceptionLookup).GetType(), "Evaluate generated a FormulaError based on a PlatformNotSupportedException, function should have rethrown it instead.");
            }
            catch (Exception e)
            {
                // if the code threw a FormulaFormatException, it's probably turning all
                // exceptions into FormulaFormatExceptions
                Assert.AreNotEqual(typeof(FormulaFormatException), e.GetType(), "A PlatformNotSupportedException was thrown by the lookup (abnormal behavior that should not be handled), but was converted into a FormulaFormatException.");
            }
        }

        [TestMethod]
        public void Evaluate10()
        {
            // testing repeated addition in parentheses
            Assert.AreEqual(40.0, new Formula("((1+2+2)+2+6+(3+4)+3+2+(1+2)+2+3+(3+4))", normalize, validate).Evaluate(dummyLookup));
        }

        [TestMethod]
        public void Evaluate11()
        {
            // testing repeated subtraction in parentheses
            Assert.AreEqual(-18.0, new Formula("((1-2-2)-2-6-(3-4)-3-2-(1-2)-2-3-(3-4))", normalize, validate).Evaluate(dummyLookup));
        }

        [TestMethod]
        public void Evaluate12()
        {
            // testing repeated addition and subtraction in parentheses
            Assert.AreEqual(-2.0, new Formula("((1-2-2)+2+6+(3-4)-3-2-(1+2)-2-3+(3+4))", normalize, validate).Evaluate(dummyLookup));
        }

        [TestMethod]
        public void Evaluate13()
        {
            // testing repeated multiplication in parentheses
            Assert.AreEqual(497664.0, (double)new Formula("((1*2*2)*2*6*(3*4)*3*2*(1*2)*2*3*(3*4))", normalize, validate).Evaluate(dummyLookup), 0.0000000001);
        }

        [TestMethod]
        public void Evaluate14()
        {
            // testing repeated division in parentheses
            Assert.AreEqual(0.00205761316872428, (double) new Formula("((1/2/2)/2/6/(3/4)/3/2/(1/2)/2/3/(3/4))", normalize, validate).Evaluate(dummyLookup), 0.0000000001);
        }

        [TestMethod]
        public void Evaluate15()
        {
            // testing repeated multiplication and division in parentheses
            Assert.AreEqual(0.375, (double) new Formula("((1/2/2)*2*6*(3/4)/3/2/(1*2)/2/3*(3*4))", normalize, validate).Evaluate(dummyLookup), 0.0000000001);
        }

        [TestMethod]
        public void DivideByZero1()
        {
            // testing for a FormulaError when trying to divide by zero
            Assert.AreEqual(typeof(FormulaError), new Formula("1/0", normalize, validate).Evaluate(dummyLookup).GetType(), "Division by zero went undetected.");
        }

        [TestMethod]
        public void DivideByZero2()
        {
            // testing for a FormulaError when trying to divide by zero-valued parenthetical
            Assert.AreEqual(typeof(FormulaError), new Formula("10/(3-3)", normalize, validate).Evaluate(dummyLookup).GetType(), "A division by zero occurred, but a FormulaError was not returned by Evaluator.");
        }

        [TestMethod]
        public void GetVariables1()
        {
            // testing for correct variables being returned
            List<string> variableList = new List<string>();
            variableList.Add("x6");
            variableList.Add("y2");
            variableList.Add("qrs4");
            variableList.Add("The foreach loop should never reach this one, and if it does it will error");

            int i = 0;
            foreach (string variable in new Formula("x6*y2+qrs4+x6").GetVariables())
            {
                Assert.AreEqual(variableList[i++], variable);
            }
        }

        [TestMethod]
        public void Equals1()
        {
            // testing equivalence between two identical Formula objects
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Formula testFormula2 = new Formula(s, normalize, validate);
            Assert.IsTrue(testFormula.Equals(testFormula2), ".Equals() incorrectly returned false for two identical Formula objects.");
        }

        [TestMethod]
        public void Equals2()
        {
            // testing equivalence between two identical unnamed Formula objects
            string s = "x1+x2";

            Assert.IsTrue(new Formula(s, normalize, validate).Equals(new Formula(s, normalize, validate)), ".Equals() incorrectly returned false for two identical unnamed Formula objects.");
        }

        [TestMethod]
        public void Equals3()
        {
            // testing equivalence between a formula and an extended version of itself
            string s1 = "x1+x2";
            string s2 = "x1+x2+x3";

            Assert.IsFalse(new Formula(s1, normalize, validate).Equals(new Formula(s2, normalize, validate)), "A formula and an extended length version of that formula with more variables were incorrectly determined equal by .Equals()");
        }

        [TestMethod]
        public void Equals4()
        {
            // testing equivalence between a formula and a truncated version of itself
            string s1 = "x1+x2+x3";
            string s2 = "x1+x2";

            Assert.IsFalse(new Formula(s1, normalize, validate).Equals(new Formula(s2, normalize, validate)), "A formula and a truncated version of that formula with fewer variables were incorrectly determined equal by .Equals()");
        }

        [TestMethod]
        public void Equals5()
        {
            // testing equivalence between a non-null Formula and null
            string s = "x1+x2+x3";

            Assert.IsFalse(new Formula(s, normalize, validate).Equals(null), "A non-null Formula was incorrectly determined to be equal to null by .Equals()");
        }

        [TestMethod]
        public void Equals6()
        {
            // testing equivalence between a Formula and a non-Formula object
            string s = "x1+x2+x3";

            Assert.IsFalse(new Formula(s, normalize, validate).Equals(new List<string>()), "A Formula object and a non-Formula object were incorrectly determined to be equal by .Equals()");
        }

        [TestMethod]
        public void Equals7()
        {
            // testing equivalence between a variable and a double
            string s1 = "x1";
            string s2 = "1.0";

            Assert.IsFalse(new Formula(s1, normalize, validate).Equals(new Formula(s2, normalize, validate)), "A formula containing only a variable, and a formula containing only a double were incorrectly determined to be equal by .Equals()");
        }

        [TestMethod]
        public void Equals8()
        {
            // testing equivalence between a double and a variable
            string s1 = "1.0";
            string s2 = "x1";

            Assert.IsFalse(new Formula(s1, normalize, validate).Equals(new Formula(s2, normalize, validate)), "A formula containing only a variable, and a formula containing only a double were incorrectly determined to be equal by .Equals()");
        }

        [TestMethod]
        public void EqualsOperator1()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsTrue(testFormula == new Formula(s, normalize, validate), "The == operator incorrectly returned that a formula was not equal to an equivalent version of itself.");
        }

        [TestMethod]
        public void EqualsOperator2()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsFalse(testFormula == null, "The == operator incorrectly returned that a non-null Formula was equal to null.");
        }

        [TestMethod]
        public void EqualsOperator3()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsFalse(null == testFormula, "The == operator incorrectly returned that null was equal to a non-null Formula.");
        }

        [TestMethod]
        public void EqualsOperator4()
        {
            Formula testFormula = null;
            Assert.IsTrue(testFormula == null, "The == operator incorrectly returned that a null Formula was not equal to null.");
        }

        [TestMethod]
        public void EqualsOperator5()
        {
            Formula testFormula = null;
            Formula testFormula2 = null;
            Assert.IsTrue(testFormula == testFormula2, "The == operator incorrectly returned that two null Formula objects were not equal.");
        }

        [TestMethod]
        public void EqualsOperator6()
        {
            string s = "x1+x2";

            Assert.IsFalse(new Formula(s, normalizeUppercase, validate) == new Formula(s, normalizeLowercase, validate), "The == operator incorrectly returned that differently-normalized Formula objects were equal.");
        }

        [TestMethod]
        public void NotEqualsOperator1()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsFalse(testFormula != new Formula(s, normalize, validate), "The != operator incorrectly returned that a Formula was not equal to an equivalent version of itself.");
        }

        [TestMethod]
        public void NotEqualsOperator2()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsTrue(testFormula != null, "The != operator incorrectly returned that a non-null Formula was equal to null.");
        }

        [TestMethod]
        public void NotEqualsOperator3()
        {
            string s = "x1+x2";

            Formula testFormula = new Formula(s, normalize, validate);
            Assert.IsTrue(null != testFormula, "The != operator incorrectly returned that null was equal to a non-null Formula.");
        }

        [TestMethod]
        public void NotEqualsOperator4()
        {
            Formula testFormula = null;
            Assert.IsFalse(testFormula != null, "The != operator incorrectly returned that a null Formula object was not equal to null.");
        }

        [TestMethod]
        public void NotEqualsOperator5()
        {
            Formula testFormula = null;
            Formula testFormula2 = null;
            Assert.IsFalse(testFormula != testFormula2, "The != operator incorrectly returned that two null Formula objects were not equal.");
        }

        [TestMethod]
        public void NotEqualsOperator6()
        {
            string s = "x1+x2";

            Assert.IsTrue(new Formula(s, normalizeUppercase, validate) != new Formula(s, normalizeLowercase, validate), "The != operator incorrectly returned that two differently-normalized Formula objects were equal.");
        }

        [TestMethod]
        public void Hashcode1()
        {
            // 3 random formulae should not have the same hashcode.
            // 2 is possible, but 3 in a row is incredibly unlikely.
            // Failing this test is not a "failure" per se (technically,
            // "int hashcode(string s){return 1;}" is a valid-but-terrible hash function) 
            // but if it fails consistently, the hash function is probably
            // really, really bad. Seriously. Don't fail this test.

            string s1 = "a343";
            string s2 = "xx4";
            string s3 = "q1";

            int hash1 = new Formula(s1).GetHashCode();
            int hash2 = new Formula(s2).GetHashCode();
            int hash3 = new Formula(s3).GetHashCode();

            Assert.AreNotEqual(hash1, hash2, "Read the comment for Hashcode1 for a long explanation of this problem.");
            Assert.AreNotEqual(hash2, hash3);
            Assert.AreNotEqual(hash1, hash3);
        }

        [TestMethod]
        public void Hashcode2()
        {
            // testing to see if introducing the normalizer messes up hashing
            // if this fails but Hashcode1 succeeds, the problem is probably the
            // normalizer making all of the strings the same, or something similar

            string s1 = "a343";
            string s2 = "xx4";
            string s3 = "q1";

            int hash1 = new Formula(s1, normalize, validate).GetHashCode();
            int hash2 = new Formula(s2, normalize, validate).GetHashCode();
            int hash3 = new Formula(s3, normalize, validate).GetHashCode();

            Assert.AreNotEqual(hash1, hash2, "If Hashcode1 passed successfully, the normalizer is probably doing something bad.");
            Assert.AreNotEqual(hash2, hash3);
            Assert.AreNotEqual(hash1, hash3);
        }

        [TestMethod]
        public void Hashcode3()
        {
            // testing a really long variable name hashcode

            string s = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            new Formula(s, normalize, validate).GetHashCode();
        }
    }
}
