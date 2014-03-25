using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;   //should be unnecessary since we already have System.Text, but Visual Studio wasn't finding Regex without it

namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluates algebraic expressions
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Gets the value of a named variable
        /// </summary>
        /// <param name="v">Name of variable to be looked up</param>
        /// <returns>Value of the named variable</returns>
        public delegate int Lookup(String v);

        /// <summary>
        /// Evaluate an algebraic expression
        /// </summary>
        /// <param name="exp">The expression to be evaluated</param>
        /// <param name="variableEvaluator">Lookup type delegate function to parse variables in the expression</param>
        /// <returns>Value of the expression</returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //exp = Regex.Replace(exp, @" +", "");                                      //removes spaces from expression
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");  //split string into substrings
            Stack<string> operators = new Stack<string>();                              //hold unused operators
            Stack<int> values = new Stack<int>();                                       //hold values
            Regex variableFormat = new Regex(@"^[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ]+[0123456789]+$"); //filter for variable names
            int outInt;     //holds an integer or variable value
            
            //loop over all substrings of the expression
            for (int i = 0; i < substrings.Length; i++)
            {
                outInt = 0;
                substrings[i] = substrings[i].Trim();
                if (substrings[i].Length != 0)                              //we only want to deal with non-whitespace characters
                {
                    bool isInt = int.TryParse(substrings[i], out outInt);   //try to parse the string as a integer
                    bool isVar = false;
                    
                    if (!isInt)
                    {
                        isVar = variableFormat.IsMatch(substrings[i]);     //try to parse the string as a variable if it's not an integer
                        if (isVar)
                        {
                            outInt = variableEvaluator(substrings[i]);          //if it's a variable, set outInt to its value
                        }
                    }

                    if (isInt || isVar)                                     //the logic for ints and vars is the same, so treat them the same
                    {
                        if(operators.Count > 0){
                            if (operators.Peek().CompareTo("*") == 0)       //if previous operator is *, then multiply
                            {
                                if (values.Count > 0)                       //are there going to be two operands?
                                {
                                    values.Push(values.Pop() * outInt);     //grab the top value, multiply it by outInt, and store the result
                                    operators.Pop();                        //pop the used *
                                }
                                else
                                {
                                    throw new ArgumentException();          //expression is invalid. Throw exception
                                }
                            }
                            else if (operators.Peek().CompareTo("/") == 0) //if previous operator is /, then divide
                            {
                                if (values.Count > 0)                       //are there going to be two operands?
                                {
                                    values.Push(values.Pop() / outInt);     //grab the top value, divide it by outInt, and store the result
                                    operators.Pop();                        //pop the used /
                                }
                                else
                                {
                                    throw new ArgumentException();          //expression is invalid. Throw exception
                                }
                            }
                            else
                            {
                                values.Push(outInt);    //couldn't multiply or divide, so store the result for now
                            }
                        }
                        else
                        {
                            values.Push(outInt);    //couldn't multiply or divide, so store the result for now
                        }
                    }
                    else
                    {
                        //if operator is + or -, then execute a previous + or - if present, and push the result and this operator onto their respective stacks
                        if ((substrings[i].CompareTo("+") == 0) || (substrings[i].CompareTo("-") == 0))
                        {
                            if (operators.Count > 0)                            //make sure there are operators in the stack before peeking
                            {
                                if ((values.Count > 1) && (operators.Peek().CompareTo("+") == 0))  //if addition occurs before this, process the addition
                                {
                                    values.Push(values.Pop() + values.Pop());   //add the sum to the value stack
                                    operators.Pop();                            //pop off the used +
                                    operators.Push(substrings[i]);              //push on this operator
                                }
                                else if ((values.Count > 1) && (operators.Peek().CompareTo("-") == 0))  //if subtraction occurs before this, process the subtraction
                                {
                                    values.Push(-1 * values.Pop() + values.Pop()); //add the difference to the value stack
                                    operators.Pop();                            //pop off the used -
                                    operators.Push(substrings[i]);              //push on this operator
                                }
                                else
                                {
                                    operators.Push(substrings[i]);              //nothing to operate on yet, just add operator to the stack
                                }
                            }
                            else
                            {
                                operators.Push(substrings[i]);                  //nothing to operate on yet, just add operator to the stack
                            }

                        }
                        else if (substrings[i].CompareTo("*") == 0)             //if operator is *, push it onto the stack
                        {
                            operators.Push(substrings[i]);
                        }
                        else if (substrings[i].CompareTo("/") == 0)             //if operator is /, push it onto the stack
                        {
                            operators.Push(substrings[i]);
                        }
                        else if (substrings[i].CompareTo("(") == 0)             //if operator is (, push it onto the stack
                        {
                            operators.Push(substrings[i]);
                        }
                        else if (substrings[i].CompareTo(")") == 0)             //if operator is ), push it onto the stack
                        {
                            //loop through all addition and subtraction that takes place in the parentheses
                            while ((operators.Count > 0) && ((operators.Peek().CompareTo("+") == 0) || (operators.Peek().CompareTo("-") == 0)))
                            {
                                if (values.Count < 2)                           //are there two operands?
                                {
                                    throw new ArgumentException();              //operator without operands; expression is invalid. Throw exception
                                }
                                else if (operators.Peek().CompareTo("+") == 0)  //if addition occurs before this, process the addition
                                {
                                    values.Push(values.Pop() + values.Pop());   //add the sum to the value stack
                                    operators.Pop();                            //pop off the used +
                                }
                                else if (operators.Peek().CompareTo("-") == 0)  //if subtraction occurs before this, process the subtraction
                                {
                                    values.Push(-1 * values.Pop() + values.Pop()); //add the difference to the value stack
                                    operators.Pop();                            //pop off the used -
                                }
                            }
                            if (operators.Count == 0)
                            {
                                throw new ArgumentException();                  //no left parenthesis found, expression is invalid. Throw exception
                            }
                            else if (operators.Peek().CompareTo("(") == 0)
                            {
                                operators.Pop();                                //pop off the used left parenthesis
                            }

                            //if there are still * or /, process them
                            if ((operators.Count > 0) && ((operators.Peek().CompareTo("*") == 0) || (operators.Peek().CompareTo("/") == 0)))
                            {
                                if (values.Count < 2)                           //are there two operands?
                                {
                                    throw new ArgumentException();              //operator without operands; expression is invalid. Throw exception
                                }
                                else if (operators.Peek().CompareTo("*") == 0)  //if multiplication occurs before this, process the multiplication
                                {
                                    values.Push(values.Pop() * values.Pop());   //add the product to the value stack
                                    operators.Pop();                            //pop off the used *
                                }
                                else if (operators.Peek().CompareTo("/") == 0)  //if division occurs before this, process the division
                                {
                                    int tempDenominator = values.Pop();
                                    values.Push(values.Pop() / tempDenominator); //add the quotient to the value stack
                                    operators.Pop();                            //pop off the used /
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentException();                      //if we're here, it must be an invalid variable name
                        }
                    }
                }
            }

            //after all tokens have been processed, only a + or - should remain on the operator stack, if it has anything on it
            if ((operators.Count > 0) && ((operators.Peek().CompareTo("+") == 0) || (operators.Peek().CompareTo("-") == 0)))
            {
                if (values.Count < 2)                           //are there two operands?
                {
                    throw new ArgumentException();              //operator without operands; expression is invalid. Throw exception
                }
                else if (operators.Peek().CompareTo("+") == 0)  //if addition occurs before this, process the addition
                {
                    values.Push(values.Pop() + values.Pop());   //add the sum to the value stack
                    operators.Pop();                            //pop off the used +
                }
                else if (operators.Peek().CompareTo("-") == 0)  //if subtraction occurs before this, process the subtraction
                {
                    values.Push(-1 * values.Pop() + values.Pop()); //add the difference to the value stack
                    operators.Pop();                            //pop off the used -
                }
            }

            //there shouldn't be any operators left, and only one value on the value stack. Otherwise, the expression was malformed
            if ((values.Count == 1) && (operators.Count == 0))
            {
                return values.Pop();                            //return the final value of the expression
            }
            else
            {
                throw new ArgumentException();                  //expression is invalid. Throw exception
            }
        }
    }
}
