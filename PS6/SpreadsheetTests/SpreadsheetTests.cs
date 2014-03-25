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
        public void ConstructorTest2()
        {
            Spreadsheet s = new Spreadsheet(x => true, x => x, "ConstructorTest2");
        }

        [TestMethod]
        public void ConstructorTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");

            s.Save("ConstructorTest3.spread");

            s = new Spreadsheet("ConstructorTest3.spread", x => true, x => x, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void ConstructorTest4()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");

            s.Save("ConstructorTest3.spread");

            // a bad validator should cause an exception
            s = new Spreadsheet("ConstructorTest3.spread", x => false, x => x, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void CheckNameTest1()
        {
            Spreadsheet s = new Spreadsheet(x => false, x => x, "CheckNameTest1");

            s.SetContentsOfCell("A1", "2.0");
        }

        [TestMethod]
        public void LookupTest1()
        {
            Spreadsheet s = new Spreadsheet(x => true, x => x, "LookupTest1");

            s.SetContentsOfCell("A1", "Some string value");
            s.SetContentsOfCell("B1", "=2 + A1");

            Assert.IsInstanceOfType(s.GetCellValue("B1"), typeof(FormulaError));
        }

        [TestMethod]
        public void LookupTest2()
        {
            Spreadsheet s = new Spreadsheet(x => true, x => x, "LookupTest2");

            s.SetContentsOfCell("A1", "=3.0");
            s.SetContentsOfCell("B1", "=3/0");
            s.SetContentsOfCell("C1", "=A1 + B1");

            Assert.IsInstanceOfType(s.GetCellValue("C1"), typeof(FormulaError));
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

            s.SetContentsOfCell("A1", "=A2 + B1");
            s.SetContentsOfCell("B1", "=A3 - A2");
            s.SetContentsOfCell("A2", "A2 + B1");
            s.SetContentsOfCell("B2", "A3 - A2");
            s.SetContentsOfCell("A3", "2.1");
            s.SetContentsOfCell("B3", "3.2");

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

            s.SetContentsOfCell(nullstring, "2.0");
        }

        [TestMethod]
        public void SetCellContentsEmptyTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");
            s.SetContentsOfCell("A1", "");
        }

        [TestMethod]
        public void SetCellContentsDoubleTest1()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> hs = new HashSet<string>();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetContentsOfCell("A1", "2.0"))
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

            s.SetContentsOfCell("", "2.0");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsDoubleTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("1X", "2.0");
        }

        [TestMethod]
        public void SetCellContentsStringTest1()
        {
            Spreadsheet s = new Spreadsheet();
            HashSet<string> hs = new HashSet<string>();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetContentsOfCell("A1", "TestString"))
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

            s.SetContentsOfCell("", "TestString");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsStringTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("1X", "TestString");
        }

        [TestMethod]
        public void SetCellContentsFormulaTest1A()
        {
            Spreadsheet s = new Spreadsheet();
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            list1.Add("A1");

            foreach (string item in s.SetContentsOfCell("A1", "=A2 + B1"))
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

            s.SetContentsOfCell("A1", "=A2 + B1");

            foreach (string item in s.SetContentsOfCell("B1", "=A3"))
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

            s.SetContentsOfCell("A1", "=A2 + B1");
            s.SetContentsOfCell("B1", "=A3 - A2");

            foreach (string item in s.SetContentsOfCell("A3", "=3+C1"))
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

            s.SetContentsOfCell("A1", "=A2 + B1");
            s.SetContentsOfCell("B1", "=A3 - A2");

            foreach (string item in s.SetContentsOfCell("A3", "=3+A1"))
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

            s.SetContentsOfCell("", "=A2 + B1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellContentsFormulaTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("1X", "=A2 + B1");
        }

        [TestMethod]
        public void GetCellContentsDoubleTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");

            Assert.AreEqual(2.0, s.GetCellContents("A1"));

            s.SetContentsOfCell("A1", "4.0");

            Assert.AreEqual(4.0, s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsDoubleTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsDoubleTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "2.0");

            s.GetCellContents("1X");
        }

        [TestMethod]
        public void GetCellContentsStringTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "TestString");

            Assert.AreEqual("TestString", s.GetCellContents("A1"));

            s.SetContentsOfCell("A1", "AnotherTestString");

            Assert.AreEqual("AnotherTestString", s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsStringTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "TestString");

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsStringTest3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "TestString");

            s.GetCellContents("1X");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCellContentsStringTest4()
        {
            Spreadsheet s = new Spreadsheet();

            string nullstring = null;

            s.SetContentsOfCell("A1", nullstring);
        }

        [TestMethod]
        public void GetCellContentsFormulaTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=A2 + B1");

            Assert.AreEqual(new Formula("A2 + B1"), s.GetCellContents("A1"));

            s.SetContentsOfCell("A1", "=X2 + Y3");

            Assert.AreEqual(new Formula("X2 + Y3"), s.GetCellContents("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsFormulaTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=A2 + B1");

            s.GetCellContents("");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContentsFormulaTest3()
        {
            Spreadsheet s = new Spreadsheet();
            
            s.SetContentsOfCell("A1", "=A2 + B1");

            s.GetCellContents("1X");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCellContentsFormulaTest4()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", null);
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

        [TestMethod]
        public void SaveTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.Save("savetest1.spread");
        }

        [TestMethod]
        public void SaveTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=A2 + B1");
            s.SetContentsOfCell("B1", "=A3 - A2");
            s.SetContentsOfCell("A2", "=A3 + B3");
            s.SetContentsOfCell("B2", "=A3 - A2");
            s.SetContentsOfCell("A3", "2.1");
            s.SetContentsOfCell("B3", "3.2");
            s.SetContentsOfCell("C1", "A string value");

            s.Save("savetest2.spread");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest3()
        {
            Spreadsheet s = new Spreadsheet();

            try
            {
                s.Save("readonly.spread");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("Error opening spreadsheet file \"readonly.spread\".", e.Message);
                throw e;
            }
        }

        [TestMethod]
        public void OpenTest1()
        {
            Spreadsheet s = new Spreadsheet();

            s.GetSavedVersion("opentest1.spread");
        }

        [TestMethod]
        public void OpenTest2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=A2 + B1");
            s.SetContentsOfCell("B1", "=A3 - A2");
            s.SetContentsOfCell("A2", "=A3 + B3");
            s.SetContentsOfCell("B2", "=A3 - A2");
            s.SetContentsOfCell("A3", "2.1");
            s.SetContentsOfCell("B3", "3.2");
            s.SetContentsOfCell("C1", "A string value");

            s.Save("opentest2.spread");

            s = new Spreadsheet();

            s.GetSavedVersion("opentest2.spread");

            Assert.AreEqual(s.GetCellContents("A1").ToString(), "A2+B1");
            Assert.AreEqual(s.GetCellContents("B1").ToString(), "A3-A2");
            Assert.AreEqual(s.GetCellContents("A2").ToString(), "A3+B3");
            Assert.AreEqual(s.GetCellContents("B2").ToString(), "A3-A2");
            Assert.AreEqual(s.GetCellContents("A3").ToString(), "2.1");
            Assert.AreEqual(s.GetCellContents("B3").ToString(), "3.2");
            Assert.AreEqual(s.GetCellContents("C1").ToString(), "A string value");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void OpenTest3()
        {
            Spreadsheet s = new Spreadsheet();

            try
            {
                s.GetSavedVersion("doesnotexist.spread");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("Error opening spreadsheet file \"doesnotexist.spread\".", e.Message);
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void OpenTest4()
        {
            Spreadsheet s = new Spreadsheet();

            try
            {
                s.GetSavedVersion("blank.spread");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("Error reading from spreadsheet file \"blank.spread\".", e.Message);
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void OpenTest5()
        {
            Spreadsheet s = new Spreadsheet();

            try
            {
                s.GetSavedVersion("badxml.spread");
            }
            catch (SpreadsheetReadWriteException e)
            {
                Assert.AreEqual("Error reading from spreadsheet file \"badxml.spread\".", e.Message);
                throw e;
            }
        }
    }
}
