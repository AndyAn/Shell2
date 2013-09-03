using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AiP.Metro
{
    #region Enum

    public enum ViewMode
    {
        ExtraLarge,
        Large,
        Medium,
        Small
    }

    public enum DisplayMode
    {
        Adaptive,
        Fixed
    }

    #endregion

    public class MetroFluidView : ListView
    {
        #region Constructors

        static MetroFluidView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroFluidView), new FrameworkPropertyMetadata(typeof(MetroFluidView)));
        }

        public MetroFluidView()
        {
            AllowDrop = true;
            DisplayMode = Metro.DisplayMode.Fixed;
            ItemWidth = ItemWidth == 0 ? 96 : ItemWidth;
        }

        #endregion

        #region Dependency properties

        public static DependencyProperty MaxItemPerlineProperty = DependencyProperty.Register("MaxItemPerline", typeof(int), typeof(MetroFluidView),
            new FrameworkPropertyMetadata(6, FrameworkPropertyMetadataOptions.AffectsRender));

        public int MaxItemPerline
        {
            get { return (int)GetValue(MaxItemPerlineProperty); }
            set { SetValue(MaxItemPerlineProperty, value); }
        }

        public static DependencyProperty MinItemPerlineProperty = DependencyProperty.Register("MinItemPerline", typeof(int), typeof(MetroFluidView),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsRender));

        public int MinItemPerline
        {
            get { return (int)GetValue(MinItemPerlineProperty); }
            set { SetValue(MinItemPerlineProperty, value); }
        }

        #endregion

        #region Properties

        private ViewMode _viewstate = ViewMode.Medium;
        public ViewMode ViewMode
        {
            get { return _viewstate; }
            set
            {
                _viewstate = value;

                switch (_viewstate)
                {
                    case ViewMode.ExtraLarge:
                        ItemWidth = 160;
                        break;
                    case ViewMode.Large:
                        ItemWidth = 128;
                        break;
                    case ViewMode.Small:
                        ItemWidth = 64;
                        break;
                    case ViewMode.Medium:
                    default:
                        ItemWidth = 96;
                        break;
                }

                CalculateWidth();
                if (Items.Count > 0)
                {
                    //ListHelperDelegation("ResizeListItems", new object[] { Items as IList, ItemWidth });
                    ResizeListItems(Items as IList);
                }
            }
        }

        private DisplayMode _dispayMode = DisplayMode.Fixed;
        public DisplayMode DisplayMode
        {
            get { return _dispayMode; }
            set
            {
                _dispayMode = value;
                CalculateWidth();
            }
        }

        private double ItemWidth { get; set; }

        private double ListItemWidth { get; set; }

        #endregion

        #region Protected events

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (Items.Count > 0 && e.NewItems != null && e.NewItems.Count > 0)
            {
                //ListHelperDelegation("ResizeListItems", new object[] { e.NewItems, ItemWidth });
                ResizeListItems(e.NewItems);
            }

            double sortIndex = 1.0;
            foreach (var item in Items)
            {
                if (item is FrameworkElement)
                {
                    (item as FrameworkElement).Tag = sortIndex++;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IList list = SelectedItems as IList;
                DataObject data = new DataObject(typeof(IList), list);
                if (list.Count > 0)
                {
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            int index = GetCurrentIndex(e.OriginalSource as UIElement);
            if (index == -1) return;

            DependencyObject item = Items[index] as DependencyObject;

            if (!(item is Rectangle))
            {
                item = VisualTreeHelper.GetParent(item);
                item = VisualTreeHelper.GetParent(item);
                item = VisualTreeHelper.GetChild(item, 0);
            }

            if (item is Rectangle)
            {
                (item as Rectangle).Fill = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

            foreach (var cell in Items)
            {
                DependencyObject item = cell as DependencyObject;

                if (!(item is Rectangle))
                {
                    item = VisualTreeHelper.GetParent(item);
                    item = VisualTreeHelper.GetParent(item);
                    item = VisualTreeHelper.GetChild(item, 0);
                }

                if (item is Rectangle)
                {
                    (item as Rectangle).Fill = Brushes.Transparent;
                }
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            if (e.Data.GetDataPresent(typeof(IList)))
            {
                IList list = e.Data.GetData(typeof(IList)) as IList;

                int index = GetCurrentIndex(e.OriginalSource as UIElement);

                if (index > -1)
                {
                    //ListHelperDelegation("SortListItems", new object[] { index, list, this });
                    SortListItems(index, list);
                }
            }

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            #region Resize items before UI renderred

            if (Items.Count > 0)
            {
                //ListHelperDelegation("ResizeListItems", new object[] { this, MaxItemPerline, MinItemPerline });
                //ListHelperDelegation("ResizeListItems", new object[] { Items as IList, ItemWidth });
                ResizeListItems(Items as IList);
            }

            #endregion

            base.OnRenderSizeChanged(sizeInfo);

            #region Resize item container after it's parent's size recalculated

            var ct = GetControl<ScrollViewer>("SV", this);
            var scp = GetControl<ScrollContentPresenter>("PART_ScrollContentPresenter", ct);
            var ip = VisualTreeHelper.GetChild(scp, 0);
            var wp = VisualTreeHelper.GetChild(ip, 0) as WrapPanel;
            wp.Width = scp.ViewportWidth;

            #endregion
        }

        #endregion

        #region Public methods

        public int GetCurrentIndex(UIElement target)
        {
            int index = -1;
            for (int i = 0; i < Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i);

                if (item != null && this.IsMouseOverTarget(target, item))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        #endregion

        #region Private methods

        private void SortListItems(int idxOfDropOnItem, IList list)
        {
            if (idxOfDropOnItem > -1)
            {
                int digit = (int)Math.Pow(10, list.Count.ToString().Length);
                double incremental = 1.0 / digit;
                double baseIndex = idxOfDropOnItem;

                foreach (var item in list)
                {
                    if (item is FrameworkElement)
                    {
                        baseIndex += incremental;
                        (item as FrameworkElement).Tag = baseIndex;
                    }
                }
            }

            Items.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Ascending));
            SelectedItems.Clear();
            ResizeListItems(Items as IList);
        }

        private void ResizeListItems(IList list)
        {
            FrameworkElement vi = null;

            double sortIndex = 1.0, delta;
            foreach (var item in list)
            {
                if (item is FrameworkElement)
                {
                    vi = item as FrameworkElement;

                    delta = vi.Height - vi.Width;
                    delta = double.IsNaN(delta) || delta == 0 ? 24 : delta;

                    vi.Width = ItemWidth;
                    vi.Height = ItemWidth + delta;

                    ListViewItem lvi = ItemContainerGenerator.ContainerFromIndex((int)sortIndex - 1) as ListViewItem;
                    lvi.Width = ListItemWidth;
                    lvi.Height = ListItemWidth + delta;

                    vi.Tag = sortIndex++;
                }
            }
        }

        private bool IsMouseOverTarget(UIElement original, UIElement target)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = original.TranslatePoint(new Point(), target);
            return bounds.Contains(mousePos);
        }

        private ListViewItem GetListViewItem(int index)
        {
            if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;
            return ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        private void CalculateWidth()
        {
            #region Calculate ListItem width to adapt container's width

            double width = Width;

            if (double.IsNaN(width))
            {
                width = ActualWidth;
                if (width == 0)
                {
                    width = ItemWidth * 5 + 19 + Padding.Left * 2 + Padding.Right * 2;
                }
            }

            width = width - 19 - Padding.Left * 2 - Padding.Right * 2;

            int itemCountPerLine = MinItemPerline;

            if (DisplayMode == Metro.DisplayMode.Adaptive)
            {
                double widthDiff = double.MaxValue;

                for (int i = MinItemPerline; i <= MaxItemPerline; i++)
                {
                    if (widthDiff > Math.Abs(width / i - 96))
                    {
                        widthDiff = Math.Abs(width / i - 96);
                        itemCountPerLine = i;
                    }
                }

                ItemWidth = width / itemCountPerLine;
                ListItemWidth = ItemWidth;
            }
            else
            {
                itemCountPerLine = (int)(width / ItemWidth);
                ListItemWidth = width / itemCountPerLine;
            }

            #endregion
        }

        // Obsoleted, just keep for reference
        private void ListHelperDelegation(string methodName, object[] args)
        {
            //Type type = typeof(ListHelper<>).MakeGenericType(new Type[1] { Items[0].GetType() });
            //var obj = Activator.CreateInstance(type, null);
            //MethodInfo[] mis = type.GetMethods();

            //foreach (MethodInfo mi in mis)
            //{
            //    if (mi.Name == methodName)
            //    {
            //        ParameterInfo[] pis = mi.GetParameters();
            //        if (pis.Length == args.Length)
            //        {
            //            bool allMatch = true;
            //            foreach (ParameterInfo pi in pis)
            //            {
            //                if (args.Where(arg => arg.GetType().GetInterfaces().Intersect(pi.ParameterType.GetInterfaces()).Count() > 0).Count() == 0)
            //                {
            //                    allMatch = false;
            //                    break;
            //                }
            //            }

            //            if (allMatch)
            //            {
            //                mi.Invoke(obj, args);
            //            }
            //        }
            //    }
            //}
        }

        private T GetControl<T>(string name, Control ctrl) where T : UIElement
        {
            return (T)ctrl.Template.FindName(name, ctrl);
        }

        #endregion

        #region Build-In classes (Obsoleted, just for reference)

        //private class ListHelper<T>
        //{
        //    public void SortListItems(int idxOfDropOnItem, IList list, MetroFluidView mfv)
        //    {
        //        if (idxOfDropOnItem > -1)
        //        {
        //            int digit = (int)Math.Pow(10, list.Count.ToString().Length);
        //            double incremental = 1.0 / digit;
        //            double baseIndex = idxOfDropOnItem;

        //            foreach (T item in list)
        //            {
        //                if (item is FrameworkElement)
        //                {
        //                    baseIndex += incremental;
        //                    (item as FrameworkElement).Tag = baseIndex;
        //                }
        //            }
        //        }

        //        mfv.Items.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Ascending));
        //        mfv.Items.Refresh();
        //        mfv.SelectedItems.Clear();
        //    }

        //    public void ResizeListItems(MetroFluidView mfv, int MaxItemCount, int MinItemCount)
        //    {
        //        FrameworkElement vi = null;
        //        double itemSize;

        //        #region Calculate the size of a list item

        //        double width = mfv.Width;

        //        if (double.IsNaN(width))
        //        {
        //            width = mfv.ActualWidth;
        //            if (width == 0)
        //            {
        //                width = 96 * 5 + 19 + mfv.Padding.Left * 2 + mfv.Padding.Right * 2;
        //            }
        //        }

        //        width = width - 19 - mfv.Padding.Left * 2 - mfv.Padding.Right * 2;
        //        int itemCountPerLine = MinItemCount;
        //        double widthDiff = double.MaxValue;

        //        for (int i = MinItemCount; i <= MaxItemCount; i++)
        //        {
        //            if (widthDiff > Math.Abs(width / i - 96))
        //            {
        //                widthDiff = Math.Abs(width / i - 96);
        //                itemCountPerLine = i;
        //            }
        //        }

        //        itemSize = width / itemCountPerLine;

        //        #endregion

        //        double sortIndex = 1.0, delta;
        //        foreach (T item in mfv.Items)
        //        {
        //            if (item is FrameworkElement)
        //            {
        //                vi = item as FrameworkElement;

        //                delta = vi.Height - vi.Width;
        //                vi.Width = itemSize;
        //                vi.Height = itemSize + delta;
        //                vi.Tag = sortIndex++;
        //            }
        //        }
        //    }

        //    public void ResizeListItems(IList list, double itemSize)
        //    {
        //        FrameworkElement vi = null;

        //        double sortIndex = 1.0, delta;
        //        foreach (T item in list)
        //        {
        //            if (item is FrameworkElement)
        //            {
        //                vi = item as FrameworkElement;

        //                delta = vi.Height - vi.Width;
        //                delta = double.IsNaN(delta) || delta == 0 ? 24 : delta;
        //                vi.Width = itemSize;
        //                vi.Height = itemSize + delta;
        //                vi.Tag = sortIndex++;
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
