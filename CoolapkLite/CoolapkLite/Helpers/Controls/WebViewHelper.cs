﻿using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class WebViewHelper
    {
        #region IsEnable

        public static bool GetIsEnable(WebView element)
        {
            return (bool)element.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(WebView element, bool value)
        {
            element.SetValue(IsEnableProperty, value);
        }

        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached(
                "IsEnable",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsEnableChanged));

        private static void OnIsEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (GetIsEnable(element))
            {
                element.SizeChanged += OnSizeChanged;
                element.NavigationCompleted += OnNavigationCompleted;
            }
            else
            {
                element.SizeChanged -= OnSizeChanged;
                element.NavigationCompleted -= OnNavigationCompleted;
            }
        }

        private static void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Thickness margin = GetMargin(sender);
            if (margin != null) { UpdateMargin(sender, margin); }
            bool isVerticalStretch = GetIsVerticalStretch(sender);
            UpdateIsVerticalStretch(sender, isVerticalStretch);
        }

        #endregion

        #region Margin

        public static Thickness GetMargin(WebView element)
        {
            return (Thickness)element.GetValue(MarginProperty);
        }

        public static void SetMargin(WebView element, Thickness value)
        {
            element.SetValue(MarginProperty, value);
        }

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached(
                "Margin",
                typeof(Thickness),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnMarginChanged));

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded") && element.IsLoaded)
            {
                Thickness margin = GetMargin(element);
                UpdateMargin(element, margin);
            }
        }

        private static void UpdateMargin(WebView element, Thickness margin)
        {
            try
            {
                Thickness Margin = margin;
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginLeft = '{Margin.Left}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginTop = '{Margin.Top}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginRight = '{Margin.Right}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginBottom = '{Margin.Bottom}px'" });
            }
            catch { }
        }

        #endregion

        #region IsVerticalStretch

        public static bool GetIsVerticalStretch(WebView element)
        {
            return (bool)element.GetValue(IsVerticalStretchProperty);
        }

        public static void SetIsVerticalStretch(WebView element, bool value)
        {
            element.SetValue(IsVerticalStretchProperty, value);
        }

        public static readonly DependencyProperty IsVerticalStretchProperty =
            DependencyProperty.RegisterAttached(
                "IsVerticalStretch",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsVerticalStretchChanged));

        private static void OnIsVerticalStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded") && element.IsLoaded)
            {
                bool isVerticalStretch = GetIsVerticalStretch(element);
                UpdateIsVerticalStretch(element, isVerticalStretch);
            }
        }

        private static async void UpdateIsVerticalStretch(WebView element, bool isVerticalStretch)
        {
            if (isVerticalStretch)
            {
                try
                {
                    element.SizeChanged -= OnSizeChanged;
                    if (!double.IsNaN(element.Height))
                    {
                        element.Height = double.NaN;
                        await Task.Delay(1);
                    }
                    string heightString = await element.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
                    if (double.TryParse(heightString.Trim('"'), out double height))
                    {
                        element.SizeChanged += OnSizeChangedReset;
                        element.Height = height;
                    }
                    else
                    {
                        element.SizeChanged += OnSizeChanged;
                    }
                }
                catch
                {
                    element.SizeChanged += OnSizeChanged;
                }
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is WebView webView)) { return; }
            if (!double.IsNaN(webView.Height)) { webView.Height = double.NaN; }
            else
            {
                bool isVerticalStretch = GetIsVerticalStretch(webView);
                UpdateIsVerticalStretch(webView, isVerticalStretch);
            }
        }

        private static void OnSizeChangedReset(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            element.SizeChanged -= OnSizeChangedReset;
            element.SizeChanged += OnSizeChanged;
        }

        #endregion

        #region HTML

        public static string GetHTML(WebView element)
        {
            return (string)element.GetValue(HTMLProperty);
        }

        public static void SetHTML(WebView element, string value)
        {
            element.SetValue(HTMLProperty, value);
        }

        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached(
                "HTML",
                typeof(string),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnHTMLChanged));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "IsLoaded"))
            {
                if (element.IsLoaded)
                {
                    string html = GetHTML(element) ?? "<div/>";
                    element.NavigateToString(html);
                }
                else
                {
                    element.Loaded -= WebView_Loaded;
                    element.Loaded += WebView_Loaded;
                }
            }
            else
            {
                element.Loaded -= WebView_Loaded;
                element.Loaded += WebView_Loaded;

                string html = GetHTML(element) ?? "<div/>";
                element.NavigateToString(html);
            }
        }

        private static void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is WebView webView)) { return; }
            string html = GetHTML(webView) ?? "<div/>";
            webView.NavigateToString(html);
        }

        #endregion
    }
}
