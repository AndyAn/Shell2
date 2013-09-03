using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace xelvor
{
    class MainApp
    {
        [STAThread]
        static void Main()
        {
            Application app = new Application();
            MainWindow host = CreateWindow();

            host.Show();
            app.Run(host);
        }

        private static MainWindow CreateWindow()
        {
            MainWindow win = new MainWindow();

            win.Closed += new EventHandler(new Action<object, EventArgs>((sender, e) =>
            {
                Environment.Exit(Environment.ExitCode);
            }));

            return win;
        }

    }
}
