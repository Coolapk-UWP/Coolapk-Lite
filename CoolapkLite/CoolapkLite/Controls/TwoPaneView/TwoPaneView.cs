using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = "PART_ColumnLeft", Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = "PART_ColumnMiddle", Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = "PART_ColumnRight", Type = typeof(ColumnDefinition))]
    [TemplatePart(Name = "PART_RowTop", Type = typeof(RowDefinition))]
    [TemplatePart(Name = "PART_RowMiddle", Type = typeof(RowDefinition))]
    [TemplatePart(Name = "PART_RowBottom", Type = typeof(RowDefinition))]
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

        public TwoPaneView()
        {
            DefaultStyleKey = typeof(TwoPaneView);

            SizeChanged -= OnSizeChanged;
            SizeChanged += OnSizeChanged;
            Window.Current.SizeChanged -= OnWindowSizeChanged;
            Window.Current.SizeChanged += OnWindowSizeChanged;
        }

        /// <summary>
        /// Override default OnApplyTemplate to capture children controls
        /// </summary>
        protected override void OnApplyTemplate()
        {
            m_loaded = true;

            m_columnLeft = (ColumnDefinition)GetTemplateChild("PART_ColumnLeft");
            m_columnMiddle = (ColumnDefinition)GetTemplateChild("PART_ColumnMiddle");
            m_columnRight = (ColumnDefinition)GetTemplateChild("PART_ColumnRight");
            m_rowTop = (RowDefinition)GetTemplateChild("PART_RowTop");
            m_rowMiddle = (RowDefinition)GetTemplateChild("PART_RowMiddle");
            m_rowBottom = (RowDefinition)GetTemplateChild("PART_RowBottom");

            base.OnApplyTemplate();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMode();
        }

        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateMode();
        }

        public void UpdateMode()
        {
            // Don't bother running this logic until after we hit OnApplyTemplate.
            if (!m_loaded)
            {
                return;
            }

            double controlWidth = ActualWidth;
            double controlHeight = ActualHeight;

            ViewMode newMode = (PanePriority == TwoPaneViewPriority.Pane1) ? ViewMode.Pane1Only : ViewMode.Pane2Only;

            // One region
            if (controlWidth > MinWideModeWidth && WideModeConfiguration != TwoPaneViewWideModeConfiguration.SinglePane)
            {
                // Split horizontally
                newMode = (WideModeConfiguration == TwoPaneViewWideModeConfiguration.LeftRight) ? ViewMode.LeftRight : ViewMode.RightLeft;
            }
            else if (controlHeight > MinTallModeHeight && TallModeConfiguration != TwoPaneViewTallModeConfiguration.SinglePane)
            {
                // Split vertically
                newMode = (TallModeConfiguration == TwoPaneViewTallModeConfiguration.TopBottom) ? ViewMode.TopBottom : ViewMode.BottomTop;
            }

            // Update row/column sizes (this may need to happen even if the mode doesn't change)
            UpdateRowsColumns(newMode);

            // Update mode if necessary
            if (newMode != m_currentMode)
            {
                m_currentMode = newMode;

                TwoPaneViewMode newViewMode = TwoPaneViewMode.SinglePane;

                switch (m_currentMode)
                {
                    case ViewMode.Pane1Only:
                        VisualStateManager.GoToState(this, "ViewMode_OneOnly", true);
                        break;
                    case ViewMode.Pane2Only:
                        VisualStateManager.GoToState(this, "ViewMode_TwoOnly", true);
                        break;
                    case ViewMode.LeftRight:
                        VisualStateManager.GoToState(this, "ViewMode_LeftRight", true);
                        newViewMode = TwoPaneViewMode.Wide;
                        break;
                    case ViewMode.RightLeft:
                        VisualStateManager.GoToState(this, "ViewMode_RightLeft", true);
                        newViewMode = TwoPaneViewMode.Wide;
                        break;
                    case ViewMode.TopBottom:
                        VisualStateManager.GoToState(this, "ViewMode_TopBottom", true);
                        newViewMode = TwoPaneViewMode.Tall;
                        break;
                    case ViewMode.BottomTop:
                        VisualStateManager.GoToState(this, "ViewMode_BottomTop", true);
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

        public void UpdateRowsColumns(ViewMode newMode)
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
        }
    }
}
