using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subnautica_Options
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //If there is an argument, pass that argument to the Form, otherwise launch the Form without arguments
            if (args != null && args.Length > 0)
                Application.Run(new Form1(args[0]));
            else
                Application.Run(new Form1());
        }
    }
}
