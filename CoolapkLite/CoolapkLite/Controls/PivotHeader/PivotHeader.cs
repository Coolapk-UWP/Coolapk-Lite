using CoolapkLite.Common;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    public class PivotHeader : ListBox
    {
        protected AnimateSelectionProvider SelectionProvider { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PivotHeader"/> class.
        /// </summary>
        public PivotHeader()
        {
            DefaultStyleKey = typeof(PivotHeader);
            SelectionProvider = new AnimateSelectionProvider
            {
                Orientation = Orientation.Horizontal,
                IndicatorName = c_selectionIndicatorName,
                ItemsControls = new[] { this }
            };
        }

        #region Pivot

        public static readonly DependencyProperty PivotProperty =
            DependencyProperty.Register(
                nameof(Pivot),
                typeof(Pivot),
                typeof(PivotHeader),
                new PropertyMetadata(null, OnPivotPropertyChanged));

        public Pivot Pivot
        {
            get => (Pivot)GetValue(PivotProperty);
            set => SetValue(PivotProperty, value);
        }

        #endregion

        private static void OnPivotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((PivotHeader)d).SetPivot();
            }
        }

        private void SetPivot()
        {
            if (Pivot == null) { return; }
            SetBinding(SelectedIndexProperty, new Binding()
            {
                Source = Pivot,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(nameof(SelectedIndex))
            });
            ItemCollection items = Pivot.Items;
            ItemsSource = Pivot.Items.Select((x) => x is PivotItem item ? item.Header : x).ToList();
            HidePivotHeader();
        }

        private void HidePivotHeader()
        {
            PivotPanel PivotPanel = Pivot?.FindDescendant<PivotPanel>();
            if (PivotPanel != null)
            {
                Grid PivotLayoutElement = PivotPanel?.FindDescendant<Grid>();
                if (PivotLayoutElement != null)
                {
                    PivotLayoutElement.RowDefinitions.First().Height = new GridLength(0, GridUnitType.Pixel);
                }
            }
            else if (!(ApiInfoHelper.IsFrameworkElementIsLoadedSupported && Pivot.IsLoaded))
            {
                Pivot.Loaded += (sender, args) =>
                {
                    PivotPanel = Pivot?.FindDescendant<PivotPanel>();
                    if (PivotPanel != null)
                    {
                        Grid PivotLayoutElement = PivotPanel?.FindDescendant<Grid>();
                        if (PivotLayoutElement != null)
                        {
                            PivotLayoutElement.RowDefinitions.First().Height = new GridLength(0, GridUnitType.Pixel);
                        }
                    }
                };
            }
        }

        private const string c_selectionIndicatorName = "SelectionIndicator";
    }
}
