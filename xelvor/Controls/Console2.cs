using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using xelvor.Utils;
using xelvor.Controls.CodeType;
using xelvor.Core;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;
using System.Management;

namespace xelvor.Controls
{
    public class Console2 : CodeBox
    {
        #region Variables

        List<string> cmdList = new List<string>();

        int cmdListIndex = 0;

        bool runningCommand = false;

        private Regex regPrompt = new Regex("[a-z]:(\\\\.+)*\\>.*$", RegexOptions.IgnoreCase);
        private ProcessInterface proc = new ProcessInterface();
        private Brush defaultCaretBrush;
        private string lastInput;
        private List<string> internalCommands = new List<string>() {
                                                    "settings",
                                                    "exit",
                                                    "cls"};

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Prompt on Console
        /// </summary>
        public string ConsolePrompt { get; set; }

        /// <summary>
        /// Set SoftwareInfo
        /// </summary>
        public string SoftwareInfo
        {
            set
            {
                if (base.Text.Length == 0)
                {
                    base.AppendText(value + "\r\n");
                }
            }
        }

        #endregion

        #region Private Properties

        private int CurrentLineIndex
        {
            get
            {
                return base.GetLineIndexFromCharacterIndex(base.CaretIndex);
            }
        }

        private int StartColumn
        {
            get
            {
                return ConsolePrompt.Length;
            }
        }

        #endregion

        #region Constructor

        public Console2()
        {
            defaultCaretBrush = CaretBrush;

            //Initialize process interface events
            proc.OnProcessOutput += new ProcessEventHanlder(proc_OnProcessOutput);
            proc.OnProcessError += new ProcessEventHanlder(proc_OnProcessError);
            proc.OnProcessInput += new ProcessEventHanlder(proc_OnProcessInput);
            proc.OnProcessExit += new ProcessEventHanlder(proc_OnProcessExit);
            proc.StartProcess("cmd.exe", "");

            ConsolePrompt = string.Format("[{0}@{1} {2}]$ ", Environment.UserName, Environment.UserDomainName, Path.GetFileName(proc.Process.StartInfo.WorkingDirectory));
        }

        #endregion

        #region Private Events

        void proc_OnProcessError(object sender, ProcessEventArgs args)
        {
            //  Write the output, in red
            WriteOutput(args.Content);

            //  Fire the output event.
            //FireConsoleOutputEvent(args.Content);
        }

        void proc_OnProcessOutput(object sender, ProcessEventArgs args)
        {
            //  Write the output, in white
            WriteOutput(args.Content);

            //  Fire the output event.
            //FireConsoleOutputEvent(args.Content);
        }

        void proc_OnProcessInput(object sender, ProcessEventArgs args)
        {

        }

        void proc_OnProcessExit(object sender, ProcessEventArgs args)
        {
            GC.Collect();
        }

        #endregion

        #region Protected Overrides

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
            CaretBrush = Brushes.WhiteSmoke;
            FontFamily = ResourceManager.Font;
            FontSize = 14.0;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            TextWrapping = TextWrapping.WrapWithOverflow;
            ContextMenu = null;

            Loaded += new RoutedEventHandler(CodeTypeLoaded);
            MaxLength = int.MaxValue;
            AcceptsReturn = true;
            AcceptsTab = false;
            Focus();

            LineNumberMarginWidth = 0.0;

            // load syntax files
            //SyntaxManager.Initialize();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            CaretBrush = Brushes.Transparent;

            base.OnPreviewMouseDown(e);
            //lastCaretIndex = CaretIndex;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                CaretIndex = GetCharacterIndexFromPoint(e.GetPosition(this), true) + 1;
            }
            //e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            DocumentLine line = GetCurrentLine();
            if (SelectedText.Length == 0)
            {
                if (line.Index != LineCount - 1)
                {
                    CaretIndex = Text.Length;
                }
                else
                {
                    if (line.PosInLine < StartColumn)
                    {
                        CaretIndex = Text.Length;
                    }
                }
            }
            
            if (e.ChangedButton == MouseButton.Right)
            {
                if (line.Index != LineCount - 1 || CaretIndex == Text.Length)
                {
                    AppendText(Clipboard.GetText());
                }
                else
                {
                    Text = Text.Insert(CaretIndex, Clipboard.GetText());
                }
                CaretIndex = Text.Length;
            }

            if (SelectedText.Length > 0)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    int startIndex = Text.IndexOf(SelectedText);
                    int endIndex = startIndex + SelectedText.Length;

                    if (CaretIndex >= startIndex && CaretIndex <= endIndex)
                    {
                        Clipboard.SetText(SelectedText);
                        CaretIndex = Text.Length;
                    }
                }
            }

            CaretBrush = defaultCaretBrush;
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (runningCommand)
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                foreach (int pid in proc.GetChildProcessIds())
                {
                    proc.KillProcess(pid);
                }

                //foreach (IntPtr handle in proc.GetChildProcessHandles())
                //{
                //    proc.KillProcess(handle);
                //}

                //WriteOutput("Ctrl^C");

                //System.Diagnostics.Debug.Print("Sent CTRL + C...");

                e.Handled = true;
                return;
            }

            DocumentLine line = GetCurrentLine();

            switch (e.Key)
            {
                case Key.Enter:
                    if (!string.IsNullOrEmpty(line.Text))
                    {
                        cmdList.Add(line.Text);
                        cmdListIndex = cmdList.Count;
                    }

                    runningCommand = true;

                    //EnterPressingEventArgs epEvent = new EnterPressingEventArgs();
                    //epEvent.CommandLineText = line.Text;
                    //epEvent.Handle = false;
                    //OnEnterPressing(this, epEvent);

                    if (!ProcessInternalCommands(line.Text))
                    {
                        //  Write the input.
                        lastInput = line.Text;
                        proc.WriteInput(line.Text);
                        if (line.Text.ToLower().StartsWith("cd "))
                        {
                            ConsolePrompt = string.Format("[{0}@{1} {2}]$ ", Environment.UserName, Environment.UserDomainName, Path.GetFileName(proc.Process.StartInfo.WorkingDirectory));
                        }
                    }

                    runningCommand = false;

                    CaretIndex = Text.Length;

                    e.Handled = true;
                    break;
                case Key.Up:
                    if (cmdList.Count > 0)
                    {
                        cmdListIndex -= (cmdListIndex == 0 ? 0 : 1);

                        if (!string.IsNullOrEmpty(line.Text))
                        {
                            base.Text = base.Text.Remove(base.Text.Length - line.Text.Length);
                        }
                        base.AppendText(cmdList[cmdListIndex]);

                        base.CaretIndex = base.Text.Length;
                    }

                    e.Handled = true;
                    break;
                case Key.Down:
                    if (cmdList.Count > 0)
                    {
                        cmdListIndex += (cmdListIndex == cmdList.Count - 1 ? 0 : 1);

                        if (!string.IsNullOrEmpty(line.Text))
                        {
                            base.Text = base.Text.Remove(base.Text.Length - line.Text.Length);
                        }
                        base.AppendText(cmdList[cmdListIndex]);

                        base.CaretIndex = base.Text.Length;
                    }

                    e.Handled = true;
                    break;
                case Key.Back:
                case Key.Left:
                    if (base.CaretIndex <= base.Text.Length - line.Text.Length)
                    {
                        e.Handled = true;
                    }
                    break;
                case Key.Right:
                    if (base.CaretIndex == line.EndOffset)
                    {
                        e.Handled = true;
                    }
                    break;
            }
        }

        #endregion

        #region Public Events

        public delegate void EnterPressingEventHandler(Console2 sender, EnterPressingEventArgs e);
        private EnterPressingEventHandler OnEnterPressing;

        public event EnterPressingEventHandler EnterPressing
        {
            add { OnEnterPressing += new EnterPressingEventHandler(value); }
            remove { OnEnterPressing -= new EnterPressingEventHandler(value); }
        }

        #endregion

        #region Public Methods

        public int FindNext(string finding)
        {
            string content = base.Text.Remove(0, base.CaretIndex);

            int startIndex = content.IndexOf(finding);
            if (startIndex > -1)
            {
                startIndex += base.CaretIndex;
                base.Select(startIndex, finding.Length);
            }
            return startIndex;
        }

        public void Replace(string finding, string replacement)
        {
            int caretIndex = this.FindNext(finding);

            if (caretIndex > -1)
            {
                base.SelectedText = replacement;

                base.CaretIndex = caretIndex + replacement.Length;
            }
            else
            {
                base.CaretIndex = 0;
                base.ScrollToHome();
            }
        }

        public void ReplaceAll(string finding, string replacement)
        {
            base.CaretIndex = 0;
            int caretIndex = 0;

            while (caretIndex > -1)
            {
                caretIndex = this.FindNext(finding);
                if (caretIndex > -1)
                {
                    base.SelectedText = replacement;
                    base.CaretIndex = caretIndex + replacement.Length;
                }
            }

            base.CaretIndex = 0;
            base.ScrollToHome();
        }

        #endregion

        #region Private Methods

        private bool ProcessInternalCommands(string p)
        {

            return false;
        }

        private void WriteOutput(string output)
        {
            if (string.IsNullOrEmpty(lastInput) == false && output == lastInput) return;

            if (output.Replace("\r\n", "") == lastInput)
            {
                output = "\r\n";
            }

            Match m = regPrompt.Match(output);
            if (m.Success)
            {
                ConsolePrompt = string.Format("[{0}@{1} {2}]$ ", Environment.UserName, Environment.UserDomainName, Path.GetFileName(m.Value.Trim().Substring(0, m.Value.Trim().Length - 1)));
                output = output.Replace(m.Value.Trim(), ConsolePrompt);
            }

            Dispatcher.BeginInvoke(new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    //  Write the output.
                    AppendText(output);
                    ScrollToEnd();
                }), new object[] { this, System.EventArgs.Empty });
        }

        private void CodeTypeLoaded(object sender, RoutedEventArgs e)
        {
            //base.AppendText(ConsolePrompt);
            base.CaretIndex = base.Text.Length;
            Focusable = true;

            this.InvalidateVisual();
        }

        private DocumentLine GetCurrentLine()
        {
            return new DocumentLine(this, CurrentLineIndex, ConsolePrompt);
        }

        #endregion
    }

    #region Related Classes

    public class DocumentLine
    {
        public DocumentLine(CodeBox doc, int lineIndex, string prompt)
        {
            Index = lineIndex;
            OriginalText = doc.GetLineText(lineIndex);
            Text = OriginalText;
            if (Text.StartsWith(prompt))
            {
                Text = Text.Remove(0, prompt.Length);
            }
            EndOffset = doc.GetCharacterIndexFromLineIndex(lineIndex) + doc.GetLineLength(lineIndex);
            PosInLine = doc.CaretIndex - doc.GetStartIndexFromLineIndex(lineIndex);
        }

        public int Index { get; private set; }

        public string Text { get; private set; }

        public string OriginalText { get; private set; }

        public int PosInLine { get; private set; }

        public int EndOffset { get; private set; }
    }

    public class EnterPressingEventArgs : EventArgs
    {
        private string cmd = "";

        public EnterPressingEventArgs() : base() { }

        public string CommandLineText
        {
            get
            {
                return cmd;
            }
            set
            {
                StringBuilder sb = new StringBuilder();
                bool isSpace = false;
                foreach (char c in value.ToCharArray())
                {
                    if (c == ' ' && isSpace)
                    {
                        continue;
                    }
                    else if (c == ' ' && !isSpace)
                    {
                        isSpace = true;
                    }
                    else if (c != ' ' && isSpace)
                    {
                        isSpace = false;
                    }
                    sb.Append(c);
                }
                cmd = sb.ToString().Trim();
            }
        }

        public bool Handle { get; set; }

        public string GetCommand()
        {
            return CommandLineText.Split(' ')[0];
        }

        public string[] GetArgs()
        {
            List<string> args = new List<string>(CommandLineText.Split(' '));
            args.RemoveAt(0);

            return args.ToArray();
        }
    }

    #endregion
}
