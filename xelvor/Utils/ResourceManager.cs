using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace xelvor.Utils
{
    public class ResourceManager
    {
        private static FontFamily font = null;

        public static ImageSource Icon
        {
            get
            {
                return IconManager.GetIcon("icon");
            }
        }

        public static FontFamily Font
        {
            get
            {
                if (font == null)
                {
                    font = new FontFamily(new Uri("pack://application:,,,/Resources/"), "./#Mono AA");
                }

                return font;
            }
        }
    }
}
