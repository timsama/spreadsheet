using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SS
{
    class ViewApplicationContext : ApplicationContext
    {
        // Number of open forms
        int openForms = 0;

        // Singleton ViewApplicationContext
        private static ViewApplicationContext viewContext;

        /// <summary>
        /// Private constructor for ViewApplicationContext
        /// </summary>
        ViewApplicationContext()
        {
        }

        public static ViewApplicationContext getAppContext()
        {
            if (viewContext == null)
            {
                viewContext = new ViewApplicationContext();
            }

            return viewContext;
        }

        /// <summary>
        /// Runs the form; based on Jim's demo code
        /// </summary>
        public void RunForm(Form form)
        {
            // One more form is running
            openForms++;

            // When this form closes, we want to find out
            form.FormClosed += (o, e) => { if (--openForms <= 0) ExitThread(); };

            // Run the form
            form.Show();
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
