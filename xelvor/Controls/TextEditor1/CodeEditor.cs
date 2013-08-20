using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using xelvor.Controls.TextEditor.Syntax;

namespace xelvor.Controls.TextEditor
{
    public class CodeEditor : TextBox
    {
        #region Variables
        bool scrollingEventEnabled;

        //private List<Syntax> decList = new List<Syntax>();

        private double lineHeight = 0;

        List<string> cmdList = new List<string>();

        int cmdListIndex = 0;

        bool runningCommand = false;

        #endregion

        #region Dependency properties
        public static DependencyProperty BaseForegroundProperty = DependencyProperty.Register("BaseForeground", typeof(Brush), typeof(CodeEditor),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke), FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Construction
        static CodeEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeEditor), new FrameworkPropertyMetadata(typeof(CodeEditor)));
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Brush of base foreground
        /// </summary>
        public Brush BaseForeground
        {
            get { return (Brush)GetValue(BaseForegroundProperty); }
            set { SetValue(BaseForegroundProperty, value); }
        }

        /// <summary>
        /// Tab size (spaces)
        /// </summary>
        public int TabSize { get; set; }

        /// <summary>
        /// Prompt on Console
        /// </summary>
        public string ConsolePrompt { get; set; }

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
        //public List<Syntax> Decorations
        //{
        //    get { return decList; }
        //    set { decList = value; }
        //}
        #endregion

        #region Private properties
        private string Tab
        {
            get
            {
                return new String(' ', TabSize);
            }
        }

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

        #region Protected overrides
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            base.Foreground = Brushes.Transparent;
            //base.Foreground = Brushes.Red;
            base.Background = Brushes.Transparent;
            base.CaretBrush = Brushes.WhiteSmoke;
            base.FontFamily = new FontFamily(new Uri("pack://application:,,,/Controls/TextEditor/Resources/"), "./#Mono AA");
            base.FontSize = 14.0;
            base.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            base.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            base.Loaded += new RoutedEventHandler(CodeEditorLoaded);
            base.MaxLength = int.MaxValue;
            base.AcceptsReturn = true;
            base.AcceptsTab = false;
            base.Focus();

            // load syntax files
            //SyntaxManager.Initialize();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            this.InvalidateVisual();
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

                    EnterPressingEventArgs epEvent = new EnterPressingEventArgs();
                    epEvent.CommandLineText = line.Text;
                    epEvent.Handle = false;

                    OnEnterPressing(this, epEvent);

                    runningCommand = false;

                    if (!epEvent.Handle)
                    {
                        // internal command: settings
                        base.AppendText("\r\nInternal Handled!");
                    }

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

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            EnsureScrolling();
            RenderRuntime(drawingContext);
        }
        #endregion

        #region Dependency properties callbacks
        /// <summary>
        /// always set transparent color
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (e.NewValue != Brushes.Transparent)
            {
                ((CodeEditor)d).Foreground = Brushes.Transparent;
            }
        }

        ///// <summary>
        /////  always set transparent color
        ///// </summary>
        ///// <param name="d"></param>
        ///// <param name="e"></param>
        //private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    CodeEditor sh = (CodeEditor)d;

        //    SolidColorBrush b = new SolidColorBrush(sh.GetAlphaColor(sh.CursorColor));
        //    SolidColorBrush a = e.NewValue as SolidColorBrush;
        //    if (a == null || a.Color != b.Color)
        //    {
        //        sh.Background = b;
        //    }

        //}

        ///// <summary>
        ///// Syntax rules changed
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="args"></param> 
        //static void OnSyntaxRulesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        //{
        //    ((CodeEditor)obj)._rules = null; // flush cache

        //    // do event
        //    RoutedEventArgs arg = new RoutedEventArgs(CodeEditor.SyntaxRulesChangedEvent);
        //    ((CodeEditor)obj).RaiseEvent(arg);
        //}
        #endregion

        #region Public event
        public delegate void EnterPressingEventHandler(CodeEditor sender, EnterPressingEventArgs e);
        private EnterPressingEventHandler OnEnterPressing;

        public event EnterPressingEventHandler EnterPressing
        {
            add { OnEnterPressing += new EnterPressingEventHandler(value); }
            remove { OnEnterPressing -= new EnterPressingEventHandler(value); }
        }
        #endregion

        #region Public methods

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

        #region Private methods

        #region Renderring
        /// <summary>
        ///The main render code
        /// </summary>
        /// <param name="drawingContext"></param>
        protected void RenderRuntime(DrawingContext drawingContext)
        {
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.ViewportWidth, this.ViewportHeight)));
            drawingContext.DrawRectangle(this.Background, null, new Rect(0, 0, this.ViewportWidth, this.ViewportHeight));

            //if (this.Text == "") return;

            double leftMargin = 2.0 + this.BorderThickness.Left;
            double topMargin = 0.0 + this.BorderThickness.Top;

            string visibleText = this.GetVisibleText();
            if (visibleText == null) return;

            FormattedText formattedText = new FormattedText(
                visibleText,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                BaseForeground);  //Text that matches the textbox's
            formattedText.Trimming = TextTrimming.None;

            ApplyTextWrapping(formattedText);

            Point renderPoint = GetRenderPoint(leftMargin);
            if (double.IsInfinity(renderPoint.X))
            {
                this.InvalidateVisual();
            }

            // draw text
            formattedText.SetForegroundBrush(BaseForeground);
            formattedText.SetFontWeight(FontWeights.Normal);

            //TextColor
            List<Pair> pairList = SyntaxManager.ParseCode("Hosts", visibleText);
            foreach (Pair pair in pairList)
            {
                formattedText.SetForegroundBrush(pair.Color, pair.Start, pair.Length);
            }

            //drawingContext.PushEffect(fontEffect, null);
            drawingContext.DrawText(formattedText, renderPoint);
        }

        /// <summary>
        /// Gets the Text that is visible in the textbox. Please note that it depends on
        /// GetFirstVisibleLineIndex and GetLastVisibleLineIndex methods
        /// </summary>
        private string GetVisibleText()
        {
            if (this.Text.Length == 0)
            {
                return string.Empty;
            }

            string visibleText = string.Empty;

            try
            {
                int firstLine = (int)(this.VerticalOffset / this.lineHeight);
                int lastLine = (int)((this.VerticalOffset + this.ViewportHeight) / this.lineHeight) + 1;

                StringBuilder text = new StringBuilder();

                if (this.LineCount == 0) // can't get layout information
                {
                    string[] lines = this.Text.Split(new string[] { "\r\n", "\r", "\n", "\f", "\v" }, StringSplitOptions.None);
                    lastLine = (lastLine > lines.Length ? lines.Length : lastLine);

                    for (int i = firstLine; i < lastLine; i++)
                    {
                        text.AppendLine(lines[i]);
                    }
                }
                else
                {
                    lastLine = (lastLine > this.LineCount ? this.LineCount : lastLine);

                    for (int i = firstLine; i < lastLine; i++)
                    {
                        text.Append(this.GetLineText(i));
                    }
                }

                if (text.ToString().Length > 0)
                {
                    visibleText = text.ToString();
                }
            }
            catch
            {
                return "Debug - GetVisibleText failure";
            }

            return visibleText;
        }

        private void ApplyTextWrapping(FormattedText formattedText)
        {
            switch (this.TextWrapping)
            {
                case TextWrapping.NoWrap:
                    break;
                case TextWrapping.Wrap:
                    formattedText.MaxTextWidth = this.ViewportWidth; //Used with Wrap only
                    break;
                case TextWrapping.WrapWithOverflow:
                    formattedText.SetMaxTextWidths(VisibleLineWidthsIncludingTrailingWhitespace());
                    break;
            }

        }

        /// <summary>
        /// Returns the line widths for use with the wrap with overflow.
        /// </summary>
        /// <returns></returns>
        private Double[] VisibleLineWidthsIncludingTrailingWhitespace()
        {

            int firstLine = this.GetFirstVisibleLineIndex();
            int lastLine = Math.Max(this.GetLastVisibleLineIndex(), firstLine);
            Double[] lineWidths = new Double[lastLine - firstLine + 1];
            if (lineWidths.Length == 1)
            {
                lineWidths[0] = MeasureString(this.Text);
            }
            else
            {
                for (int i = firstLine; i <= lastLine; i++)
                {
                    string lineString = this.GetLineText(i);
                    lineWidths[i - firstLine] = MeasureString(lineString);
                }
            }
            return lineWidths;
        }

        /// <summary>
        /// Gets the Renderpoint, the top left corner of the first character displayed. Note that this can 
        /// have negative vslues when the textbox is scrolling.
        /// </summary>
        /// <param name="firstChar">The first visible character</param>
        /// <returns></returns>
        private Point GetRenderPoint(double leftMargin)
        {
            int firstLine = (int)(this.VerticalOffset / this.lineHeight);
            firstLine = this.GetFirstVisibleLineIndex();

            if (this.CurrentLineIndex == -1)
            {
                return new Point(double.NegativeInfinity, double.NegativeInfinity);
            }
            int firstChar = this.GetCharacterIndexFromLineIndex(firstLine);
            Rect rect = GetRectFromCharacterIndex(firstChar);

            //int lastLine = (int)((this.VerticalOffset + this.ViewportHeight) / this.lineHeight) + 1;
            //lastLine = (lastLine > this.LineCount ? this.LineCount : lastLine);
            //Point renderPoint = new Point(leftMargin - this.HorizontalOffset, firstLine * this.lineHeight - this.VerticalOffset);
            Point renderPoint = new Point(leftMargin - this.HorizontalOffset, rect.Top);
            return renderPoint;
        }

        private void EnsureScrolling()
        {
            if (!scrollingEventEnabled)
            {
                DependencyObject dp = VisualTreeHelper.GetChild(this, 0);
                ScrollViewer sv = VisualTreeHelper.GetChild(dp, 0) as ScrollViewer;
                sv.ScrollChanged += new ScrollChangedEventHandler(ScrollChanged);
                scrollingEventEnabled = true;
            }
        }
        #endregion

        #region Private Events - CodeEditorLoaded & ScrollChanged
        private void CodeEditorLoaded(object sender, RoutedEventArgs e)
        {
            if (lineHeight == 0)
            {
                Rect rect = base.GetRectFromCharacterIndex(base.CaretIndex);
                lineHeight = rect.Bottom - rect.Top;
            }

            base.AppendText(ConsolePrompt);
            base.CaretIndex = base.Text.Length;

            this.InvalidateVisual();
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InvalidateVisual();
        }
        #endregion

        /// <summary>
        /// Returns the width of the string in the font and fontsize of the textbox including the trailing white space.
        /// Used for wrap with overflow.
        /// </summary>
        /// <param name="str">The string to measure</param>
        /// <returns></returns>
        private double MeasureString(string str)
        {
            FormattedText formattedText = new FormattedText(
                str,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                new SolidColorBrush(Colors.Black));

            if (str == "")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else if (str.Substring(0, 1) == "\t")
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
            else
            {
                return formattedText.WidthIncludingTrailingWhitespace;
            }
        }

        private DocumentLine GetCurrentLine()
        {
            return new DocumentLine(this, CurrentLineIndex, ConsolePrompt);
        }

        #endregion
    }

    #region Related classes
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
