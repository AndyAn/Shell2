using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace xelvor.Controls.TextEditor.Syntax
{
    internal class Rule
    {
        internal string Name { get; set; }
        internal Regex RuleRegex { get; set; }
        internal Brush Color { get; set; }
    }

    internal class SyntaxDefinition
    {
        internal List<Rule> RuleSet { get; set; }
        internal string Name { get; set; }
        internal List<string> Extensions { get; set; }

        internal SyntaxDefinition()
        {
            this.RuleSet = new List<Rule>();
            this.Extensions = new List<string>();
        }
    }
}
