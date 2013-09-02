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

namespace xelvor.Controls
{
    public class Console2 : CodeBox
    {
        #region Variables

        List<string> cmdList = new List<string>();

        int cmdListIndex = 0;

        bool runningCommand = false;

        Regex regPrompt = new Regex("[a-z]:(\\\\.+)*\\>.*$", RegexOptions.IgnoreCase);
        private ProcessInterface proc = new ProcessInterface();
        private string lastInput;

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
                return ConsolePrompt.Length - 1;
            }
        }

        #endregion

        #region Constructor

        public Console2()
        {
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
        }

        #endregion

        #region Protected Overrides

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            base.Foreground = Brushes.Transparent;
            base.Background = Brushes.Transparent;
            base.CaretBrush = Brushes.WhiteSmoke;
            base.FontFamily = ResourceManager.Font;
            base.FontSize = 14.0;
            base.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            base.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            base.TextWrapping = TextWrapping.WrapWithOverflow;

            base.Loaded += new RoutedEventHandler(CodeTypeLoaded);
            base.MaxLength = int.MaxValue;
            base.AcceptsReturn = true;
            base.AcceptsTab = false;
            base.Focus();

            base.LineNumberMarginWidth = 0.0;

            // load syntax files
            //SyntaxManager.Initialize();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (runningCommand)
            {
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

                    //  Write the input.
                    proc.WriteInput(line.Text);
                    if (line.Text.ToLower().StartsWith("cd "))
                    {
                        ConsolePrompt = string.Format("[{0}@{1} {2}]$ ", Environment.UserName, Environment.UserDomainName, Path.GetFileName(proc.Process.StartInfo.WorkingDirectory));
                    }

                    runningCommand = false;

                    //if (!epEvent.Handle)
                    //{
                    //    // internal command: settings
                    //    base.AppendText("\r\nInternal Handled!");
                    //}

                    base.AppendText("\r\n" + ConsolePrompt);
                    base.CaretIndex = base.Text.Length;

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

        public List<string> GetInternalCommands()
        {
            return new List<string>() {
                "settings",
                "exit"};
        }

        #endregion

        #region Private Methods

        private void WriteOutput(string output)
        {
            if (string.IsNullOrEmpty(lastInput) == false &&
                (output == lastInput || output.Replace("\r\n", "") == lastInput))
                return;

            Dispatcher.BeginInvoke(new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    //  Write the output.
                    AppendText(regPrompt.Replace(output, ConsolePrompt));
                    ScrollToEnd();
                }), new object[] { this, System.EventArgs.Empty });
        }

        private void CodeTypeLoaded(object sender, RoutedEventArgs e)
        {
            base.AppendText(ConsolePrompt);
            base.CaretIndex = base.Text.Length;

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
        public DocumentLine(TextBox doc, int lineIndex, string prompt)
        {
            Index = lineIndex;
            Text = doc.GetLineText(lineIndex).Remove(0, prompt.Length);
            EndOffset = doc.GetCharacterIndexFromLineIndex(lineIndex) + doc.GetLineLength(lineIndex);
        }

        public int Index { get; private set; }

        public string Text { get; private set; }

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
