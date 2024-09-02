using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Helpers
{
    public static class WebViewHelper
    {
        #region IsEnable

        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached(
                "IsEnable",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsEnableChanged));

        public static bool GetIsEnable(WebView element)
        {
            return (bool)element.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(WebView element, bool value)
        {
            element.SetValue(IsEnableProperty, value);
        }

        private static void OnIsEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WebView element)) { return; }
            if (e.NewValue is true)
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

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached(
                "Margin",
                typeof(Thickness),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnMarginChanged));

        public static Thickness GetMargin(WebView element)
        {
            return (Thickness)element.GetValue(MarginProperty);
        }

        public static void SetMargin(WebView element, Thickness value)
        {
            element.SetValue(MarginProperty, value);
        }

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WebView element)) { return; }
            if (ApiInfoHelper.IsFrameworkElementIsLoadedSupported && element.IsLoaded)
            {
                UpdateMargin(element, (Thickness)e.NewValue);
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

        public static readonly DependencyProperty IsVerticalStretchProperty =
            DependencyProperty.RegisterAttached(
                "IsVerticalStretch",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsVerticalStretchChanged));

        public static bool GetIsVerticalStretch(WebView element)
        {
            return (bool)element.GetValue(IsVerticalStretchProperty);
        }

        public static void SetIsVerticalStretch(WebView element, bool value)
        {
            element.SetValue(IsVerticalStretchProperty, value);
        }

        private static void OnIsVerticalStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WebView element)) { return; }
            if (ApiInfoHelper.IsFrameworkElementIsLoadedSupported && element.IsLoaded)
            {
                UpdateIsVerticalStretch(element, e.NewValue is true);
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

        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached(
                "HTML",
                typeof(string),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnHTMLChanged));

        public static string GetHTML(WebView element)
        {
            return (string)element.GetValue(HTMLProperty);
        }

        public static void SetHTML(WebView element, string value)
        {
            element.SetValue(HTMLProperty, value);
        }

        private static async void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string html = e.NewValue?.ToString() ?? "<div/>";
            if (!(d is WebView element)) { return; }
            if (!ApiInfoHelper.IsFrameworkElementIsLoadedSupported)
            {
                element.NavigateToString(html);
            }
            await element.ResumeOnLoadedAsync(() => false);
            element.NavigateToString(html);
        }

        #endregion
    }
}
