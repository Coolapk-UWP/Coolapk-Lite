﻿using CoolapkLite.Common;
using CoolapkLite.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Helpers
{
    public static class ListViewHelper
    {
        #region Padding

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.RegisterAttached(
                "Padding",
                typeof(Thickness),
                typeof(ListViewHelper),
                new PropertyMetadata(null, OnPaddingChanged));

        public static Thickness GetPadding(ListViewBase element)
        {
            return (Thickness)element.GetValue(PaddingProperty);
        }

        public static void SetPadding(ListViewBase element, Thickness value)
        {
            element.SetValue(PaddingProperty, value);
        }

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListViewBase element = (ListViewBase)d;
            if (ApiInfoHelper.IsFrameworkElementIsLoadedSupported)
            {
                if (element.IsLoaded)
                {
                    UpdatePadding(element, (Thickness)e.NewValue);
                }
                else
                {
                    element.Loaded -= OnListViewLoaded;
                    element.Loaded += OnListViewLoaded;
                }
            }
            else
            {
                if (element.FindDescendant<ScrollViewer>() != null)
                {
                    UpdatePadding(element, (Thickness)e.NewValue);
                }
                else
                {
                    element.Loaded -= OnListViewLoaded;
                    element.Loaded += OnListViewLoaded;
                }
            }
            element.RegisterPropertyChangedCallback(ItemsControl.ItemsPanelProperty, OnItemsPanelChanged);
        }

        private static async void OnItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
        {
            ListViewBase element = (ListViewBase)sender;
            if (element.FindDescendant<ScrollViewer>() is ScrollViewer scrollViewer
                && scrollViewer?.FindDescendant<ItemsPresenter>() is ItemsPresenter itemsPresenter)
            {
                await ThreadSwitcher.ResumeBackgroundAsync();
                do
                {
                    await Task.Delay(100);
                }
                while (await itemsPresenter.Dispatcher.AwaitableRunAsync(() => itemsPresenter?.FindDescendant<Panel>() == null));
                await itemsPresenter.Dispatcher.ResumeForegroundAsync();
                UpdatePadding(element, GetPadding(element));
            }
            else
            {
                element.Loaded -= OnListViewLoaded;
                element.Loaded += OnListViewLoaded;
            }
        }

        private static void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            ListViewBase element = (ListViewBase)sender;
            Thickness padding = GetPadding(element);
            UpdatePadding(element, padding);
            element.Loaded -= OnListViewLoaded;
        }

        public static void UpdatePadding(this ListViewBase element, Thickness padding)
        {
            if (element?.FindDescendant<ScrollViewer>() is ScrollViewer scrollViewer
                && scrollViewer?.FindDescendant<ItemsPresenter>()?.FindDescendant<Panel>() is Panel panel)
            {
                Thickness margin = new Thickness(-padding.Left, -padding.Top, -padding.Right, -padding.Bottom);

                panel.Margin = padding;

                scrollViewer.Margin = padding;
                scrollViewer.Padding = margin;
            }
        }

        #endregion

        #region EnableIncrementalLoading

        public static readonly DependencyProperty EnableIncrementalLoadingProperty =
            DependencyProperty.RegisterAttached(
                "EnableIncrementalLoading",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(false, OnEnableIncrementalLoadingChanged));

        public static bool GetEnableIncrementalLoading(ListViewBase element)
        {
            return (bool)element.GetValue(EnableIncrementalLoadingProperty);
        }

        public static void SetEnableIncrementalLoading(ListViewBase element, bool value)
        {
            element.SetValue(EnableIncrementalLoadingProperty, value);
        }

        private static void OnEnableIncrementalLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListViewBase element = (ListViewBase)d;
            if ((bool)e.NewValue)
            {
                if (ApiInfoHelper.IsFrameworkElementIsLoadedSupported)
                {
                    if (element.IsLoaded)
                    {
                        InstallIncrementalLoadingWorkaround(element, null);
                    }
                    else
                    {
                        element.Loaded -= InstallIncrementalLoadingWorkaround;
                        element.Loaded += InstallIncrementalLoadingWorkaround;
                    }
                }
                else
                {
                    if (element.FindDescendant<ScrollViewer>() != null)
                    {
                        InstallIncrementalLoadingWorkaround(element, null);
                    }
                    else
                    {
                        element.Loaded -= InstallIncrementalLoadingWorkaround;
                        element.Loaded += InstallIncrementalLoadingWorkaround;
                    }
                }
            }
        }

        private static void InstallIncrementalLoadingWorkaround(object sender, RoutedEventArgs args)
        {
            ListViewBase element = (ListViewBase)sender;
            ScrollViewer scrollViewer = element.FindDescendant<ScrollViewer>();

            scrollViewer.SizeChanged -= ScrollViewer_SizeChanged;
            scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            scrollViewer.SizeChanged += ScrollViewer_SizeChanged;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            void ScrollViewer_SizeChanged(object _sender, SizeChangedEventArgs e)
            {
                LoadMoreItems(element, scrollViewer);
            }

            void ScrollViewer_ViewChanged(object _sender, ScrollViewerViewChangedEventArgs e)
            {
                LoadMoreItems(element, scrollViewer);
            }

            async void LoadMoreItems(ListViewBase listViewBase, ScrollViewer _scrollViewer)
            {
                const double loadingThreshold = 0.5;
                if (!(listViewBase.ItemsSource is ISupportIncrementalLoading source)) { return; }
                check:
                if (listViewBase.Items.Count > 0 && !source.HasMoreItems) { return; }
                if (GetIsIncrementallyLoading(listViewBase)) { return; }
                if (((_scrollViewer.ExtentHeight - _scrollViewer.VerticalOffset) / _scrollViewer.ViewportHeight) - 1.0 <= loadingThreshold)
                {
                    try
                    {
                        SetIsIncrementallyLoading(listViewBase, true);
                        await source.LoadMoreItemsAsync(20);
                        goto check;
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.GetLogger(nameof(ShyHeaderListView)).Error(ex.ExceptionToMessage(), ex);
                    }
                    finally
                    {
                        SetIsIncrementallyLoading(listViewBase, false);
                    }
                }
            }
        }

        #endregion

        #region IsIncrementallyLoading

        private static readonly DependencyProperty IsIncrementallyLoadingProperty =
            DependencyProperty.RegisterAttached(
                "IsIncrementallyLoading",
                typeof(bool),
                typeof(ListViewHelper),
                new PropertyMetadata(false));

        private static bool GetIsIncrementallyLoading(ListViewBase element)
        {
            return (bool)element.GetValue(IsIncrementallyLoadingProperty);
        }

        private static void SetIsIncrementallyLoading(ListViewBase element, bool value)
        {
            element.SetValue(IsIncrementallyLoadingProperty, value);
        }

        #endregion
    }
}
