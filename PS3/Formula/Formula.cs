// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        List<string> tokenList;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string,string> normalize, Func<string,bool> isValid)
        {
            //initialize the list of tokens
            tokenList = new List<string>();
            string normalizedToken = "";
            string errormessage;

            foreach (string token in GetTokens(formula))
            {
                try
                {
                    // normalize the token
                    normalizedToken = normalize(token);
                }
                catch (Exception e)
                {
                    if (typeof(ArgumentException) == e.GetType())
                    {
                        // handle the ArgumentException from the normalizer
                        throw new FormulaFormatException("Token \"" + token + "\" is not a legal variable name: formula is invalid.");
                    }
                    else
                    {
                        // an unknown exception occurred; rethrow it
                        throw e;
                    }
                }
                
                // check to see if the normalized token is legal, throw FormulaFormatException if not
                if (!isLegalVariableName(normalizedToken))
                {
                    throw new FormulaFormatException("Token \"" + token + "\" is not a legal variable name: formula is invalid.");
                }

                try
                {

                    // check the normalized token for validity, throw FormulaFormatException if not
                    if (isValid(normalizedToken))
                    {
                        // no exceptions were thrown on this token, so it's safe to add to the list
                        tokenList.Add(normalizedToken);
                    }
                    else
                    {
                        throw new FormulaFormatException("Token \"" + token + "\" was rejected by the validator: formula is invalid.");
                    }
                }
                catch (Exception e)
                {
                    if (typeof(ArgumentException) == e.GetType())
                    {
                        // handle the ArgumentException from the validator
                        throw new FormulaFormatException("Token \"" + token + "\" caused an exception in the validator: formula is invalid.");
                    }
                    else
                    {
                        // an unknown exception occurred; rethrow it
                        throw e;
                    }
                }
            }

            if (!isAFormula(out errormessage))
            {
                throw new FormulaFormatException(errormessage);
            }
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            // create stacks for both operators and values
            Stack<string> operators = new Stack<string>();
            Stack<double> values = new Stack<double>();

            // a variable's regex pattern
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";

            // declare a variable to temporarily hold a double or variable value from the lookup delegate
            double outDouble;
            
            // loop over all tokens in the formula
            foreach (string token in tokenList)
            {
                // reset any leftover value to 0
                outDouble = 0;
                
                // try to parse the token as a double
                bool isDouble = double.TryParse(token, out outDouble);
                bool isVar = false;
                    
                if (!isDouble)
                {
                    // try to parse the token as a variable if it's not a double
                    isVar = Regex.IsMatch(token, varPattern);
                    
                    // if the token is a variable, set outDouble to its value using the lookup delegate
                    if (isVar)
                    {
                        try
                        {
                            outDouble = lookup(token);
                        }
                        catch (Exception e)
                        {
                            if (typeof(ArgumentException) == e.GetType())
                            {
                                // handle the ArgumentException from the lookup
                                return new FormulaError("Lookup failed: variable \"" + token + "\" does not contain a value.");
                            }
                            else
                            {
                                // an unknown exception occurred; rethrow it
                                throw e;
                            }
                        }
                    }
                }

                // the logic for doubles and vars is the same, so treat them the same
                if (isDouble || isVar)
                {
                    if (operators.Count > 0)
                    {
                        //if previous operator is * or /, then process it
                        if (operators.Peek().CompareTo("*") == 0)
                        {
                            // add the product to the values stack and pop off the used *
                            values.Push(values.Pop() * outDouble);
                            operators.Pop();
                        }
                        else if (operators.Peek().CompareTo("/") == 0)
                        {
                            // check for divide by zero error
                            if (outDouble == 0)
                            {
                                return new FormulaError("Formula Error: division by zero. The universe has now imploded--thanks a lot!");
                            }

                            // add the quotient to the values stack and pop off the used /
                            values.Push(values.Pop() / outDouble);
                            operators.Pop();
                        }
                        else
                        {
                            // couldn't multiply or divide, so store the result for now
                            values.Push(outDouble);
                        }
                    }
                    else
                    {
                        // couldn't multiply or divide, so store the result for now
                        values.Push(outDouble);
                    }
                }
                else
                {
                    // if operator is + or -, then execute a previous + or - if present, and push the result and this operator onto their respective stacks
                    if ((token.CompareTo("+") == 0) || (token.CompareTo("-") == 0))
                    {
                        // make sure there are operators in the stack before peeking
                        if (operators.Count > 0)
                        {
                            // if addition or subtraction occurs before this, process it
                            if ((values.Count > 1) && (operators.Peek().CompareTo("+") == 0))
                            {
                                // add the sum to the value stack and pop off the used +
                                values.Push(values.Pop() + values.Pop());
                                operators.Pop();

                                // push this operator onto the stack
                                operators.Push(token);
                            }
                            else if ((values.Count > 1) && (operators.Peek().CompareTo("-") == 0))
                            {
                                // add the difference to the value stack and pop off the used -
                                values.Push(-1 * values.Pop() + values.Pop());
                                operators.Pop();

                                // push this operator onto the stack
                                operators.Push(token);
                            }
                            else
                            {
                                // there's nothing to operate on yet, just add operator to the stack
                                operators.Push(token);
                            }
                        }
                        else
                        {
                            // there's nothing to operate on yet, just add operator to the stack
                            operators.Push(token);
                        }

                    }
                    else if (token.CompareTo("*") == 0)
                    {
                        // if operator is *, push it onto the stack
                        operators.Push(token);
                    }
                    else if (token.CompareTo("/") == 0)
                    {
                        // if operator is /, push it onto the stack
                        operators.Push(token);
                    }
                    else if (token.CompareTo("(") == 0)
                    {
                        // if operator is (, push it onto the stack
                        operators.Push(token);
                    }
                    else if (token.CompareTo(")") == 0)
                    {
                        // if operator is ), loop through all addition and subtraction that takes place in the parentheses
                        while ((operators.Count > 0) && ((operators.Peek().CompareTo("+") == 0) || (operators.Peek().CompareTo("-") == 0)))
                        {
                            // if addition or subtraction occurs before this, process it
                            if (operators.Peek().CompareTo("+") == 0)
                            {
                                // add the sum to the value stack and pop off the used +
                                values.Push(values.Pop() + values.Pop());
                                operators.Pop();
                            }
                            else if (operators.Peek().CompareTo("-") == 0)
                            {
                                // add the difference to the value stack and pop off the used -
                                values.Push(-1 * values.Pop() + values.Pop());
                                operators.Pop();
                            }
                        }

                        // pop off the used (, this should always execute
                        if (operators.Peek().CompareTo("(") == 0)
                        {
                            operators.Pop();
                        }

                        // if there are still * or /, process them
                        if ((operators.Count > 0) && ((operators.Peek().CompareTo("*") == 0) || (operators.Peek().CompareTo("/") == 0)))
                        {
                            // if multiplication or division occurs before this, process it
                            if (operators.Peek().CompareTo("*") == 0)
                            {
                                // add the product to the value stack and pop off the used *
                                values.Push(values.Pop() * values.Pop());
                                operators.Pop();
                            }
                            else if (operators.Peek().CompareTo("/") == 0)
                            {
                                // we have to pop the numerator and denominator off in reverse order
                                double denominator = values.Pop();

                                // check for divide by zero error
                                if (denominator == 0)
                                {
                                    return new FormulaError("Formula Error: division by zero. The universe has now imploded--thanks a lot!");
                                }

                                // add the quotient to the value stack and pop off the used /
                                values.Push(values.Pop() / denominator);
                                operators.Pop();
                            }
                        }
                    }
                }
            }

            // after all tokens have been processed, only a + or -, if anything, should remain on the operator stack
            if ((operators.Count > 0) && ((operators.Peek().CompareTo("+") == 0) || (operators.Peek().CompareTo("-") == 0)))
            {
                // if addition or subtraction occurs before this, process it
                if (operators.Peek().CompareTo("+") == 0)
                {
                    // add the sum to the value stack and pop off the used +
                    values.Push(values.Pop() + values.Pop());
                    operators.Pop();
                }
                else if (operators.Peek().CompareTo("-") == 0)
                {
                    // add the difference to the value stack and pop off the used -
                    values.Push(-1 * values.Pop() + values.Pop());
                    operators.Pop();
                }
            }

            // there should be only one value on the value stack. Pop and return its value
            return values.Pop();
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            // List to hold used variables
            List<string> variableList = new List<string>();

            // Patterns for string tokens
            string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";

            // Enumerate tokens that match the variable regex pattern
            foreach (string token in tokenList)
            {
                // check if token is a variable, and if we have already returned it once
                if (Regex.IsMatch(token, varPattern, RegexOptions.Singleline) && ! variableList.Contains(token))
                {
                    variableList.Add(token);
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            string stringValue = "";

            // everything in tokenList is already normalized, and whitespace has been removed, so just concatenate them
            foreach (string token in tokenList)
            {
                stringValue += token;
            }

            return stringValue;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens, which are compared as doubles, and variable tokens,
        /// whose normalized forms are compared as strings.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            string objFormula;

            // return false if the passed-in object is null
            if (obj == null)
            {
                return false;
            }

            // return false if the passed-in object is not a Formula
            if (obj.GetType() != typeof(Formula))
            {
                return false;
            }

            objFormula = obj.ToString();

            // loop over each token in the passed-in Formula, comparing them with this Formula's tokens in sequence
            int i = 0;
            foreach (string token in GetTokens(objFormula))
            {
                // if the token lists are different sizes, or any two corresponding tokens are unequal
                // then the formulae are unequal, so return false
                if ((i >= tokenList.Count) || (! compareTokens(token, tokenList[i++])))
                {
                    return false;
                }
            }

            // if tokenList wasn't checked to the end then the passed-in formula is a subset of this formula
            // but they are not equal because the passed-in object has fewer tokens than this object
            //
            // For example:
            // new Formula("x1+y2+z3").Equals(new Formula("x1+y2")) would pass the above loop, but it would get caught here
            // because the first has 3 tokens and the second only has 2
            if (i != tokenList.Count)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares two tokens as strings, unless they can be parsed as doubles, in which case it compares them as doubles.
        /// </summary>
        /// <param name="token1">The first token to be compared</param>
        /// <param name="token2">The second token to be compared</param>
        /// <returns>Whether or not the two tokens are equal</returns>
        private bool compareTokens(string token1, string token2)
        {
            double token1Value, token2Value;
            bool token1IsDouble = double.TryParse(token1, out token1Value);
            bool token2IsDouble = double.TryParse(token2, out token2Value);

            // if one is a double and the other is not, the tokens are not equal
            if (token1IsDouble ^ token2IsDouble)
            {
                return false;
            }

            // because of the above XOR, we know that token1IsDouble == token2IsDouble, so we only need to test one of them
            // check if the tokens are doubles
            if (token1IsDouble)
            {
                return token1Value == token2Value;
            }
            else // tokens are strings, so compare them as case-sensitive strings
            {
                return (0 == String.Compare(token1, token2, false));
            }
        }

        /// <summary>
        /// Determines whether a variable name is allowed or not
        /// </summary>
        /// <param name="s">The variable name</param>
        /// <returns>Returns true if the variable name is legal</returns>
        public static bool isLegalVariableName(string s)
        {
            bool isAType;
            
            // Patterns for individual tokens
            String opPattern = @"[\+\-*/]";
            String varPattern = @"^[a-zA-Z]+[1-9]\d*$";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String lpPattern = @"\(";
            String rpPattern = @"\)";

            // Pattern to catch a bad variable name
            String badVarPattern = @"\d+([a-zA-Z_])+";

            // dummy variable for trying to parse as a double
            double dummy;

            // check token against variable type pattern
            isAType = Regex.IsMatch(s, varPattern);
            if (isAType)
            {
                // token matched the variable type pattern, return false if it also matches
                // the bad variable pattern, true if it does not
                return ! Regex.IsMatch(s, badVarPattern);
            }

            // check token against other type patterns, become true if it matches one of them
            isAType = isAType || double.TryParse(s, out dummy);
            isAType = isAType || Regex.IsMatch(s, doublePattern);
            isAType = isAType || Regex.IsMatch(s, opPattern);
            isAType = isAType || Regex.IsMatch(s, lpPattern);
            isAType = isAType || Regex.IsMatch(s, rpPattern);
            
            return isAType;
        }

        /// <summary>
        /// Determines whether or not a formula follow operator rules
        /// </summary>
        /// <returns>Returns true if formula is valid. Throws an exception if it is invalid.</returns>
        private bool isAFormula(out string errormessage)
        {
            bool expectingVariableOrDouble = true;
            string lastString = "";
            
            // dummy variable for verifying double parseability of a token, since the
            // regex doesn't seem to be 100% reliable
            double dummy;

            // Patterns for individual tokens
            String opPattern = @"^[\+\-*/]$";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String lpPattern = @"\(";
            String rpPattern = @"\)";

            if (tokenList.Count == 0)
            {
                errormessage = "No tokens detected: formula is invalid";
                return false;
            }

            // check the formula for invalid variable names, unexpected operators, or symbols that slipped by
            foreach (string token in tokenList)
            {
                // check for invalid variable names and unexpected operators
                if (expectingVariableOrDouble)
                {
                    // if we see an operator, throw "unexpected operator (token)" exception
                    // for the purposes of this part, parentheses don't count
                    if (Regex.IsMatch(token, opPattern))
                    {
                        errormessage = "Unexpected operator \"" + token + "\" encountered: formula is invalid";
                        return false;
                    }
                    else if (Regex.IsMatch(token, rpPattern))
                    {
                        errormessage = "Unexpected closing parenthesis encountered: formula is invalid";
                        return false;
                    }
                    else if (double.TryParse(token, out dummy) || Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, doublePattern))
                    {
                        expectingVariableOrDouble = false;
                        lastString = token;
                    }
                }
                else
                {
                    // if we see a variable or double, throw "lastString+token isn't a valid variable name" exception
                    // for the purposes of this part, parentheses don't count
                    if (double.TryParse(token, out dummy) || Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, doublePattern))
                    {
                        errormessage = "Token \"" + lastString + token + "\" is not a legal variable name: formula is invalid.";
                        return false;
                    }
                    else if (Regex.IsMatch(token, lpPattern))
                    {
                        errormessage = "Unexpected opening parenthesis encountered: formula is invalid";
                        return false;
                    }
                    else if (Regex.IsMatch(token, opPattern))
                    {
                        expectingVariableOrDouble = true;
                        lastString = token;
                    }
                }
            }

            // throw an exception if the formula doesn't end in a variable, double, or closing parenthesis
            if (expectingVariableOrDouble && (string.Compare(lastString, ")") != 0))
            {
                errormessage = "String terminated unexpectedly: formula is invalid";
                return false;
            }

            // check parenthesis matching before returning true
            return parenthesisMatch(out errormessage);
        }

        /// <summary>
        /// Verifies the usage of parentheses in the formula
        /// </summary>
        /// <returns>Throws an exception if parenthesis usage is invalid.</returns>
        private bool parenthesisMatch(out string errormessage)
        {
            int parenlevel = 0;

            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";

            foreach (string token in tokenList)
            {
                // increment the parenthesis level every time an opening parenthesis is found
                if (Regex.IsMatch(token, lpPattern))
                {
                    parenlevel++;
                }

                // decrement the parenthesis level every time a closing parenthesis is found
                if (Regex.IsMatch(token, rpPattern))
                {
                    parenlevel--;
                }

                // if there are ever more right parentheses found so far than left, formula is invalid
                if (parenlevel < 0)
                {
                    errormessage = "Unexpected closing parenthesis found: formula is invalid.";
                    return false;
                }
            }

            // if all of the parentheses match up, the level at the end will be 0
            if (parenlevel != 0)
            {
                errormessage = "Unmatched opening parenthesis found: formula is invalid.";
                return false;
            }

            errormessage = "";
            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            bool f1null = object.ReferenceEquals(f1, null);
            bool f2null = object.ReferenceEquals(f2, null);

            // if one of the two is null and the other is not, they are not equal
            if (f1null ^ f2null)
            {
                return false;
            }
            
            // because of the above XOR, we know that if f1 is null, so is f2
            // check if both f1 and f2 are null, and if they are, then they are equal
            if (f1null)
            {
                return true;
            }

            // neither f1 nor f2 is null, so check their equality using the Equals method
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            bool f1null = object.ReferenceEquals(f1, null);
            bool f2null = object.ReferenceEquals(f2, null);

            // if one of the two is null and the other is not, they are not equal
            if (f1null ^ f2null)
            {
                return true;
            }

            // because of the above XOR, we know that if f1 is null, so is f2
            // check if both f1 and f2 are null, and if they are, then they are equal
            if (f1null)
            {
                return false;
            }

            // neither f1 nor f2 is null, so check their equality using the Equals method
            return ! f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            int hashcode = 0;
            int placevalue = 0;

            // cast each character of each token to an int (via char), and multiply it by an incrementing power of -2
            // but ensure that the power of two doesn't get so large that it causes an overflow
            foreach (string token in tokenList)
            {
                for (int i = 0; i < token.Length; i++)
                {
                    hashcode += (int)token[placevalue % token.Length] * (-2) ^ placevalue++;
                    if (placevalue >= 23)
                    {
                        placevalue = 0;
                    }
                }
            }
            return hashcode;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}

