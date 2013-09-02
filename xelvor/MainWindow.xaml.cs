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
using AiP.Metro;

namespace xelvor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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

            //SoftwareInfo = GetSoftwareInfo();

            //idtor.SoftwareInfo = string.Join("\r\n", SoftwareInfo.ToArray());
            //idtor.ConsolePrompt = IDENTIFIER_STRING;
            idtor.PreviewMouseDown += idtor_PreviewMouseDown;
            idtor.PreviewKeyDown += idtor_PreviewKeyDown;
            //idtor.EnterPressing += idtor_EnterPressing;

            //CurrentPath.Text = string.Format(CURRENT_PATH, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower());
            //CurrentPath.FontFamily = ResourceManager.Font;
        }

        #region Window Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WaitForInput();
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

        private void FindFocusedControl(DependencyObject dpObj)
        {
            int count = VisualTreeHelper.GetChildrenCount(dpObj);

            for (int i = 0; i < count; i++)
            {
                var subDp = VisualTreeHelper.GetChild(dpObj, i);
                if (VisualTreeHelper.GetChildrenCount(subDp) > 0)
                {
                    if (subDp is UIElement)
                    {
                        if ((subDp as UIElement).IsFocused)
                        {
                            MessageBox.Show((subDp as FrameworkElement).Name);
                        }
                    }
                    FindFocusedControl(subDp);
                }
            }
        }

        //private void idtor_EnterPressing(CodeEditor sender, EnterPressingEventArgs e)
        //{
        //    string cmd = e.GetCommand();
        //    string[] args = e.GetArgs();

        //    if (idtor.GetInternalCommands().IndexOf(cmd) == -1)
        //    {
        //        e.Handle = true;

        //        //SendCommandToDOSConsole(e.CommandLineText);

        //        return;
        //    }

        //    switch (cmd)
        //    {
        //        case "exit":
        //            this.Close();
        //            break;
        //    }
        //}

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

        //private void WindowStyleChange(WindowState windowState)
        //{
        //    if (windowState == WindowState.Maximized)
        //    {
        //        Margin = new Thickness(0);
        //    }
        //    else
        //    {
        //        Margin = new Thickness(10);
        //    }
        //}

        //private List<string> GetSoftwareInfo()
        //{
        //    List<string> info = new List<string>();

        //    using (Process process = new Process())
        //    {
        //        process.StartInfo.FileName = "cmd.exe";
        //        process.StartInfo.Arguments = "-?";
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.RedirectStandardError = true;

        //        process.Start();

        //        info.AddRange(process.StandardOutput.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
        //    }
        //    info[info.Count - 1] = ""; // remove initial path information

        //    return info;
        //}

        //private void SendCommandToDOSConsole(string cmd)
        //{
        //    using (Process process = new Process())
        //    {
        //        process.StartInfo.FileName = "cmd.exe";
        //        process.StartInfo.Arguments = string.Empty;
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.RedirectStandardError = true;
        //        process.StartInfo.RedirectStandardInput = true;
        //        process.StartInfo.WorkingDirectory = CurrentPath.Text.Remove(0, 6);

        //        process.Start();

        //        process.StandardInput.WriteLine(cmd);
        //        process.StandardInput.WriteLine("exit");

        //        AppendText("\r\n\r\n");

        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();

        //        process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
        //        process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);

        //        //Keyboard.AddKeyDownHandler(idtor, new KeyEventHandler(delegate (object o, KeyEventArgs e) {});
        //        while (!process.WaitForExit(0))
        //        {
        //            DoEvent();
        //            //if (Keyboard.Modifiers | ModifierKeys.Control == ModifierKeys.Control && Keyboard.)
        //        }
        //    }
        //}

        //void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(e.Data))
        //        this.AppendText(e.Data + "\r\n");
        //}

        //void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(e.Data))
        //        this.AppendText(e.Data + "\r\n");
        //}

        private void AppendText(string text)
        {
            this.Dispatcher.BeginInvoke(new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    //string currentPath = CurrentPath.Text.Remove(0, 6);
                    string output = text.ToLower().Trim();

                    if (SoftwareInfo.IndexOf(text.Trim()) > -1)
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (output.Remove(0, 1).StartsWith(":\\"))
                        {
                            //if (!output.StartsWith(currentPath))
                            //{
                            //    //if (output != currentPath.ToLower())
                            //    CurrentPath.Text = string.Format(CURRENT_PATH, output.Remove(output.IndexOf(">")));
                            //}
                            return;
                        }
                        if (output.EndsWith("exit"))
                        {
                            return;
                        }
                    }
                    //idtor.AppendText(text);
                    //idtor.ScrollToEnd();
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
    }
}