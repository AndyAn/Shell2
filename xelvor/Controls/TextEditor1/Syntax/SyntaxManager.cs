using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Media;
using xelvor.Controls.TextEditor;

namespace xelvor.Controls.TextEditor.Syntax
{
    internal static class SyntaxManager
    {
        private static List<SyntaxDefinition> SyntaxSet = new List<SyntaxDefinition>();

        internal static void Initialize()
        {
            string Prefix = "xelvor.Controls.TextEditor.Resources.";
            XmlReader reader = XmlReader.Create(typeof(SyntaxManager).Assembly.GetManifestResourceStream(Prefix + "Hosts.xml"));
            SyntaxSet.Add(ParseSyntaxFile(reader));
        }

        internal static List<Pair> ParseCode(string identifier, string code)
        {
            // identifier could be extension name or syntax file type
            SyntaxDefinition sd = null;
            List<Pair> pairList = new List<Pair>();

            if (identifier.StartsWith("."))
            {
                sd = SyntaxSet.SingleOrDefault(s => s.Extensions.Contains(identifier.TrimStart('.').ToLower()));
            }
            else
            {
                sd = SyntaxSet.SingleOrDefault(s => s.Name == identifier.ToLower());
            }

            if (sd != null)
            {
                foreach (Rule rule in sd.RuleSet)
                {
                    MatchCollection matches = rule.RuleRegex.Matches(code);
                    foreach (Match m in matches)
                    {
                        pairList.Add(new Pair(m.Index, m.Length, rule.Color));
                    }
                }
            }

            return pairList;
        }

        private static SyntaxDefinition ParseSyntaxFile(XmlReader reader)
        {
            SyntaxDefinition syntax = new SyntaxDefinition();

            XmlDocument xml = new XmlDocument();

            xml.Load(reader);

            // read information
            XmlNode node = xml.SelectSingleNode("/Syntax/Info");
            syntax.Name = node.ChildNodes[0].InnerText.ToLower();
            XmlNodeList list = node.SelectNodes("//Extension/Item");
            foreach (XmlNode n in list)
            {
                syntax.Extensions.Add(n.InnerText.ToLower());
            }

            // read ruleset
            list = xml.SelectNodes("/Syntax/RuleSet/Rule");
            foreach (XmlNode n in list)
            {
                Rule rule = new Rule();

                foreach (XmlNode item in n.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "Name":
                            rule.Name = item.InnerText.ToLower();
                            break;
                        case "Identify":
                            rule.RuleRegex = new Regex(item.InnerText, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            break;
                        case "Color":
                            rule.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.InnerText));
                            break;
                    }
                }

                syntax.RuleSet.Add(rule);
            }

            return syntax;
        }
    }
}
