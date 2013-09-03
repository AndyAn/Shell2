using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AiP.Metro
{
    internal class MessagePrompt
    {
        internal static MessageBoxResult Show(string message)
        {
            return Show(message, "Information", MessageBoxButton.OK);
        }

        internal static MessageBoxResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButton.OK);
        }

        internal static MessageBoxResult Show(string message, string title, MessageBoxButton button)
        {
            MetroBox mb = new MetroBox(title, button);
            mb.Title = title;
            mb.Height = 196;
            mb.Foreground = new SolidColorBrush(Color.FromArgb(255, 140, 191, 38));
            mb.Background = new SolidColorBrush(Color.FromArgb(255, 69, 69, 69));

            mb.OkayButtonClick += new RoutedEventHandler(new Action<object, RoutedEventArgs>((o, re) =>
            {
                mb.Close();
            }));
            mb.YesButtonClick += new RoutedEventHandler(new Action<object, RoutedEventArgs>((o, re) =>
            {
                mb.Close();
            }));
            mb.NoButtonClick += new RoutedEventHandler(new Action<object, RoutedEventArgs>((o, re) =>
            {
                mb.Close();
            }));
            mb.CancelButtonClick += new RoutedEventHandler(new Action<object, RoutedEventArgs>((o, re) =>
            {
                mb.Close();
            }));

            WrapPanel wp = new WrapPanel();
            wp.HorizontalAlignment = HorizontalAlignment.Center;
            wp.VerticalAlignment = VerticalAlignment.Stretch;

            Image img = new Image();
            img.Name = "Icon";
            img.Width = 64;
            img.Height = 64;
            img.Source = ResourceHelper.GetIcon("info");

            TextBlock tb = new TextBlock();
            tb.Width = mb.Width * 2 / 3;
            tb.FontFamily = new FontFamily("Segoe");
            tb.FontSize = 18;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 176, 176, 176));
            tb.Margin = new Thickness(20, 10, 0, 0);
            tb.Text = message;

            wp.Children.Add(img);
            wp.Children.Add(tb);
            mb.Content = wp;

            MessageBoxResult result = mb.ShowDialog();
            return result;
        }
    }
}
