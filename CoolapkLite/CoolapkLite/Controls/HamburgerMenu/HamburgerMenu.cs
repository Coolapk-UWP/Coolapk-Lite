// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoolapkLite.Helpers;
using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenu is based on a SplitView control. By default it contains a HamburgerButton and a ListView to display menu items.
    /// </summary>
    [TemplatePart(Name = "LayoutRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "HamburgerButton", Type = typeof(Button))]
    [TemplatePart(Name = "ButtonsListView", Type = typeof(ListViewBase))]
    [TemplatePart(Name = "OptionsListView", Type = typeof(ListViewBase))]
    [TemplatePart(Name = "PaneAutoSuggestItem", Type = typeof(ListViewItem))]
    public partial class HamburgerMenu : ContentControl
    {
        private Grid _layoutRoot;
        private Button _hamburgerButton;
        private ListViewBase _buttonsListView;
        private ListViewBase _optionsListView;
        private ListViewItem _paneAutoSuggestItem;
        private VisualStateGroup _windowsSizeGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="HamburgerMenu"/> class.
        /// </summary>
        public HamburgerMenu()
        {
            DefaultStyleKey = typeof(HamburgerMenu);
            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Override default OnApplyTemplate to capture children controls
        /// </summary>
        protected override void OnApplyTemplate()
        {
            if (PaneForeground == null)
            {
                PaneForeground = Foreground;
            }

            if (_hamburgerButton != null)
            {
                _hamburgerButton.Click -= HamburgerButton_Click;
            }

            if (_buttonsListView != null)
            {
                _buttonsListView.Loading -= HamburgerListView_Loading;
                _buttonsListView.ItemClick -= ButtonsListView_ItemClick;
            }

            if (_optionsListView != null)
            {
                _optionsListView.Loading -= HamburgerListView_Loading;
                _optionsListView.ItemClick -= OptionsListView_ItemClick;
            }

            if (_paneAutoSuggestItem != null)
            {
                _paneAutoSuggestItem.Loading -= HamburgerListView_Loading;
                _paneAutoSuggestItem.Tapped -= PaneAutoSuggestButton_Tapped;
            }

            _layoutRoot = (Grid)GetTemplateChild("LayoutRoot");
            _hamburgerButton = (Button)GetTemplateChild("HamburgerButton");
            _buttonsListView = (ListViewBase)GetTemplateChild("ButtonsListView");
            _optionsListView = (ListViewBase)GetTemplateChild("OptionsListView");
            _paneAutoSuggestItem = (ListViewItem)GetTemplateChild("PaneAutoSuggestItem");
            _windowsSizeGroup = _layoutRoot.FindVisualStateGroupByName("WindowsSizeGroup");

            if (_hamburgerButton != null)
            {
                _hamburgerButton.Click += HamburgerButton_Click;
            }

            if (_buttonsListView != null)
            {
                _buttonsListView.Loading += HamburgerListView_Loading;
                _buttonsListView.ItemClick += ButtonsListView_ItemClick;
            }

            if (_optionsListView != null)
            {
                _optionsListView.Loading += HamburgerListView_Loading;
                _optionsListView.ItemClick += OptionsListView_ItemClick;
            }

            if (_paneAutoSuggestItem != null)
            {
                _paneAutoSuggestItem.Loading += HamburgerListView_Loading;
                _paneAutoSuggestItem.Tapped += PaneAutoSuggestButton_Tapped;
            }

            if (_windowsSizeGroup != null)
            {
                _windowsSizeGroup.CurrentStateChanged += WindowsSizeGroup_CurrentStateChanged;
            }

            UpdateTitleBarPadding();

            base.OnApplyTemplate();
        }

        private void WindowsSizeGroup_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState != e.OldState)
            {
                switch (e.NewState.Name)
                {
                    case "OverlaySize":
                        IsPaneOpen = false;
                        DisplayMode = SplitViewDisplayMode.Overlay;
                        break;
                    case "CompactSize":
                        IsPaneOpen = false;
                        DisplayMode = SplitViewDisplayMode.CompactOverlay;
                        break;
                    case "ExpandedSize":
                        IsPaneOpen = true;
                        DisplayMode = SplitViewDisplayMode.CompactInline;
                        break;
                }
            }
        }

        private void UpdatePaneState()
        {
            VisualStateManager.GoToState(this, DisplayMode == SplitViewDisplayMode.CompactOverlay && !IsPaneOpen ? "ClosedCompact" : "NotClosedCompact", true);
            VisualStateManager.GoToState(this, (DisplayMode == SplitViewDisplayMode.Overlay || DisplayMode == SplitViewDisplayMode.CompactOverlay) && IsPaneOpen ? "PaneOverlaying" : "PaneNotOverlaying", true);
        }

        private void UpdateTitleBarPadding()
        {
            VisualStateManager.GoToState(this, UIHelper.HasStatusBar || UIHelper.HasTitleBar ? "TitleBarVisible" : "TitleBarCollapsed", true);
        }

        private void UpdateDisplayModeState()
        {
            switch (DisplayMode)
            {
                case SplitViewDisplayMode.Overlay:
                    VisualStateManager.GoToState(this, "MinimalSize", true);
                    break;
                case SplitViewDisplayMode.CompactOverlay:
                    VisualStateManager.GoToState(this, "CompactSize", true);
                    break;
                case SplitViewDisplayMode.CompactInline:
                    VisualStateManager.GoToState(this, "ExpandedSize", true);
                    break;
                case SplitViewDisplayMode.Inline:
                    VisualStateManager.GoToState(this, "InlineSize", true);
                    break;
            }
        }

        private void UpdateAutoSuggestBoxState()
        {
            VisualStateManager.GoToState(this, AutoSuggestBox == null ? "AutoSuggestBoxCollapsed" : "AutoSuggestBoxVisible", true);
        }
    }

    public class DisplayModeToBool : IValueConverter
    {
        private static bool HasConnectedAnimation = ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation");
        private static bool HasConnectedAnimationConfiguration = ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation", "Configuration");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !HasConnectedAnimationConfiguration && value is SplitViewDisplayMode split && parameter is string mode
                ? !(split.ToString() == mode) && HasConnectedAnimation
                : (object)HasConnectedAnimation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
