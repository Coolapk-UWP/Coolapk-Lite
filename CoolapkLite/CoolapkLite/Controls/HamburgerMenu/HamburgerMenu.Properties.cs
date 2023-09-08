// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenu is based on a SplitView control. By default it contains a HamburgerButton and a ListView to display menu items.
    /// </summary>
    public partial class HamburgerMenu
    {
        #region OpenPaneLength

        /// <summary>
        /// Identifies the <see cref="OpenPaneLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(
                nameof(OpenPaneLength),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(320.0));

        /// <summary>
        /// Gets or sets the width of the pane when it's fully expanded.
        /// </summary>
        public double OpenPaneLength
        {
            get => (double)GetValue(OpenPaneLengthProperty);
            set => SetValue(OpenPaneLengthProperty, value);
        }

        #endregion

        #region PanePlacement

        /// <summary>
        /// Identifies the <see cref="PanePlacement"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register(
                nameof(PanePlacement),
                typeof(SplitViewPanePlacement),
                typeof(HamburgerMenu),
                new PropertyMetadata(SplitViewPanePlacement.Left));

        /// <summary>
        /// Gets or sets a value that specifies whether the pane is shown on the right or left side of the control.
        /// </summary>
        public SplitViewPanePlacement PanePlacement
        {
            get => (SplitViewPanePlacement)GetValue(PanePlacementProperty);
            set => SetValue(PanePlacementProperty, value);
        }

        #endregion

        #region DisplayMode

        /// <summary>
        /// Identifies the <see cref="DisplayMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(SplitViewDisplayMode),
                typeof(HamburgerMenu),
                new PropertyMetadata(SplitViewDisplayMode.CompactInline, OnDisplayModeChanged));

        /// <summary>
        /// Gets or sets a value that specifies how the pane and content areas are shown.
        /// </summary>
        public SplitViewDisplayMode DisplayMode
        {
            get => (SplitViewDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenu)d).UpdateDisplayModeState();
        }

        #endregion

        #region CompactPaneLength

        /// <summary>
        /// Identifies the <see cref="CompactPaneLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CompactPaneLengthProperty =
            DependencyProperty.Register(
                nameof(CompactPaneLength),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(48.0));

        /// <summary>
        /// Gets or sets the width of the pane in its compact display mode.
        /// </summary>
        public double CompactPaneLength
        {
            get => (double)GetValue(CompactPaneLengthProperty);
            set => SetValue(CompactPaneLengthProperty, value);
        }

        #endregion

        #region PaneForeground

        /// <summary>
        /// Identifies the <see cref="PaneForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneForegroundProperty =
            DependencyProperty.Register(
                nameof(PaneForeground),
                typeof(Brush),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Brush to apply to the foreground of the Pane area of the control
        /// (specifically, the hamburger button foreground).
        /// </summary>
        public Brush PaneForeground
        {
            get => (Brush)GetValue(PaneForegroundProperty);
            set => SetValue(PaneForegroundProperty, value);
        }

        #endregion

        #region PaneBackground

        /// <summary>
        /// Identifies the <see cref="PaneBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneBackgroundProperty =
            DependencyProperty.Register(
                nameof(PaneBackground),
                typeof(Brush),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Brush to apply to the background of the Pane area of the control.
        /// </summary>
        public Brush PaneBackground
        {
            get => (Brush)GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        #endregion

        #region PaneOverlayBackground

        /// <summary>
        /// Identifies the <see cref="PaneOverlayBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaneOverlayBackgroundProperty =
            DependencyProperty.Register(
                nameof(PaneOverlayBackground),
                typeof(Brush),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Brush to apply to the background of the Pane area of the control when overlay.
        /// </summary>
        public Brush PaneOverlayBackground
        {
            get => (Brush)GetValue(PaneOverlayBackgroundProperty);
            set => SetValue(PaneOverlayBackgroundProperty, value);
        }

        #endregion

        #region IsPaneOpen

        /// <summary>
        /// Identifies the <see cref="IsPaneOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(
                nameof(IsPaneOpen),
                typeof(bool),
                typeof(HamburgerMenu),
                new PropertyMetadata(false, OnIsPaneOpenChanged));

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value that specifies whether the pane is expanded to its full width.
        /// </summary>
        public bool IsPaneOpen
        {
            get => (bool)GetValue(IsPaneOpenProperty);
            set => SetValue(IsPaneOpenProperty, value);
        }

        private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenu)d).UpdatePaneState();
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(object),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets an object source used to generate the content of the menu.
        /// </summary>
        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        #endregion

        #region ItemTemplate

        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the DataTemplate used to display each item.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        #endregion

        #region ItemTemplateSelector

        /// <summary>
        /// Identifies the <see cref="ItemTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            DependencyProperty.Register(
                nameof(ItemTemplateSelector),
                typeof(DataTemplateSelector),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the DataTemplateSelector used to display each item.
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
            set => SetValue(ItemTemplateSelectorProperty, value);
        }

        #endregion

        #region SelectedItem

        /// <summary>
        /// Identifies the <see cref="SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(HamburgerMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected menu item.
        /// </summary>
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        #endregion

        #region SelectedIndex

        /// <summary>
        /// Identifies the <see cref="SelectedIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedIndex),
                typeof(int),
                typeof(HamburgerMenu),
                new PropertyMetadata(-1));

        /// <summary>
        /// Gets or sets the selected menu index.
        /// </summary>
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        #endregion

        #region AutoSuggestBox

        /// <summary>
        /// Identifies the <see cref="AutoSuggestBox"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoSuggestBoxProperty =
            DependencyProperty.Register(
                nameof(AutoSuggestBox),
                typeof(AutoSuggestBox),
                typeof(HamburgerMenu),
                new PropertyMetadata(null, OnAutoSuggestBoxChanged));

        /// <summary>
        /// Gets or sets an AutoSuggestBox to be displayed in the HamburgerMenu.
        /// </summary>
        public AutoSuggestBox AutoSuggestBox
        {
            get => (AutoSuggestBox)GetValue(AutoSuggestBoxProperty);
            set => SetValue(AutoSuggestBoxProperty, value);
        }

        private static void OnAutoSuggestBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HamburgerMenu)d).UpdateAutoSuggestBoxState();
        }

        #endregion

        #region CompactModeThresholdWidth

        /// <summary>
        /// Identifies the <see cref="CompactModeThresholdWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CompactModeThresholdWidthProperty =
            DependencyProperty.Register(
                nameof(CompactModeThresholdWidth),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(641.0));

        /// <summary>
        /// Gets or sets the minimum window width at which the HamburgerMenu enters Compact display mode.
        /// </summary>
        public double CompactModeThresholdWidth
        {
            get => (double)GetValue(CompactModeThresholdWidthProperty);
            set => SetValue(CompactModeThresholdWidthProperty, value);
        }

        #endregion

        #region ExpandedModeThresholdWidth

        /// <summary>
        /// Identifies the <see cref="ExpandedModeThresholdWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandedModeThresholdWidthProperty =
            DependencyProperty.Register(
                nameof(ExpandedModeThresholdWidth),
                typeof(double),
                typeof(HamburgerMenu),
                new PropertyMetadata(1008.0));

        /// <summary>
        /// Gets or sets the minimum window width at which the HamburgerMenu enters Compact display mode.
        /// </summary>
        public double ExpandedModeThresholdWidth
        {
            get => (double)GetValue(ExpandedModeThresholdWidthProperty);
            set => SetValue(ExpandedModeThresholdWidthProperty, value);
        }

        #endregion

        /// <summary>
        /// Gets the collection used to generate the content of the items list.
        /// </summary>
        /// <exception cref="Exception">
        /// Exception thrown if ButtonsListView is not yet defined.
        /// </exception>
        public ItemCollection Items => _buttonsListView == null
                    ? throw new Exception("ButtonsListView is not defined yet. Please use ItemsSource instead.")
                    : _buttonsListView.Items;
    }
}
