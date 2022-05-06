// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Controls
{
    /// <summary>
    /// The HamburgerMenu is based on a SplitView control. By default it contains a HamburgerButton and a ListView to display menu items.
    /// </summary>
    [TemplatePart(Name = "HamburgerButton", Type = typeof(Button))]
    [TemplatePart(Name = "ButtonsListView", Type = typeof(Windows.UI.Xaml.Controls.ListViewBase))]
    [TemplatePart(Name = "OptionsListView", Type = typeof(Windows.UI.Xaml.Controls.ListViewBase))]
    public partial class HamburgerMenu : ContentControl
    {
        private Button _hamburgerButton;
        private Windows.UI.Xaml.Controls.ListViewBase _buttonsListView;
        private Windows.UI.Xaml.Controls.ListViewBase _optionsListView;

        private ControlTemplate _previousTemplateUsed;
        private object _navigationView;
        private object _settingsObject;

        /// <summary>
        /// Gets a value indicating whether <see cref="NavigationView"/> is supported
        /// </summary>
        public static bool IsNavigationViewSupported { get; } = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.NavigationView");

        /// <summary>
        /// Initializes a new instance of the <see cref="HamburgerMenu"/> class.
        /// </summary>
        public HamburgerMenu()
        {
            DefaultStyleKey = typeof(HamburgerMenu);
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
                _buttonsListView.ItemClick -= ButtonsListView_ItemClick;
            }

            if (_optionsListView != null)
            {
                _optionsListView.ItemClick -= OptionsListView_ItemClick;
            }

            _hamburgerButton = (Button)GetTemplateChild("HamburgerButton");
            _buttonsListView = (Windows.UI.Xaml.Controls.ListViewBase)GetTemplateChild("ButtonsListView");
            _optionsListView = (Windows.UI.Xaml.Controls.ListViewBase)GetTemplateChild("OptionsListView");

            if (_hamburgerButton != null)
            {
                _hamburgerButton.Click += HamburgerButton_Click;
            }

            if (_buttonsListView != null)
            {
                _buttonsListView.ItemClick += ButtonsListView_ItemClick;
            }

            if (_optionsListView != null)
            {
                _optionsListView.ItemClick += OptionsListView_ItemClick;
            }

            base.OnApplyTemplate();
        }

        private bool IsSettingsItem(object menuItem)
        {
            return menuItem.GetType()
                           .GetProperties()
                           .Any(p => p.GetValue(menuItem).ToString().IndexOf("setting", StringComparison.OrdinalIgnoreCase) >= 0
                                  || p.GetValue(menuItem).ToString() == ((char)Symbol.Setting).ToString());
        }
    }
}
