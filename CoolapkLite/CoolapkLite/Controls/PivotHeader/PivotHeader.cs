using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkLite.Controls
{
    public sealed class PivotHeader : ListBox
    {
        #region Const Values

        private static readonly Vector2 c_frame1point1 = new Vector2(0.9f, 0.1f);
        private static readonly Vector2 c_frame1point2 = new Vector2(1.0f, 0.2f);
        private static readonly Vector2 c_frame2point1 = new Vector2(0.1f, 0.9f);
        private static readonly Vector2 c_frame2point2 = new Vector2(0.2f, 1f);

        #endregion Const Values

        public static readonly DependencyProperty PivotProperty = DependencyProperty.Register(
           nameof(Pivot),
           typeof(Pivot),
           typeof(PivotHeader),
           new PropertyMetadata(null, OnPivotPropertyChanged));

        public Pivot Pivot
        {
            get => (Pivot)GetValue(PivotProperty);
            set => SetValue(PivotProperty, value);
        }

        private static void OnPivotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((PivotHeader)d).SetPivot();
            }
        }

        public PivotHeader()
        {
            DefaultStyleKey = typeof(PivotHeader);
            SelectionChanged += ShyHeader_SelectionChanged;
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
            else if (!(ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded") && Pivot.IsLoaded))
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

        private void ShyHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode != SelectionMode.Single) { return; }
            if (m_activeIndicator is null && e.RemovedItems.Count == 1)
            { m_activeIndicator = FindSelectionIndicator(e.RemovedItems.FirstOrDefault()); }
            try { AnimateSelectionChangedToItem(e.AddedItems.FirstOrDefault()); } catch { }
        }

        private void AnimateSelectionChangedToItem(object selectedItem)
        {
            if (selectedItem != null)
            {
                AnimateSelectionChanged(selectedItem);
            }
        }

        // Please clear the field m_lastSelectedItemPendingAnimationInTopNav when calling this method to prevent garbage value and incorrect animation
        // when the layout is invalidated as it's called in OnLayoutUpdated.
        private void AnimateSelectionChanged(object nextItem)
        {
            // If we are delaying animation due to item movement in top nav overflow, don't do anything
            if (m_lastSelectedItemPendingAnimationInTopNav != null)
            {
                return;
            }

            UIElement prevIndicator = m_activeIndicator;
            UIElement nextIndicator = FindSelectionIndicator(nextItem);

            bool haveValidAnimation = false;
            // It's possible that AnimateSelectionChanged is called multiple times before the first animation is complete.
            // To have better user experience, if the selected target is the same, keep the first animation
            // If the selected target is not the same, abort the first animation and launch another animation.
            if (m_prevIndicator != null || m_nextIndicator != null) // There is ongoing animation
            {
                if (nextIndicator != null && m_nextIndicator == nextIndicator) // animate to the same target, just wait for animation complete
                {
                    if (prevIndicator != null && prevIndicator != m_prevIndicator)
                    {
                        ResetElementAnimationProperties(prevIndicator, 0.0f);
                    }
                    haveValidAnimation = true;
                }
                else
                {
                    // If the last animation is still playing, force it to complete.
                    OnAnimationComplete(null, null);
                }
            }

            if (!haveValidAnimation)
            {
                UIElement paneContentGrid = this;

                if ((prevIndicator != nextIndicator) && paneContentGrid != null && prevIndicator != null && nextIndicator != null)
                {
                    // Make sure both indicators are visible and in their original locations
                    ResetElementAnimationProperties(prevIndicator, 1.0f);
                    ResetElementAnimationProperties(nextIndicator, 1.0f);

                    // get the item positions in the pane
                    Point point = new Point(0, 0);
                    double prevPos;
                    double nextPos;

                    Point prevPosPoint = prevIndicator.TransformToVisual(paneContentGrid).TransformPoint(point);
                    Point nextPosPoint = nextIndicator.TransformToVisual(paneContentGrid).TransformPoint(point);
                    Size prevSize = prevIndicator.RenderSize;
                    Size nextSize = nextIndicator.RenderSize;

                    bool areElementsAtSameDepth = false;
                    if (IsTopNavigationView)
                    {
                        prevPos = prevPosPoint.X;
                        nextPos = nextPosPoint.X;
                        areElementsAtSameDepth = prevPosPoint.Y == nextPosPoint.Y;
                    }
                    else
                    {
                        prevPos = prevPosPoint.Y;
                        nextPos = nextPosPoint.Y;
                        areElementsAtSameDepth = prevPosPoint.X == nextPosPoint.X;
                    }

                    Visual visual = ElementCompositionPreview.GetElementVisual(this);
                    CompositionScopedBatch scopedBatch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                    if (!areElementsAtSameDepth)
                    {
                        bool isNextBelow = prevPosPoint.Y < nextPosPoint.Y;
                        if (prevIndicator.RenderSize.Height > prevIndicator.RenderSize.Width)
                        {
                            PlayIndicatorNonSameLevelAnimations(prevIndicator, true, isNextBelow ? false : true);
                        }
                        else
                        {
                            PlayIndicatorNonSameLevelTopPrimaryAnimation(prevIndicator, true);
                        }

                        if (nextIndicator.RenderSize.Height > nextIndicator.RenderSize.Width)
                        {
                            PlayIndicatorNonSameLevelAnimations(nextIndicator, false, isNextBelow ? true : false);
                        }
                        else
                        {
                            PlayIndicatorNonSameLevelTopPrimaryAnimation(nextIndicator, false);
                        }

                    }
                    else
                    {
                        double outgoingEndPosition = nextPos - prevPos;
                        double incomingStartPosition = prevPos - nextPos;

                        // Play the animation on both the previous and next indicators
                        PlayIndicatorAnimations(prevIndicator,
                            0,
                            outgoingEndPosition,
                            prevSize,
                            nextSize,
                            true);
                        PlayIndicatorAnimations(nextIndicator,
                            incomingStartPosition,
                            0,
                            prevSize,
                            nextSize,
                            false);
                    }

                    scopedBatch.End();
                    m_prevIndicator = prevIndicator;
                    m_nextIndicator = nextIndicator;

                    scopedBatch.Completed +=
                        (object sender, CompositionBatchCompletedEventArgs args) =>
                        {
                            OnAnimationComplete(sender, args);
                        };
                }
                else
                {
                    // if all else fails, or if animations are turned off, attempt to correctly set the positions and opacities of the indicators.
                    ResetElementAnimationProperties(prevIndicator, 0.0f);
                    ResetElementAnimationProperties(nextIndicator, 1.0f);
                }

                m_activeIndicator = nextIndicator;
            }
        }

        private void PlayIndicatorNonSameLevelAnimations(UIElement indicator, bool isOutgoing, bool fromTop)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(indicator);
            Compositor comp = visual.Compositor;

            // Determine scaling of indicator (whether it is appearing or disappearing)
            float beginScale = isOutgoing ? 1.0f : 0.0f;
            float endScale = isOutgoing ? 0.0f : 1.0f;
            ScalarKeyFrameAnimation scaleAnim = comp.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, beginScale);
            scaleAnim.InsertKeyFrame(1.0f, endScale);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(600);

            // Determine where the indicator is animating from/to
            Size size = indicator.RenderSize;
            double dimension = IsTopNavigationView ? size.Width : size.Height;
            float newCenter = fromTop ? 0.0f : (float)dimension;
            Vector3 indicatorCenterPoint = visual.CenterPoint;
            indicatorCenterPoint.Y = newCenter;
            visual.CenterPoint = indicatorCenterPoint;

            visual.StartAnimation("Scale.Y", scaleAnim);
        }

        private void PlayIndicatorNonSameLevelTopPrimaryAnimation(UIElement indicator, bool isOutgoing)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(indicator);
            Compositor comp = visual.Compositor;

            // Determine scaling of indicator (whether it is appearing or disappearing)
            float beginScale = isOutgoing ? 1.0f : 0.0f;
            float endScale = isOutgoing ? 0.0f : 1.0f;
            ScalarKeyFrameAnimation scaleAnim = comp.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, beginScale);
            scaleAnim.InsertKeyFrame(1.0f, endScale);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(600);

            // Determine where the indicator is animating from/to
            Size size = indicator.RenderSize;
            double newCenter = size.Width / 2;
            Vector3 indicatorCenterPoint = visual.CenterPoint;
            indicatorCenterPoint.Y = (float)newCenter;
            visual.CenterPoint = indicatorCenterPoint;

            visual.StartAnimation("Scale.X", scaleAnim);
        }

        private void PlayIndicatorAnimations(UIElement indicator, double from, double to, Size beginSize, Size endSize, bool isOutgoing)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(indicator);
            Compositor comp = visual.Compositor;

            Size size = indicator.RenderSize;
            double dimension = IsTopNavigationView ? size.Width : size.Height;

            double beginScale = 1.0;
            double endScale = 1.0;
            if (IsTopNavigationView && size.Width > 0.001)
            {
                beginScale = beginSize.Width / size.Width;
                endScale = endSize.Width / size.Width;
            }

            StepEasingFunction singleStep = comp.CreateStepEasingFunction();
            singleStep.IsFinalStepSingleFrame = true;

            if (isOutgoing)
            {
                // fade the outgoing indicator so it looks nice when animating over the scroll area
                ScalarKeyFrameAnimation opacityAnim = comp.CreateScalarKeyFrameAnimation();
                opacityAnim.InsertKeyFrame(0.0f, 1.0f);
                opacityAnim.InsertKeyFrame(0.333f, 1.0f, singleStep);
                opacityAnim.InsertKeyFrame(1.0f, 0.0f, comp.CreateCubicBezierEasingFunction(c_frame2point1, c_frame2point2));
                opacityAnim.Duration = TimeSpan.FromMilliseconds(600);

                visual.StartAnimation("Opacity", opacityAnim);
            }

            ScalarKeyFrameAnimation posAnim = comp.CreateScalarKeyFrameAnimation();
            posAnim.InsertKeyFrame(0.0f, (float)(from < to ? from : (from + (dimension * (beginScale - 1)))));
            posAnim.InsertKeyFrame(0.333f, (float)(from < to ? (to + (dimension * (endScale - 1))) : to), singleStep);
            posAnim.Duration = TimeSpan.FromMilliseconds(600);

            ScalarKeyFrameAnimation scaleAnim = comp.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, (float)beginScale);
            scaleAnim.InsertKeyFrame(0.333f, (float)(Math.Abs(to - from) / dimension + (from < to ? endScale : beginScale)), comp.CreateCubicBezierEasingFunction(c_frame1point1, c_frame1point2));
            scaleAnim.InsertKeyFrame(1.0f, (float)endScale, comp.CreateCubicBezierEasingFunction(c_frame2point1, c_frame2point2));
            scaleAnim.Duration = TimeSpan.FromMilliseconds(600);

            ScalarKeyFrameAnimation centerAnim = comp.CreateScalarKeyFrameAnimation();
            centerAnim.InsertKeyFrame(0.0f, from < to ? 0.0f : (float)dimension);
            centerAnim.InsertKeyFrame(1.0f, from < to ? (float)dimension : 0.0f, singleStep);
            centerAnim.Duration = TimeSpan.FromMilliseconds(200);

            if (IsTopNavigationView)
            {
                visual.StartAnimation("Offset.X", posAnim);
                visual.StartAnimation("Scale.X", scaleAnim);
                visual.StartAnimation("CenterPoint.X", centerAnim);
            }
            else
            {
                visual.StartAnimation("Offset.Y", posAnim);
                visual.StartAnimation("Scale.Y", scaleAnim);
                visual.StartAnimation("CenterPoint.Y", centerAnim);
            }
        }

        private void OnAnimationComplete(object sender, CompositionBatchCompletedEventArgs args)
        {
            UIElement indicator = m_prevIndicator;
            ResetElementAnimationProperties(indicator, 0.0f);
            m_prevIndicator = null;

            indicator = m_nextIndicator;
            ResetElementAnimationProperties(indicator, 1.0f);
            m_nextIndicator = null;
        }

        private void ResetElementAnimationProperties(UIElement element, float desiredOpacity)
        {
            if (element != null)
            {
                element.Opacity = desiredOpacity;
                if (ElementCompositionPreview.GetElementVisual(element) is Visual visual)
                {
                    visual.Offset = new Vector3(0.0f, 0.0f, 0.0f);
                    visual.Scale = new Vector3(1.0f, 1.0f, 1.0f);
                    visual.Opacity = desiredOpacity;
                }
            }
        }

        private UIElement FindSelectionIndicator(object item)
        {
            if (item != null)
            {
                if (ContainerFromItem(item) is FrameworkElement container)
                {
                    if (container.FindDescendant(c_selectionIndicatorName) is UIElement indicator)
                    {
                        return indicator;
                    }
                    else
                    {
                        // Indicator was not found, so maybe the layout hasn't updated yet.
                        // So let's do that now.
                        container.UpdateLayout();
                        return container.FindDescendant(c_selectionIndicatorName);
                    }
                }
            }
            return null;
        }

        // Indicator animations
        private UIElement m_prevIndicator;
        private UIElement m_nextIndicator;
        private UIElement m_activeIndicator;
        private readonly object m_lastSelectedItemPendingAnimationInTopNav;

        private readonly bool IsTopNavigationView = true;
        private const string c_selectionIndicatorName = "SelectionIndicator";
    }
}
