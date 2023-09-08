using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "PivotHeader", Type = typeof(PivotHeader))]
    public class ShyHeaderPivotListView : ShyHeaderListView, IShyHeader
    {
        private PivotHeader _pivotHeader;

        #region LeftHeader

        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.Register(
                nameof(LeftHeader),
                typeof(object),
                typeof(ShyHeaderPivotListView),
                null);

        public object LeftHeader
        {
            get => GetValue(LeftHeaderProperty);
            set => SetValue(LeftHeaderProperty, value);
        }

        #endregion

        #region RightHeader

        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.Register(
                nameof(RightHeader),
                typeof(object),
                typeof(ShyHeaderPivotListView),
                null);

        public object RightHeader
        {
            get => GetValue(RightHeaderProperty);
            set => SetValue(RightHeaderProperty, value);
        }

        #endregion

        #region ShyHeaderItemSource

        public static readonly DependencyProperty ShyHeaderItemSourceProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderItemSource),
                typeof(IEnumerable<ShyHeaderItem>),
                typeof(ShyHeaderPivotListView),
                new PropertyMetadata(null, OnShyHeaderItemSourcePropertyChanged));

        public IEnumerable<ShyHeaderItem> ShyHeaderItemSource
        {
            get => (IEnumerable<ShyHeaderItem>)GetValue(ShyHeaderItemSourceProperty);
            set => SetValue(ShyHeaderItemSourceProperty, value);
        }

        #endregion

        #region ShyHeaderSelectedIndex

        public static readonly DependencyProperty ShyHeaderSelectedIndexProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderSelectedIndex),
                typeof(int),
                typeof(ShyHeaderPivotListView),
                new PropertyMetadata(-1, OnShyHeaderSelectedIndexPropertyChanged));

        public int ShyHeaderSelectedIndex
        {
            get => (int)GetValue(ShyHeaderSelectedIndexProperty);
            set => SetValue(ShyHeaderSelectedIndexProperty, value);
        }

        #endregion

        #region ShyHeaderSelectedItem

        public static readonly DependencyProperty ShyHeaderSelectedItemProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderSelectedItem),
                typeof(ShyHeaderItem),
                typeof(ShyHeaderPivotListView),
                null);

        public ShyHeaderItem ShyHeaderSelectedItem
        {
            get => (ShyHeaderItem)GetValue(ShyHeaderSelectedItemProperty);
            set => SetValue(ShyHeaderSelectedItemProperty, value);
        }

        #endregion

        public event SelectionChangedEventHandler ShyHeaderSelectionChanged;

        private static void OnShyHeaderItemSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as ShyHeaderPivotListView).UpdateShyHeaderItem(e.NewValue as IList<ShyHeaderItem>);
            }
        }

        private static void OnShyHeaderSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as ShyHeaderPivotListView).UpdateShyHeaderSelectedIndex(e.NewValue as int?);
            }
        }

        public ShyHeaderPivotListView() => DefaultStyleKey = typeof(ShyHeaderPivotListView);

        protected override void OnApplyTemplate()
        {
            if (_pivotHeader != null)
            {
                _pivotHeader.SelectionChanged -= PivotHeader_SelectionChanged;
            }

            _pivotHeader = (PivotHeader)GetTemplateChild("PivotHeader");

            if (_pivotHeader != null)
            {
                if (_pivotHeader.Items.Any())
                {
                    _pivotHeader.SelectedIndex = 0;
                }
                _pivotHeader.SelectionChanged += PivotHeader_SelectionChanged;
                if (ShyHeaderItemSource != null)
                {
                    UpdateShyHeaderItem();
                }
            }

            base.OnApplyTemplate();
        }

        private void PivotHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShyHeaderSelectedIndex = (sender as PivotHeader).SelectedIndex;
            IList<object> AddedItems = (from item in ShyHeaderItemSource
                                        where e.AddedItems.Contains(item.Header)
                                        select (object)item).ToList();
            IList<object> RemovedItems = (from item in ShyHeaderItemSource
                                          where e.RemovedItems.Contains(item.Header)
                                          select (object)item).ToList();
            ShyHeaderSelectionChanged?.Invoke(this, new SelectionChangedEventArgs(RemovedItems, AddedItems));
        }

        private void UpdateShyHeaderItem(IEnumerable<ShyHeaderItem> items = null)
        {
            items = items ?? ShyHeaderItemSource;
            if (items == null) { return; }
            if (_pivotHeader != null)
            {
                _pivotHeader.ItemsSource = (from item in items
                                            select item?.Header ?? string.Empty).ToArray();
            }
            if (_pivotHeader?.SelectedIndex == -1)
            {
                try { _pivotHeader.SelectedIndex = 0; } catch { }
            }
        }

        private void UpdateShyHeaderSelectedIndex(int? index = null)
        {
            index = index ?? SelectedIndex;
            if (index == -1) { return; }
            ShyHeaderSelectedItem = ShyHeaderItemSource.ElementAt(index.Value);
            ItemsSource = ShyHeaderSelectedItem.ItemSource;
        }
    }

    public class ShyHeaderItem : DependencyObject
    {
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register(
                nameof(Tag),
                typeof(object),
                typeof(ShyHeaderItem),
                null);

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(ShyHeaderItem),
                null);

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(object),
                typeof(ShyHeaderItem),
                null);

        public object Tag
        {
            get => GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object ItemSource
        {
            get => GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }
    }
}
