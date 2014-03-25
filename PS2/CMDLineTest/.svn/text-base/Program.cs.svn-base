using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;

namespace CMDLineTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependents("a", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> aPends = new HashSet<string>(t.GetDependents("a"));
            Console.WriteLine(t.GetDependents("a"));
            //Assert.IsTrue(aPends.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }
    }
}
