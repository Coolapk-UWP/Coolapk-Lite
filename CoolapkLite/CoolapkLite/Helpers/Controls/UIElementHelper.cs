using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace CoolapkLite.Helpers
{
    public static class UIElementHelper
    {
        #region ContextFlyout

        /// <summary>
        /// Gets the flyout associated with this element.
        /// </summary>
        /// <param name="element">The flyout associated with this element.</param>
        /// <returns>
        /// The flyout associated with this element, if any; otherwise, <see langword="null"/>. The default is <see langword="null"/>.
        /// </returns>
        public static FlyoutBase GetContextFlyout(UIElement element)
        {
            return (FlyoutBase)element.GetValue(ContextFlyoutProperty);
        }

        /// <summary>
        /// Sets the flyout associated with this element.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The flyout associated with this element.</param>
        public static void SetContextFlyout(UIElement element, FlyoutBase value)
        {
            element.SetValue(ContextFlyoutProperty, value);
        }

        /// <summary>
        /// Identifies the ContextFlyout dependency property.
        /// </summary>
        public static readonly DependencyProperty ContextFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "ContextFlyout",
                typeof(FlyoutBase),
                typeof(UIElementHelper),
                new PropertyMetadata(null, OnContextFlyoutChanged));

        private static void OnContextFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = (UIElement)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "ContextFlyout"))
            {
                element.ContextFlyout = GetContextFlyout(element);
            }
            else if (element is FrameworkElement frameworkElement)
            {
                FlyoutBase.SetAttachedFlyout(frameworkElement, e.NewValue as FlyoutBase);

                element.KeyDown -= OnKeyDown;
                element.Holding -= OnHolding;
                element.RightTapped -= OnRightTapped;

                if (element != null)
                {
                    element.KeyDown += OnKeyDown;
                    element.Holding += OnHolding;
                    element.RightTapped += OnRightTapped;
                }
            }
        }

        private static void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Menu)
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
        }

        private static void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout is MenuFlyout menu)
            {
                menu.ShowAt(element, e.GetPosition(element));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
        }

        private static void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout is MenuFlyout menu)
            {
                menu.ShowAt(element, e.GetPosition(element));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
        }

        #endregion

        #region AllowFocusOnInteraction

        /// <summary>
        /// Gets a value that indicates whether the element automatically gets focus when the user interacts with it.
        /// </summary>
        /// <param name="element">The flyout associated with this element.</param>
        /// <returns>
        /// A value that indicates whether the element automatically gets focus when the user interacts with it.
        /// </returns>
        public static bool GetAllowFocusOnInteraction(FrameworkElement element)
        {
            return (bool)element.GetValue(AllowFocusOnInteractionProperty);
        }

        /// <summary>
        /// Sets a value that indicates whether the element automatically gets focus when the user interacts with it.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">A value that indicates whether the element automatically gets focus when the user interacts with it.</param>
        public static void SetAllowFocusOnInteraction(FrameworkElement element, bool value)
        {
            element.SetValue(AllowFocusOnInteractionProperty, value);
        }

        /// <summary>
        /// Identifies the AllowFocusOnInteraction dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowFocusOnInteractionProperty =
            DependencyProperty.RegisterAttached(
                "AllowFocusOnInteraction",
                typeof(bool),
                typeof(UIElementHelper),
                new PropertyMetadata(false, OnAllowFocusOnInteractionChanged));

        private static void OnAllowFocusOnInteractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "AllowFocusOnInteraction"))
            {
                element.AllowFocusOnInteraction = GetAllowFocusOnInteraction(element);
            }
        }

        #endregion
    }
}
