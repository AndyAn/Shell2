using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using xelvor.Utils;
using xelvor.Controls.CodeType;
using System.Diagnostics;
using System.Windows.Threading;
using xelvor.Core;

namespace xelvor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string CURRENT_PATH = " - \\> {0}";
        string IDENTIFIER_STRING = string.Format("[{0}@{1}] > ", Environment.UserName, Environment.UserDomainName);
        List<string> cmdList = new List<string>();
        List<string> SoftwareInfo;

        public MainWindow()
        {
            InitializeComponent();

            Icon = ResourceManager.Icon;
            Margin = new Thickness(10);
            Width = 800;
            Height = 600;

            SoftwareInfo = GetSoftwareInfo();

            //idtor.SoftwareInfo = string.Join("\r\n", SoftwareInfo.ToArray());
            idtor.ConsolePrompt = IDENTIFIER_STRING;
            idtor.PreviewMouseDown += idtor_PreviewMouseDown;
            idtor.PreviewKeyDown += idtor_PreviewKeyDown;
            idtor.EnterPressing += idtor_EnterPressing;

            CurrentPath.Text = string.Format(CURRENT_PATH, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower());
            CurrentPath.FontFamily = ResourceManager.Font;
        }

        #region Window Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WaitForInput();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Max.Content = "1";
                Max.ToolTip = "Maximize Window";
            }
            else
            {
                Max.Content = "2";
                Max.ToolTip = "Restore Down";
            }

            WindowStyleChange(WindowState);
        }

        #endregion

        #region Editor Events

        private void idtor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            int col = idtor.CaretIndex;
        }

        private void idtor_PreviewKeyDown(object sender, KeyEventArgs e)
        {


        }

        private void idtor_EnterPressing(CodeEditor sender, EnterPressingEventArgs e)
        {
            string cmd = e.GetCommand();
            string[] args = e.GetArgs();

            if (idtor.GetInternalCommands().IndexOf(cmd) == -1)
            {
                e.Handle = true;

                //SendCommandToDOSConsole(e.CommandLineText);

                return;
            }

            switch (cmd)
            {
                case "exit":
                    this.Close();
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void WaitForInput()
        {
            idtor.Focus();
        }

        //private MiirorSettings LoadConfig()
        //{
        //    string cfgFile = IO.Path.Combine(Environment.CurrentDirectory, "Miiror.cfg");

        //    if (!IO.File.Exists(cfgFile))
        //    {
        //        ObjectSerializer<MiirorSettings>.Save(new MiirorSettings(), cfgFile);
        //    }

        //    return ObjectSerializer<MiirorSettings>.Load(cfgFile);
        //}

        private bool SaveConfig()
        {
            bool isSaved = true;

            //try
            //{
            //    string cfgFile = IO.Path.Combine(Environment.CurrentDirectory, "Miiror.cfg");

            //    ObjectSerializer<MiirorSettings>.Save(MiSettings, cfgFile);
            //}
            //catch
            //{
            //    isSaved = false;
            //}

            return isSaved;
        }

        private bool SaveConfig(bool isRefresh)
        {
            //if (isRefresh)
            //{
            //    MiSettings.MonitorList.Clear();
            //    foreach (MiirorItemDO item in mirorGroup.GetSource())
            //    {
            //        MiSettings.MonitorList.Add(new MiirorItem()
            //        {
            //            Filtered = item.Filtered,
            //            Identity = item.Identity,
            //            IsFolder = item.IsFolder,
            //            IsRecursive = item.IsRecursive,
            //            IsWorking = item.IsWorking,
            //            Source = item.Source,
            //            Target = item.Target
            //        });
            //    }
            //}

            return SaveConfig();
        }

        private void WindowStyleChange(WindowState windowState)
        {
            if (windowState == WindowState.Maximized)
            {
                Margin = new Thickness(0);
            }
            else
            {
                Margin = new Thickness(10);
            }
        }

        private List<string> GetSoftwareInfo()
        {
            List<string> info = new List<string>();

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "-?";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();

                info.AddRange(process.StandardOutput.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }
            info[info.Count - 1] = ""; // remove initial path information

            return info;
        }

        private void SendCommandToDOSConsole(string cmd)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = string.Empty;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.WorkingDirectory = CurrentPath.Text.Remove(0, 6);

                process.Start();

                process.StandardInput.WriteLine(cmd);
                process.StandardInput.WriteLine("exit");

                AppendText("\r\n\r\n");

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);

                //Keyboard.AddKeyDownHandler(idtor, new KeyEventHandler(delegate (object o, KeyEventArgs e) {});
                while (!process.WaitForExit(0))
                {
                    DoEvent();
                    //if (Keyboard.Modifiers | ModifierKeys.Control == ModifierKeys.Control && Keyboard.)
                }
            }
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                this.AppendText(e.Data + "\r\n");
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                this.AppendText(e.Data + "\r\n");
        }

        private void AppendText(string text)
        {
            this.Dispatcher.BeginInvoke(new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    string currentPath = CurrentPath.Text.Remove(0, 6);
                    string output = text.ToLower().Trim();

                    if (SoftwareInfo.IndexOf(text.Trim()) > -1)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (output.Remove(0, 1).StartsWith(":\\"))
                        {
                            if (!output.StartsWith(currentPath))
                            {
                                //if (output != currentPath.ToLower())
                                CurrentPath.Text = string.Format(CURRENT_PATH, output.Remove(output.IndexOf(">")));
                            }
                            return;
                        }
                        if (output.EndsWith("exit"))
                        {
                            return;
                        }
                    }
                    idtor.AppendText(text);
                    idtor.ScrollToEnd();
                }), new object[] { this, System.EventArgs.Empty });
        }

        public void DoEvent()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(
                    delegate(object f)
                    {
                        (f as DispatcherFrame).Continue = false;
                        return null;
                    }),
                frame);
            Dispatcher.PushFrame(frame);
        }

        #endregion

        #region Windows API

        private readonly int agWidth = 4; //12;
        private readonly int borderThickness = 4;
        private Point mousePoint = new Point();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32API.WM_NCHITTEST:

                    mousePoint.X = Forms.Control.MousePosition.X;
                    mousePoint.Y = Forms.Control.MousePosition.Y;

                    #region test mouse location

                    // left top corner
                    if (mousePoint.Y - Location.Y - Margin.Top <= agWidth
                       && mousePoint.X - Location.X - Margin.Left <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOPLEFT);
                    }
                    // left bottom corner    
                    else if (ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= agWidth
                       && mousePoint.X - Location.X - Margin.Left <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOMLEFT);
                    }
                    // right top corner
                    else if (mousePoint.Y - Location.Y - Margin.Top <= agWidth
                       && ActualWidth + Location.X - mousePoint.X - Margin.Right <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOPRIGHT);
                    }
                    // right bottom corner
                    else if (ActualWidth + Location.X - mousePoint.X - Margin.Right <= agWidth
                       && ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOMRIGHT);
                    }
                    // left side of window
                    else if (mousePoint.X - Location.X - Margin.Left <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTLEFT);
                    }
                    // right side of window
                    else if (ActualWidth + Location.X - mousePoint.X - Margin.Right <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTRIGHT);
                    }
                    // top of window
                    else if (mousePoint.Y - Location.Y - Margin.Top <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTTOP);
                    }
                    // bottom of window
                    else if (ActualHeight + Location.Y - mousePoint.Y - Margin.Bottom <= borderThickness)
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTBOTTOM);
                    }
                    // moving window
                    else if ((mousePoint.Y - Location.Y - Margin.Top <= 24 && mousePoint.X - Location.X - Margin.Left - Margin.Right <= ActualWidth - 160) ||
                        (mousePoint.Y - Location.Y - Margin.Top > 24 && mousePoint.Y - Location.Y - Margin.Top <= 36))
                    {
                        handled = true;
                        return new IntPtr((int)Win32API.HitTest.HTCAPTION);
                    }

                    break;

                    #endregion

                case Win32API.WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        internal Point Location
        {
            get
            {
                return new Point(WindowState == WindowState.Maximized ? 0 : Left, WindowState == WindowState.Maximized ? 0 : Top);
            }
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            Win32API.MINMAXINFO mmi = (Win32API.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Win32API.MINMAXINFO));

            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = Win32API.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                Win32API.MONITORINFO monitorInfo = new Win32API.MONITORINFO();
                Win32API.GetMonitorInfo(monitor, monitorInfo);
                Win32API.RECT rcWorkArea = monitorInfo.rcWork;
                Win32API.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);// -3;
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);// -3;
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);// +6;
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);// +6;
                mmi.ptMinTrackSize.x = (int)this.MinWidth;
                mmi.ptMinTrackSize.y = (int)this.MinHeight;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion

        #region Control Panel

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        #endregion
    }
}
