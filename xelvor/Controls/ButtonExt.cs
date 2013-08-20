using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace xelvor.Controls
{
    public class ButtonExt : Button
    {
        static ButtonExt()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonExt), new FrameworkPropertyMetadata(typeof(ButtonExt)));
        }

        public string HighLightBackground
        {
            get { return (string)GetValue(HighLightBackgroundProperty); }
            set { SetValue(HighLightBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HighLightBackgroundProperty =
            DependencyProperty.Register("HighLightBackground", typeof(string), typeof(ButtonExt), new PropertyMetadata(null));

        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(ButtonExt), new UIPropertyMetadata(null));

        public string ImageUri
        {
            get { return (string)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
        }

        public static readonly DependencyProperty ImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(string), typeof(ButtonExt), new UIPropertyMetadata(string.Empty,
              (o, e) =>
              {
                  try
                  {
                      Uri uriSource = new Uri((string)e.NewValue, UriKind.RelativeOrAbsolute);
                      if (uriSource != null)
                      {
                          ButtonExt button = o as ButtonExt;
                          BitmapImage img = new BitmapImage(uriSource);
                          button.SetValue(ImageSourceProperty, img);
                      }
                  }
                  catch (Exception ex)
                  {
                      throw ex;
                  }
              }));

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(ButtonExt), new UIPropertyMetadata(null));
    }
}