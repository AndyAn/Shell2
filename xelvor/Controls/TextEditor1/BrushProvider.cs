using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace xelvor.Controls.TextEditor
{
    public static class BrushProvider
    {
        private static LinearGradientBrush linenumberBrush = null;
        public static Brush GetLineNumberBrush
        {
            get
            {
                if (linenumberBrush == null)
                {
                    linenumberBrush = new LinearGradientBrush();
                    linenumberBrush.StartPoint = new Point(0, 0.5);
                    linenumberBrush.EndPoint = new Point(1, 0.5);
                    linenumberBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 128, 128, 128), 0));
                    linenumberBrush.GradientStops.Add(new GradientStop(Color.FromArgb(128, 128, 128, 128), 0.5));
                    linenumberBrush.GradientStops.Add(new GradientStop(Color.FromArgb(128, 128, 128, 128), 1));
                }

                return linenumberBrush;
            }
        }
    }
}
