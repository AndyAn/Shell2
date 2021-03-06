﻿using System.Collections.Generic;

namespace xelvor.Controls.CodeType.Decorations
{
    public class ExplicitDecoration : Decoration
    {
        public int Start { get; set; }
        public int Length { get; set; }

        public override List<Pair> Ranges(string Text)
        {
            List<Pair> ranges = new List<Pair>();

            Pair p = new Pair(Start, Length);
            ranges.Add(p);
            return ranges;
        }

        public override bool AreRangesSorted
        {
            get { return true; }
        }
    }
}
