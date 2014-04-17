using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace SpreadsheetGUITest
{
    /// <summary>
    /// Summary description for CodedUITest2
    /// </summary>
    [CodedUITest]
    public class CodedUITest1
    {
        public CodedUITest1()
        {
        }

        [TestMethod]
        public void OpenAndClose1()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
            this.UIMap.OpenAndClose();

        }

        [TestMethod]
        public void OpenAndDivideClose1()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
            this.UIMap.DivideByZero();
            this.UIMap.ChangeLanguages();
        }

        [TestMethod]
        public void openNothing()
        {
            this.UIMap.OpenNothing();
            this.UIMap.Maximize();
        }

        [TestMethod]
        public void Saving()
        {
            this.UIMap.Saving();
        }

        [TestMethod]
        public void circularException()
        {
            this.UIMap.Circular();
        }

        [TestMethod]
        public void ArrowsAndDelete1()
        {

            this.UIMap.OpenAndNew();
            this.UIMap.ArrowKeys();
            this.UIMap.Delete();
            this.UIMap.CircularDependency();
        }

        [TestMethod]
        public void Resize1()
        {
            this.UIMap.HelpText();
            this.UIMap.Resize();
            this.UIMap.OpenNewWindowSave();
        }

        [TestMethod]
        public void OpenInNewWindow1()
        {

            this.UIMap.OpenInNewWindow();


        }

        [TestMethod]
        public void Save1()
        {

            this.UIMap.Save();

        }

        [TestMethod]
        public void InvalidToken()
        {

            this.UIMap.InvalidTokens();

        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
