using System.Windows;
using System;

namespace AiP.Metro
{
    public class MetroBase : Window
    {
        static MetroBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroBase), new FrameworkPropertyMetadata(typeof(MetroBase)));
        }

        internal MetroBase()
        {
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            LoadResources();
        }

        private void LoadResources()
        {
            #region Initialize resources to window

            Resources.MergedDictionaries.Add(ResourceHelper.GetResources());

            #endregion
        }

        internal Point Location
        {
            get
            {
                return new Point(WindowState == WindowState.Maximized ? 0 : Left, WindowState == WindowState.Maximized ? 0 : Top);
            }
        }

        internal T GetControl<T>(string name)
        {
            return (T)Template.FindName(name, this);
        }

        internal T GetResources<T>(string key)
        {
            if (Resources.Contains(key))
            {
                return (T)Resources[key];
            }
            else
            {
                throw new Exception("The resource[key: " + key + "] cannot be found in the dictionary.");
            }
        }
    }
}
