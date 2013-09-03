using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Forms = System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;

namespace AiP.Metro
{
    public enum WindowType
    {
        Normal,
        Popup,
        ShowBox
    }

    public class MetroWindow : MetroBase
    {
        private readonly int agWidth = 4; //12;
        private readonly int borderThickness = 4;
        private int controlBoxItemCount = 0;
        private Point mousePoint = new Point();
        private Point windowPoint = new Point();

        #region Constructor

        public MetroWindow()
        {
            Margin = new Thickness(10);
            WindowType = Metro.WindowType.Normal;
        }

        #endregion

        #region Dependency properties

        public static DependencyProperty TitleBorderBrushProperty = DependencyProperty.Register("TitleBorderBrush", typeof(Brush), typeof(MetroWindow),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke), FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush TitleBorderBrush
        {
            get { return (Brush)GetValue(TitleBorderBrushProperty); }
            set { SetValue(TitleBorderBrushProperty, value); }
        }

        public static DependencyProperty TitleBrushProperty = DependencyProperty.Register("TitleBrush", typeof(Brush), typeof(MetroWindow),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke), FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush TitleBrush
        {
            get { return (Brush)GetValue(TitleBrushProperty); }
            set { SetValue(TitleBrushProperty, value); }
        }

        public static DependencyProperty WindowTypeProperty = DependencyProperty.Register("WindowType", typeof(Enum), typeof(MetroWindow),
            new FrameworkPropertyMetadata(WindowType.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public WindowType WindowType
        {
            get { return (WindowType)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }

        //public static DependencyProperty QuickAccessToolBarProperty = DependencyProperty.Register("QuickAccessToolBar", typeof(QuickAccessItems), typeof(MetroWindow));

        //public QuickAccessItems QuickAccessToolBar
        //{
        //    get { return (QuickAccessItems)GetValue(QuickAccessToolBarProperty); }
        //    set
        //    {
        //        SetValue(QuickAccessToolBarProperty, value);
        //    }
        //}

        #endregion

        #region Properties

        //public WrapPanel QuickAccessToolBar
        //{
        //    get { return GetControl<WrapPanel>("QuickAccessToolBar"); }
        //}

        #endregion

        #region Overrided Events

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }

            ProcessTitleBar();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowType == Metro.WindowType.Normal)
            {
                Button Max = Template.FindName("Max", this) as Button;
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
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        internal new void Show()
        {
            Style = WindowType == Metro.WindowType.Normal ? GetResources<Style>("MetroWindow") : GetResources<Style>("MetroOverlay");

            base.Show();
        }

        internal new bool? ShowDialog()
        {
            Style = WindowType == Metro.WindowType.Normal ? GetResources<Style>("MetroWindow") : GetResources<Style>("MetroOverlay");

            return base.ShowDialog();
        }

        internal virtual IntPtr WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            switch (msg)
            {
                case Win32API.WM_NCHITTEST:
                    mousePoint.X = Forms.Control.MousePosition.X;
                    mousePoint.Y = Forms.Control.MousePosition.Y;

                    #region process minimized height and width

                    if (Width < MinWidth || Height < MinHeight)
                    {
                        Left = windowPoint.X;
                        Top = windowPoint.Y;
                        Width = Width < MinWidth ? MinWidth : Width;
                        Height = Height < MinHeight ? MinHeight : Height;

                        return IntPtr.Zero;
                    }

                    #endregion

                    #region test mouse location

                    if (WindowType == Metro.WindowType.Normal)
                    {
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
                        else if ((mousePoint.Y - Location.Y - Margin.Top <= 32 && mousePoint.X - Location.X - Margin.Left - Margin.Right <= ActualWidth - controlBoxItemCount * 32) ||
                            (mousePoint.Y - Location.Y - Margin.Top > 32 && mousePoint.Y - Location.Y - Margin.Top <= 48))
                        {
                            handled = true;
                            return new IntPtr((int)Win32API.HitTest.HTCAPTION);
                        }
                    }
                    else if (WindowType == Metro.WindowType.ShowBox)
                    {
                        // moving window
                        if ((mousePoint.Y - Location.Y - Margin.Top <= 32 && mousePoint.X - Location.X - Margin.Left - Margin.Right <= ActualWidth - controlBoxItemCount * 32) || mousePoint.Y - Location.Y - Margin.Top > 32)
                        {
                            handled = true;
                            return new IntPtr((int)Win32API.HitTest.HTCAPTION);
                        }
                    }
                    else
                    {
                        // moving window
                        if (mousePoint.Y - Location.Y - Margin.Top <= 32 && mousePoint.X - Location.X - Margin.Left - Margin.Right <= ActualWidth - controlBoxItemCount * 32)
                        {
                            handled = true;
                            return new IntPtr((int)Win32API.HitTest.HTCAPTION);
                        }
                    }

                    break;

                    #endregion

                case Win32API.WM_GETMINMAXINFO:
                    if (WindowType == Metro.WindowType.Normal)
                    {
                        Win32API.WmGetMinMaxInfo(hwnd, lParam);
                    }
                    handled = true;
                    break;

                case Win32API.WM_SIZING:
                    windowPoint.X = Left;
                    windowPoint.Y = Top;
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Private methods

        private void ProcessTitleBar()
        {
            Button button;

            if (WindowType == Metro.WindowType.Normal)
            {
                Border border = GetControl<Border>("Title");
                border.SetBinding(Border.BorderBrushProperty, new Binding() { Path = new PropertyPath("TitleBorderBrush"), Mode = BindingMode.OneWay, Source = this });

                TextBlock text = VisualTreeHelper.GetChild(border, 0) as TextBlock;
                text.SetBinding(TextBlock.ForegroundProperty, new Binding() { Path = new PropertyPath("TitleBrush"), Mode = BindingMode.OneWay, Source = this });

                button = GetControl<Button>("Min");
                button.Click += new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) =>
                {
                    WindowState = WindowState.Minimized;
                }));

                button = GetControl<Button>("Max");
                button.Click += new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) =>
                {
                    if (WindowState == WindowState.Normal)
                    {
                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        WindowState = WindowState.Normal;
                    }
                }));

                controlBoxItemCount = GetControl<Canvas>("ControlBox").Children.Count + 1;
                //controlBoxItemCount = GetControl<Canvas>("ControlBox").Children.Count + GetControl<WrapPanel>("QuickAccessToolBar").Children.Count + 1;

                //QuickAccessToolBar = new QuickAccessItems(GetControl<WrapPanel>("QuickAccessToolBar"));
                //QuickAccessToolBar.PropertyChanged += new PropertyChangedEventHandler(QuickAccessToolBar_PropertyChanged);
            }
            else
            {
                if (WindowType == Metro.WindowType.Popup)
                {
                    GetControl<Canvas>("Bar").Background = new SolidColorBrush(Color.FromArgb(21, 255, 255, 255));
                }

                controlBoxItemCount = 2;
            }

            button = GetControl<Button>("Close");
            button.Click += new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) =>
            {
                Close();
            }));
        }

        void QuickAccessToolBar_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            controlBoxItemCount = GetControl<Canvas>("ControlBox").Children.Count + GetControl<WrapPanel>("QuickAccessToolBar").Children.Count + 1;
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

        #endregion
    }

    public class QuickAccessItems : WrapPanel
    {
        public QuickAccessItems()
        {
            //QuickAccessItems.
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = new PropertyChangedEventHandler(new Action<object, PropertyChangedEventArgs>((sender, e) => { }));

        #endregion
    }
}
