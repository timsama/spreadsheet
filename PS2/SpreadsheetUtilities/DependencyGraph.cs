// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// (Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.)
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        Dictionary<String, DependencyNode> nodeSet { get; set; }
        int sizeOfNodeSet;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            // size starts out at 0
            sizeOfNodeSet = 0;

            // create an empty dictionary to hold all nodes
            nodeSet = new Dictionary<String, DependencyNode>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            // return the number of dependency pairs in the graph
            get { return sizeOfNodeSet; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                DependencyNode temp;

                // try to get s's node
                if (nodeSet.TryGetValue(s, out temp))
                {
                    // return the number of dependees
                    return temp.Dependees.Count;
                }
                else
                {
                    // if the node does not exist, it has no dependees
                    return 0;
                }
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            DependencyNode temp;

            // try to get s's node
            if (nodeSet.TryGetValue(s, out temp))
            {
                // return true if there is at least one dependent
                return (temp.Dependents.Count > 0);
            }
            else
            {
                // if node does not exist, it has no dependents
                return false;
            }
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            DependencyNode temp;

            // try to get s's node
            if (nodeSet.TryGetValue(s, out temp))
            {
                // return true if there is at least one dependee
                return (temp.Dependees.Count > 0);
            }
            else
            {
                // if node does not exist, it has no dependees
                return false;
            }
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            DependencyNode temp;

            // try to get s's node
            if (nodeSet.TryGetValue(s, out temp))
            {
                // if successful, return a list of s's dependents
                return temp.Dependents;
            }
            else
            {
                // if the node does not exist, return an empty list of strings
                return new List<String>();
            }
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            DependencyNode temp;

            // try to get s's node
            if (nodeSet.TryGetValue(s, out temp))
            {
                // if successful, return a list of s's dependees
                return temp.Dependees;
            }
            else
            {
                // if the node does not exist, return an empty list of strings
                return new List<String>();
            }
        }


        /// <summary>
        /// Adds the ordered pair (s,t), if it doesn't exist
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void AddDependency(string s, string t)
        {
            DependencyNode temp;

            // Add nodes for each if they don't already exist
            if (!nodeSet.ContainsKey(s))
            {
                nodeSet.Add(s, new DependencyNode(s));
            }
            if (!nodeSet.ContainsKey(t))
            {
                nodeSet.Add(t, new DependencyNode(t));
            }

            // try to get s's node, and make sure it doesn't already contain t
            if (nodeSet.TryGetValue(s, out temp) && ! temp.Dependents.Contains(t))
            {
                // add t to s's list of dependents
                temp.Dependents.Add(t);

                //only need to increment size once for each set of adds
                sizeOfNodeSet++;
            }
            // try to get t's node, and make sure it doesn't already contain s
            if (nodeSet.TryGetValue(t, out temp) && ! temp.Dependees.Contains(s))
            {
                //add s to t's list of dependees
                temp.Dependees.Add(s);
            }               
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            DependencyNode temp;

            // try to get s's node, make sure it contains t
            if (nodeSet.TryGetValue(s, out temp) && temp.Dependents.Contains(t))
            {
                // remove t from s's dependents list
                temp.Dependents.Remove(t);
                
                //only need to decrement size once for each set of removes
                sizeOfNodeSet--;
            }
            // try to get t's node, make sure it contains s
            if (nodeSet.TryGetValue(t, out temp) && temp.Dependees.Contains(s))
            {
                // remove s from t's dependees list
                temp.Dependees.Remove(s);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            DependencyNode temp;
            List<String> deleteDependents = new List<string>();

            // try to get the list of dependents
            if (nodeSet.TryGetValue(s, out temp))
            {
                // list each old dependency to be removed
                foreach (String dependent in temp.Dependents)
                {
                    deleteDependents.Add(dependent);
                }
                // remove each old dependency
                foreach (String dependent in deleteDependents)
                {
                    RemoveDependency(s, dependent);
                }
                // add each new dependency
                foreach (String dependent in newDependents)
                {
                    AddDependency(s, dependent);
                }
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            DependencyNode temp;
            List<String> deleteDependees = new List<string>();

            // try to get the list of dependents
            if (nodeSet.TryGetValue(s, out temp))
            {
                // list each old dependency to be removed
                foreach (String dependee in temp.Dependees)
                {
                    deleteDependees.Add(dependee);
                }
                // remove each old dependency
                foreach (String dependee in deleteDependees)
                {
                    RemoveDependency(dependee, s);
                }
                // add each new dependency
                foreach (String dependee in newDependees)
                {
                    AddDependency(dependee, s);
                }
            }
        }

    }

    /// <summary>
    /// An object to track the dependencies of a single node
    /// </summary>
    public class DependencyNode
    {
        /// <summary>
        /// The name of the dependency node
        /// </summary>
        public String Name
        { get; set; }

        /// <summary>
        /// A list of dependents
        /// </summary>
        public List<String> Dependents
        { get; set; }

        /// <summary>
        /// A list of dependees
        /// </summary>
        public List<String> Dependees
        { get; set; }

        /// <summary>
        /// Constructor for DependencyNode class
        /// </summary>
        /// <param name="_name">The name of the node</param>
        public DependencyNode(String _name)
        {
            // set the name of the node
            Name = _name;

            // the dependents and dependees sets are new (empty) lists
            Dependees = new List<string>();
            Dependents = new List<string>();
        }
    }
}
