using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FormulaEvaluator;

namespace EvalTester
{
    class Tester
    {
        static void Main(string[] args)
        {
            Evaluator.Lookup temp = dummy;
            String input = "";
            if (args.Length == 0)
            {
                input = Console.ReadLine();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    input += args[i];
                }
            }

            Console.WriteLine(input);
            Console.Write(Evaluator.Evaluate(input, temp));

            if (args.Length == 0)
            {
                Console.Read();
            }
        }

        public static int dummy(String s)
        {
            int temp;
            int.TryParse(s.Substring(s.Length - 1, 1), out temp);
            return s.Substring(0, 1).CompareTo("j") * temp;
        }
    }
}
