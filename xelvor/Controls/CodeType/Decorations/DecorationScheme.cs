using System.Collections.Generic;
using System.ComponentModel;

namespace xelvor.Controls.CodeType.Decorations
{
    [TypeConverter(typeof(DecorationSchemeTypeConverter))]
    public class DecorationScheme
    {
        List<Decoration> mDecorations = new List<Decoration>();

        public List<Decoration> BaseDecorations
        {
            get { return mDecorations; }
            set { mDecorations = value; }
        }

        public string Name { get; set; }
    }
}
