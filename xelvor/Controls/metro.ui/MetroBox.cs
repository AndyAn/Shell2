using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AiP.Metro
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public class MetroBox : MetroBase
    {
        private MessageBoxButton BtnGrp;
        private static MessageBoxResult result;
        private MetroBase owner = null;

        #region Constructors

        public MetroBox(string title, MessageBoxButton button)
        {
            TryToGetOwner();

            Margin = (owner.WindowState == WindowState.Maximized) ? new Thickness(0, 10, 0, 10) : new Thickness(10);

            Title = title;
            Width = OwnerWidth;
            ShowInTaskbar = false;
            Hide();

            BtnGrp = button;
        }

        public MetroBox(MessageBoxButton button) : this("Information", button)
        {
        }

        public MetroBox(string title) : this(title, MessageBoxButton.OK)
        {
        }

        public MetroBox() : this("Information", MessageBoxButton.OK)
        {
        }

        #endregion

        #region Private Properties

        private double OwnerWidth
        {
            get
            {
                double w = 0;

                w = double.IsNaN(owner.Width) ? 0 : owner.Width;
                if (w == 0)
                {
                    w = double.IsNaN(owner.ActualWidth) ? 0 : owner.ActualWidth;
                    if (w == 0)
                    {
                        w = SystemParameters.WorkArea.Width;
                    }
                }

                return w;
            }
        }

        private double OwnerHeight
        {
            get
            {
                double h = 0;

                h = double.IsNaN(owner.Height) ? 0 : owner.Height;
                if (h == 0)
                {
                    h = double.IsNaN(owner.ActualHeight) ? 0 : owner.ActualHeight;
                    if (h == 0)
                    {
                        h = SystemParameters.WorkArea.Height;
                    }
                }

                return h;
            }
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Style = GetResources<Style>("MetroBox");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            GetControl<Run>("AppName").Text = GetType().Assembly.EntryPoint.DeclaringType.Namespace.Split('.')[0] + " :";

            WrapPanel ButtonGroup = GetControl<WrapPanel>("ButtonGroup");
            Button Yes = GetControl<Button>("Yes");
            Button No = GetControl<Button>("No");
            Button Cancel = GetControl<Button>("Cancel");
            Button Okay = GetControl<Button>("Okay");
            switch (BtnGrp)
            {
                case MessageBoxButton.OK:
                    ButtonGroup.Children.Remove(Yes);
                    ButtonGroup.Children.Remove(No);
                    ButtonGroup.Children.Remove(Cancel);
                    Okay.Click += OkayButtonClick;
                    break;
                case MessageBoxButton.OKCancel:
                    ButtonGroup.Children.Remove(Yes);
                    ButtonGroup.Children.Remove(No);
                    Okay.Click += OkayButtonClick;
                    Cancel.Click += CancelButtonClick;
                    break;
                case MessageBoxButton.YesNo:
                    ButtonGroup.Children.Remove(Okay);
                    ButtonGroup.Children.Remove(Cancel);
                    Yes.Click += YesButtonClick;
                    No.Click += NoButtonClick;
                    break;
                case MessageBoxButton.YesNoCancel:
                    ButtonGroup.Children.Remove(Okay);
                    Yes.Click += YesButtonClick;
                    No.Click += NoButtonClick;
                    Cancel.Click += CancelButtonClick;
                    break;
            }
        }

        internal new MessageBoxResult ShowDialog()
        {
            Left = owner.Location.X;
            Top = owner.Location.Y + (OwnerHeight - Height) / 2;
            base.ShowDialog();
            if (owner.Name == "__temp__")
            {
                owner.Close();
                owner = null;
            }
            return result;
        }

        private void TryToGetOwner()
        {
            if (owner != null) return;

            WindowCollection wc = Application.Current.Windows;
            foreach (MetroBase mw in wc)
            {
                if (mw.IsActive)
                {
                    owner = mw;
                    break;
                }
            }

            if (owner == null)
            {
                owner = new MetroBase();
                owner.Name = "__temp__";
                owner.WindowState = WindowState.Maximized;
                owner.Width = SystemParameters.WorkArea.Width;
                owner.Height = SystemParameters.WorkArea.Height;
                owner.Background = Brushes.Transparent;
                owner.Top = 0;
                owner.Left = 0;
            }
        }

        #region Event Handler Definition

        internal event RoutedEventHandler OkayButtonClick = new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) => { result = MessageBoxResult.OK; }));
        internal event RoutedEventHandler YesButtonClick = new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) => { result = MessageBoxResult.Yes; }));
        internal event RoutedEventHandler NoButtonClick = new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) => { result = MessageBoxResult.No; }));
        internal event RoutedEventHandler CancelButtonClick = new RoutedEventHandler(new Action<object, RoutedEventArgs>((sender, e) => { result = MessageBoxResult.Cancel; }));

        #endregion
    }
}
