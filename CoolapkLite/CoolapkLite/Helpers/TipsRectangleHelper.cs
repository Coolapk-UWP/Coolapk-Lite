using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    public class TipsRectangleHelper : DependencyObject
    {
        private static Dictionary<string, TipsRectangleServiceItem> TokenRectangles = new Dictionary<string, TipsRectangleServiceItem>();
        private static Collection<WeakReference<Selector>> Selectors = new Collection<WeakReference<Selector>>();

        public static string GetToken(FrameworkElement obj)
        {
            return (string)obj.GetValue(TokenProperty);
        }

        public static void SetToken(FrameworkElement obj, string value)
        {
            obj.SetValue(TokenProperty, value);
        }

        public static readonly DependencyProperty TokenProperty =
            DependencyProperty.RegisterAttached("Token", typeof(string), typeof(TipsRectangleHelper), new PropertyMetadata(null));

        public static TipsRectangleServiceStates GetState(FrameworkElement obj)
        {
            return (TipsRectangleServiceStates)obj.GetValue(StateProperty);
        }

        public static void SetState(FrameworkElement obj, TipsRectangleServiceStates value)
        {
            obj.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached("State", typeof(TipsRectangleServiceStates), typeof(TipsRectangleHelper), new PropertyMetadata(TipsRectangleServiceStates.None, StatePropertyChanged));

        public static string GetTipTargetName(DependencyObject obj)
        {
            return (string)obj.GetValue(TipTargetNameProperty);
        }

        public static void SetTipTargetName(DependencyObject obj, string value)
        {
            obj.SetValue(TipTargetNameProperty, value);
        }

        public static readonly DependencyProperty TipTargetNameProperty =
            DependencyProperty.RegisterAttached("TipTargetName", typeof(string), typeof(TipsRectangleHelper), new PropertyMetadata(null, TipTargetNamePropertyChanged));

        private static void StatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue != (int)e.OldValue && e.NewValue != null)
            {
                var state = (TipsRectangleServiceStates)e.NewValue;
                if (d is FrameworkElement ele)
                {
                    var token = GetToken(ele);
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        switch (state)
                        {
                            case TipsRectangleServiceStates.None:
                                {
                                    if (TokenRectangles.ContainsKey(token)) TokenRectangles.Remove(token);
                                }
                                break;
                            case TipsRectangleServiceStates.From:
                                {
                                    if (!TokenRectangles.ContainsKey(token))
                                    {
                                        TokenRectangles[token] = new TipsRectangleServiceItem(ele, null);
                                    }
                                    else if (TokenRectangles[token].TargetItem != null)
                                    {
                                        TryStartAnimation(token, ele, TokenRectangles[token].TargetItem);
                                        TokenRectangles.Remove(token);
                                    }
                                }
                                break;
                            case TipsRectangleServiceStates.To:
                                {
                                    if (!TokenRectangles.ContainsKey(token))
                                    {
                                        TokenRectangles[token] = new TipsRectangleServiceItem(null, ele);
                                    }
                                    else if (TokenRectangles[token].SourceItem != null)
                                    {
                                        TryStartAnimation(token, TokenRectangles[token].SourceItem, ele);
                                        TokenRectangles.Remove(token);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void TipTargetNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue is string TargetName)
            {
                if (d is Selector selector)
                {
                    bool IsIn = false;
                    WeakReference<Selector> weak_tmp = null;
                    foreach (var item in Selectors)
                    {
                        if (item.TryGetTarget(out Selector tmp))
                        {
                            if (tmp == selector)
                            {
                                IsIn = true;
                                weak_tmp = item;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(TargetName))
                    {
                        if (IsIn)
                        {
                            Selectors.Remove(weak_tmp);
                            selector.SelectionChanged -= Selector_SelectionChanged;
                        }
                    }
                    else
                    {
                        if (!IsIn)
                        {
                            if (selector is ListViewBase || selector is ListBox)
                            {
                                weak_tmp = new WeakReference<Selector>(selector);
                                Selectors.Add(weak_tmp);
                                selector.SelectionChanged += Selector_SelectionChanged;
                            }
                        }
                    }

                }
            }
        }

        private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = (Selector)sender;
            if (selector is ListViewBase listView)
            {
                if (listView.SelectionMode == ListViewSelectionMode.None || listView.SelectionMode == ListViewSelectionMode.Multiple) return;
            }
            else if (selector is ListBox listBox)
            {
                if (listBox.SelectionMode == SelectionMode.Multiple) return;
            }

            var name = GetTipTargetName(selector);
            var token = GetToken(selector);

            DependencyObject SourceItemContainer = null;
            DependencyObject TargetItemContainer = null;
            FrameworkElement SourceItemTips = null;
            FrameworkElement TargetItemTips = null;

            if (e.AddedItems.Count == 1 && e.RemovedItems.Count == 1)
            {
                var targetItem = e.AddedItems.FirstOrDefault();
                TargetItemContainer = selector.ContainerFromItem(targetItem);
                TargetItemTips = TargetItemContainer?.FindDescendantByName(name);

                var sourceItem = e.RemovedItems.FirstOrDefault();
                SourceItemContainer = selector.ContainerFromItem(sourceItem);
                SourceItemTips = SourceItemContainer?.FindDescendantByName(name);
            }
            if (SourceItemTips != null && TargetItemTips != null)
            {
                if (string.IsNullOrWhiteSpace(token)) token = selector.GetHashCode().ToString();
                TryStartAnimation(token, SourceItemTips, TargetItemTips);
            }

        }

        private static void TryStartAnimation(string token, FrameworkElement source, FrameworkElement target)
        {
            try
            {
                if (source.ActualHeight > 0 && source.ActualWidth > 0)
                {
                    var service = ConnectedAnimationService.GetForCurrentView();
                    if (source != target)
                    {
                        service.GetAnimation(token)?.Cancel();
                        service.DefaultDuration = TimeSpan.FromSeconds(0.33d);
                        var animation = service.PrepareToAnimate(token, source);
                        animation.TryStart(target);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    internal class TipsRectangleServiceItem
    {
        public FrameworkElement SourceItem { get; set; }
        public FrameworkElement TargetItem { get; set; }
        public TipsRectangleServiceItem(FrameworkElement SourceItem, FrameworkElement TargetItem)
        {
            this.SourceItem = SourceItem;
            this.TargetItem = TargetItem;
        }
    }

    public enum TipsRectangleServiceStates
    {
        None, From, To
    }
}
