﻿using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = c_columnLeftName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = c_columnMiddleName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = c_columnRightName, Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = c_rowTopName, Type = typeof(RowDefinition))]
    [TemplatePart(Name = c_rowMiddleName, Type = typeof(RowDefinition))]
    [TemplatePart(Name = c_rowBottomName, Type = typeof(RowDefinition))]
    public partial class TwoPaneView : Control
    {
        private bool m_loaded = false;
        private ViewMode m_currentMode = ViewMode.None;
        private ColumnDefinition m_columnLeft = new ColumnDefinition();
        private ColumnDefinition m_columnMiddle = new ColumnDefinition();
        private ColumnDefinition m_columnRight = new ColumnDefinition();
        private RowDefinition m_rowTop = new RowDefinition();
        private RowDefinition m_rowMiddle = new RowDefinition();
        private RowDefinition m_rowBottom = new RowDefinition();

        private const string c_pane1ScrollViewerName = "PART_Pane1ScrollViewer";
        private const string c_pane2ScrollViewerName = "PART_Pane2ScrollViewer";

        private const string c_columnLeftName = "PART_ColumnLeft";
        private const string c_columnMiddleName = "PART_ColumnMiddle";
        private const string c_columnRightName = "PART_ColumnRight";
        private const string c_rowTopName = "PART_RowTop";
        private const string c_rowMiddleName = "PART_RowMiddle";
        private const string c_rowBottomName = "PART_RowBottom";

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoPaneView"/> class.
        /// </summary>
        public TwoPaneView()
        {
            DefaultStyleKey = typeof(TwoPaneView);
            SizeChanged -= OnSizeChanged;
            SizeChanged += OnSizeChanged;
            Loaded += (sender, args) =>
            {
                if (WindowHelper.IsXamlRootSupported && XamlRoot != null)
                {
                    XamlRoot.Changed -= OnXamlRootSizeChanged;
                    XamlRoot.Changed += OnXamlRootSizeChanged;
                }
                else if (Window.Current is Window window)
                {
                    window.SizeChanged -= OnWindowSizeChanged;
                    window.SizeChanged += OnWindowSizeChanged;
                }
            };
            Unloaded += (sender, args) =>
            {
                if (WindowHelper.IsXamlRootSupported && XamlRoot != null)
                {
                    XamlRoot.Changed -= OnXamlRootSizeChanged;
                }
                else if (Window.Current is Window window)
                {
                    window.SizeChanged -= OnWindowSizeChanged;
                }
            };
        }

        /// <summary>
        /// Override default OnApplyTemplate to capture children controls
        /// </summary>
        protected override void OnApplyTemplate()
        {
            m_loaded = true;

            SetScrollViewerProperties(c_pane1ScrollViewerName);
            SetScrollViewerProperties(c_pane2ScrollViewerName);

            m_columnLeft = (ColumnDefinition)GetTemplateChild(c_columnLeftName);
            m_columnMiddle = (ColumnDefinition)GetTemplateChild(c_columnMiddleName);
            m_columnRight = (ColumnDefinition)GetTemplateChild(c_columnRightName);
            m_rowTop = (RowDefinition)GetTemplateChild(c_rowTopName);
            m_rowMiddle = (RowDefinition)GetTemplateChild(c_rowMiddleName);
            m_rowBottom = (RowDefinition)GetTemplateChild(c_rowBottomName);

            base.OnApplyTemplate();
        }

        private void SetScrollViewerProperties(string scrollViewerName)
        {
            if (ApiInfoHelper.IsSizesContentToTemplatedParentSupported)
            {
                if (GetTemplateChild(scrollViewerName) is ScrollViewer scrollViewer)
                {
                    scrollViewer.Loaded += OnScrollViewerLoaded;
                }
            }
        }

        private void OnScrollViewerLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is FrameworkElement scrollViewer)
            {
                FrameworkElement scrollContentPresenterFE = scrollViewer.FindDescendant("ScrollContentPresenter");
                if (scrollContentPresenterFE != null)
                {
                    if (scrollContentPresenterFE is ScrollContentPresenter scrollContentPresenter)
                    {
                        scrollContentPresenter.SizesContentToTemplatedParent = true;
                    }
                }
            }
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateMode();
        }

        private void OnXamlRootSizeChanged(XamlRoot sender, XamlRootChangedEventArgs args)
        {
            UpdateMode();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMode();
        }

        private void UpdateMode()
        {
            // Don't bother running this logic until after we hit OnApplyTemplate.
            if (!m_loaded) { return; }

            double controlWidth = ActualWidth;
            double controlHeight = ActualHeight;

            ViewMode newMode = (PanePriority == TwoPaneViewPriority.Pane1) ? ViewMode.Pane1Only : ViewMode.Pane2Only;

            // Calculate new mode
            DisplayRegionHelperInfo info = DisplayRegionHelper.GetRegionInfo(this);
            Rect rcControl = GetControlRect();
            bool isInMultipleRegions = IsInMultipleRegions(info, rcControl);

            if (isInMultipleRegions)
            {
                if (info.Mode == TwoPaneViewMode.Wide)
                {
                    // Regions are laid out horizontally
                    if (WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
                    {
                        newMode = (WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight) ? ViewMode.LeftRight : ViewMode.RightLeft;
                    }
                }
                else if (info.Mode == TwoPaneViewMode.Tall)
                {
                    // Regions are laid out vertically
                    if (TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
                    {
                        newMode = (TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom) ? ViewMode.TopBottom : ViewMode.BottomTop;
                    }
                }
            }
            else
            {
                // One region
                if (double.IsNaN(MinWideModeWidth) && double.IsNaN(MinTallModeHeight))
                {
                    if (controlWidth > controlHeight && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
                    {
                        // Split horizontally
                        newMode = (WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight) ? ViewMode.LeftRight : ViewMode.RightLeft;
                    }
                    else if (controlHeight > controlWidth && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
                    {
                        // Split vertically
                        newMode = (TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom) ? ViewMode.TopBottom : ViewMode.BottomTop;
                    }
                }
                else if (controlWidth > MinWideModeWidth && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
                {
                    // Split horizontally
                    newMode = (WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight) ? ViewMode.LeftRight : ViewMode.RightLeft;
                }
                else if (controlHeight > MinTallModeHeight && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
                {
                    // Split vertically
                    newMode = (TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom) ? ViewMode.TopBottom : ViewMode.BottomTop;
                }
            }

            // Network row/column sizes (this may need to happen even if the mode doesn't change)
            UpdateRowsColumns(newMode, info, rcControl);

            // Network mode if necessary
            if (newMode != m_currentMode)
            {
                m_currentMode = newMode;

                TwoPaneViewMode newViewMode = TwoPaneViewMode.SinglePane;

                switch (m_currentMode)
                {
                    case ViewMode.Pane1Only:
                        _ = VisualStateManager.GoToState(this, "ViewMode_OneOnly", true);
                        break;
                    case ViewMode.Pane2Only:
                        _ = VisualStateManager.GoToState(this, "ViewMode_TwoOnly", true);
                        break;
                    case ViewMode.LeftRight:
                        _ = VisualStateManager.GoToState(this, "ViewMode_LeftRight", true);
                        newViewMode = TwoPaneViewMode.Wide;
                        break;
                    case ViewMode.RightLeft:
                        _ = VisualStateManager.GoToState(this, "ViewMode_RightLeft", true);
                        newViewMode = TwoPaneViewMode.Wide;
                        break;
                    case ViewMode.TopBottom:
                        _ = VisualStateManager.GoToState(this, "ViewMode_TopBottom", true);
                        newViewMode = TwoPaneViewMode.Tall;
                        break;
                    case ViewMode.BottomTop:
                        _ = VisualStateManager.GoToState(this, "ViewMode_BottomTop", true);
                        newViewMode = TwoPaneViewMode.Tall;
                        break;
                }

                if (newViewMode != Mode)
                {
                    Mode = newViewMode;
                    ModeChanged?.Invoke(this, this);
                }
            }
        }

        private void UpdateRowsColumns(ViewMode newMode, DisplayRegionHelperInfo info, Rect rcControl)
        {
            if (m_columnLeft != null && m_columnMiddle != null && m_columnRight != null && m_rowTop != null && m_rowMiddle != null && m_rowBottom != null)
            {
                // Reset split lengths
                m_columnMiddle.Width = new GridLength(0, GridUnitType.Pixel);
                m_rowMiddle.Height = new GridLength(0, GridUnitType.Pixel);

                // Set columns lengths
                if (newMode == ViewMode.LeftRight || newMode == ViewMode.RightLeft)
                {
                    m_columnLeft.Width = (newMode == ViewMode.LeftRight) ? Pane1Length : Pane2Length;
                    m_columnRight.Width = (newMode == ViewMode.LeftRight) ? Pane2Length : Pane1Length;
                }
                else
                {
                    m_columnLeft.Width = new GridLength(1, GridUnitType.Star);
                    m_columnRight.Width = new GridLength(0, GridUnitType.Pixel);
                }

                // Set row lengths
                if (newMode == ViewMode.TopBottom || newMode == ViewMode.BottomTop)
                {
                    m_rowTop.Height = (newMode == ViewMode.TopBottom) ? Pane1Length : Pane2Length;
                    m_rowBottom.Height = (newMode == ViewMode.TopBottom) ? Pane2Length : Pane1Length;
                }
                else
                {
                    m_rowTop.Height = new GridLength(1, GridUnitType.Star);
                    m_rowBottom.Height = new GridLength(0, GridUnitType.Pixel);
                }

                // Handle regions
                if (IsInMultipleRegions(info, rcControl) && newMode != ViewMode.Pane1Only && newMode != ViewMode.Pane2Only)
                {
                    Rect rc1 = info.Regions[0];
                    Rect rc2 = info.Regions[1];
                    Rect rcWindow = DisplayRegionHelper.WindowRect(this);

                    if (info.Mode == TwoPaneViewMode.Wide)
                    {
                        m_columnMiddle.Width = new GridLength(rc2.X - rc1.Width, GridUnitType.Pixel);

                        m_columnLeft.Width = new GridLength(rc1.Width - rcControl.X, GridUnitType.Pixel);
                        m_columnRight.Width = new GridLength(rc2.Width - (rcWindow.Width - rcControl.Width - rcControl.X), GridUnitType.Pixel);
                    }
                    else
                    {
                        m_rowMiddle.Height = new GridLength(rc2.Y - rc1.Height, GridUnitType.Pixel);

                        m_rowTop.Height = new GridLength(rc1.Height - rcControl.Y, GridUnitType.Pixel);
                        m_rowBottom.Height = new GridLength(rc2.Height - (rcWindow.Height - rcControl.Height - rcControl.Y), GridUnitType.Pixel);
                    }
                }
            }
        }

        private Rect GetControlRect()
        {
            // Find out where this control is in the window
            GeneralTransform transform = TransformToVisual(DisplayRegionHelper.WindowElement(this));
            return transform.TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));
        }

        private bool IsInMultipleRegions(DisplayRegionHelperInfo info, Rect rcControl)
        {
            bool isInMultipleRegions = false;

            if (info.Mode != TwoPaneViewMode.SinglePane)
            {
                Rect rc1 = info.Regions[0];
                Rect rc2 = info.Regions[1];

                if (info.Mode == TwoPaneViewMode.Wide)
                {
                    // Check that the control is over the split
                    if (rcControl.X < rc1.Width && rcControl.X + rcControl.Width > rc2.X)
                    {
                        isInMultipleRegions = true;
                    }
                }
                else if (info.Mode == TwoPaneViewMode.Tall)
                {
                    // Check that the control is over the split
                    if (rcControl.Y < rc1.Height && rcControl.Y + rcControl.Height > rc2.Y)
                    {
                        isInMultipleRegions = true;
                    }
                }
            }

            return isInMultipleRegions;
        }
    }
}
