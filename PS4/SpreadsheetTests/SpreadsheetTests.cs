using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            Spreadsheet s = new Spreadsheet();
        }

        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest1()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> hs = new HashSet<string>();

            foreach (string cell in s.GetNamesOfAllNonemptyCells())
            {
                hs.Add(cell);
            }

            Assert.AreEqual(0, hs.Count);
        }

        [TestMethod]
        public void GetNamesOfAllNonemptyCellsTest2()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");
            list1.Add("A2");
            list1.Add("A3");
            list1.Add("B1");
            list1.Add("B2");
            list1.Add("B3");

            s.SetCellContents("A1", new Formula("A2 + B1"));
            s.SetCellContents("B1", new Formula("A3 - A2"));
            s.SetCellContents("A2", "A2 + B1");
            s.SetCellContents("B2", "A3 - A2");
            s.SetCellContents("A3", 2.1);
            s.SetCellContents("B3", 3.2);

            foreach (string cell in s.GetNamesOfAllNonemptyCells())
            {
                Assert.IsTrue(list1.Contains(cell));
                list2.Add(cell);
            }

            foreach (string cell in list1)
            {
                Assert.IsTrue(list2.Contains(cell));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsBadNameTest1()
        {
            Spreadsheet s = new Spreadsheet();
            
            string nullstring = null;

            s.SetCellContents(nullstring, 2.0);
        }

        [TestMethod]
        public void SetCellContentsDoubleTest1()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> hs = new HashSet<string>();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetCellContents("A1", 2.0))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
            }

            foreach (string item in list1)
            {
                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsDoubleTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("", 2.0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsDoubleTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("1X", 2.0);
        }

        [TestMethod]
        public void SetCellContentsStringTest1()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> hs = new HashSet<string>();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetCellContents("A1", "TestString"))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
            }

            foreach (string item in list1)
            {
                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsStringTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("", "TestString");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsStringTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("1X", "TestString");
        }

        [TestMethod]
        public void SetCellContentsFormulaTest1A()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetCellContents("A1", f))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
            }

            foreach (string item in list1)
            {
                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        public void SetCellContentsFormulaTest1B()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");
            list1.Add("B1");

            s.SetCellContents("A1", new Formula("A2 + B1"));

            foreach (string item in s.SetCellContents("B1", new Formula("A3")))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
                Console.WriteLine(item);
            }

            foreach (string item in list1)
            {

                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        public void SetCellContentsFormulaTest1C()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");
            list1.Add("B1");
            list1.Add("A3");

            s.SetCellContents("A1", new Formula("A2 + B1"));
            s.SetCellContents("B1", new Formula("A3 - A2"));

            foreach (string item in s.SetCellContents("A3", new Formula("3+C")))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
                Console.WriteLine(item);
            }

            foreach (string item in list1)
            {

                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void SetCellContentsFormulaTest1D()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");
            list1.Add("B1");
            list1.Add("A3");

            s.SetCellContents("A1", new Formula("A2 + B1"));
            s.SetCellContents("B1", new Formula("A3 - A2"));

            foreach (string item in s.SetCellContents("A3", new Formula("3+A1")))
            {
                Assert.IsTrue(list1.Contains(item));
                list2.Add(item);
                Console.WriteLine(item);
            }

            foreach (string item in list1)
            {

                Assert.IsTrue(list2.Contains(item));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsFormulaTest2()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");

            s.SetCellContents("", f);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsFormulaTest3()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");
            
            s.SetCellContents("1X", f);
        }

        [TestMethod]
        public void GetCellContentsDoubleTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", 2.0);

            Assert.AreEqual(2.0, s.GetCellContents("A1"));

            s.SetCellContents("A1", 4.0);

            Assert.AreEqual(4.0, s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsDoubleTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", 2.0);

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsDoubleTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", 2.0);

            s.GetCellContents("1X");
        }

        [TestMethod]
        public void GetCellContentsStringTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", "TestString");

            Assert.AreEqual("TestString", s.GetCellContents("A1"));

            s.SetCellContents("A1", "AnotherTestString");

            Assert.AreEqual("AnotherTestString", s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsStringTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", "TestString");

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsStringTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", "TestString");

            s.GetCellContents("1X");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCellContentsStringTest4()
        {
            Spreadsheet s = new Spreadsheet();

            string nullstring = null;

            s.SetCellContents("A1", nullstring);
        }

        [TestMethod]
        public void GetCellContentsFormulaTest1()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");

            s.SetCellContents("A1", f);

            Assert.AreEqual(f, s.GetCellContents("A1"));

            s.SetCellContents("A1", new Formula("X2 + Y3"));

            Assert.AreEqual(new Formula("X2 + Y3"), s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsFormulaTest2()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");

            s.SetCellContents("A1", f);

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsFormulaTest3()
        {
            Spreadsheet s = new Spreadsheet();
            Formula f = new Formula("A2 + B1");

            s.SetCellContents("A1", f);

            s.GetCellContents("1X");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCellContentsFormulaTest4()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetCellContents("A1", (Formula) null);
        }

        [TestMethod]
        public void GetCellContentsEmptyTest1()
        {
            Spreadsheet s = new Spreadsheet();

            Assert.AreEqual("", s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsEmptyTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsEmptyTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.GetCellContents("1X");
        }
    }
}
