using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace xelvor.Controls.TextEditor.Syntax
{
    internal class Pair
    {
        internal int Start { get; private set; }
        internal int Length { get; private set; }
        internal Brush Color { get; private set; }

        internal Pair() { } 

        internal Pair(int start, int len, Brush color)
        {
            this.Start = start;
            this.Length = len;
            this.Color = color;
        }
    }
}
