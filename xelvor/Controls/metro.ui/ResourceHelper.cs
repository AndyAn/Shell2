using System;
using System.Windows;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace AiP.Metro
{
    static class ResourceHelper
    {
        private static string baseNS = "";

        static ResourceHelper()
        {
            baseNS = typeof(ResourceHelper).Assembly.EntryPoint.DeclaringType.Namespace.Split('.')[0];
        }

        private static ResourceDictionary resxDict = null;
        public static ResourceDictionary GetResources()
        {
            if (resxDict == null)
            {
                Uri uri = new Uri(string.Format("/{0};component/Controls/metro.ui/Resources/Controls.xaml", baseNS), UriKind.RelativeOrAbsolute);
                try
                {
                    resxDict = Application.LoadComponent(uri) as ResourceDictionary;
                    //Application.Current.Resources.MergedDictionaries.Add(resxDict);
                }
                catch (Exception e)
                {
                    MessagePrompt.Show(e.Message);
                }
            }

            return resxDict;
        }


        private static Dictionary<string, BitmapImage> iconDict = null;
        public static BitmapImage GetIcon(string name)
        {
            if (iconDict == null)
            {
                iconDict = new Dictionary<string, BitmapImage>();
                Uri uri = new Uri(string.Format("pack://application:,,,/{0};component/Controls/metro.ui/Resources/info.png", baseNS), UriKind.RelativeOrAbsolute);
                iconDict.Add("info", new BitmapImage(uri));
            }

            if (iconDict.ContainsKey(name))
            {
                return iconDict[name];
            }

            return null;
        }
    }
}
