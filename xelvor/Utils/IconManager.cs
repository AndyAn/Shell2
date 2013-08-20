using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using Drawing2D = System.Drawing.Drawing2D;
using System.Windows.Resources;

namespace xelvor.Utils
{
    internal static class IconManager
    {
        public static ImageSource GetIcon(string name)
        {
            Uri uriSource = new Uri(string.Format("/xelvor;component/Resources/{0}.png", name.Trim().ToLower()), UriKind.RelativeOrAbsolute);
            StreamResourceInfo sri = Application.GetResourceStream(uriSource);
            ImageSource icon = null;
            
            PngBitmapDecoder decoder;

            decoder = new PngBitmapDecoder(sri.Stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

            if (decoder.Frames != null && decoder.Frames.Count > 0)
                icon = decoder.Frames[0];

            return icon;
        }

        public static ImageSource DrawIcon(Brush fillBrush, string message)
        {
            int dimension = 32;

            Color color = (fillBrush as SolidColorBrush).Color;
            Drawing.Bitmap bitmap = DrawIcon(new Drawing.SolidBrush(Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)), message, dimension).ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());

            return icon;
        }

        private static Drawing.Icon DrawIcon(Drawing.Brush fillBrush, string message, int dimension)
        {
            Drawing.Icon oIcon = null;

            Drawing.Bitmap bm = new Drawing.Bitmap(dimension, dimension);
            Drawing.Graphics g = Drawing.Graphics.FromImage((Drawing.Image)bm);
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias;
            Drawing.Font oFont = new Drawing.Font("Arial", 30, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Pixel);
            g.FillRectangle(fillBrush, new Drawing.Rectangle(0, 0, bm.Width, bm.Height));
            g.DrawString(message, oFont, new Drawing.SolidBrush(Drawing.Color.Black), 2, 0);
            oIcon = Drawing.Icon.FromHandle(bm.GetHicon());
            oFont.Dispose();
            g.Dispose();
            bm.Dispose();
            return oIcon;
        }
    }
}
