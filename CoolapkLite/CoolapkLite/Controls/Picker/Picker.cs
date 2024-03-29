﻿using CoolapkLite.Helpers;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Controls
{
    public class Picker : ContentControl
    {
        private Popup _popup;
        private Grid _rootGrid;

        /// <summary>
        /// Creates a new instance of the <see cref="Picker"/> class.
        /// </summary>
        public Picker()
        {
            DefaultStyleKey = typeof(Picker);
            Loaded += (sender, args) =>
            {
                if (WindowHelper.IsXamlRootSupported && XamlRoot != null)
                {
                    XamlRoot.Changed -= XamlRoot_SizeChanged;
                    XamlRoot.Changed += XamlRoot_SizeChanged;
                }
                else if (Window.Current is Window window)
                {
                    window.SizeChanged -= Window_SizeChanged;
                    window.SizeChanged += Window_SizeChanged;
                }
            };
            Unloaded += (sender, args) =>
            {
                if (WindowHelper.IsXamlRootSupported && XamlRoot != null)
                {
                    XamlRoot.Changed -= XamlRoot_SizeChanged;
                }
                else if (Window.Current is Window window)
                {
                    window.SizeChanged -= Window_SizeChanged;
                }
            };
        }

        #region PopupTransitions

        public static readonly DependencyProperty PopupTransitionsProperty =
            DependencyProperty.Register(
                nameof(PopupTransitions),
                typeof(TransitionCollection),
                typeof(Picker),
                null);

        public TransitionCollection PopupTransitions
        {
            get => (TransitionCollection)GetValue(PopupTransitionsProperty);
            set => SetValue(PopupTransitionsProperty, value);
        }

        #endregion

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (_rootGrid != null)
            {
                _rootGrid.Width = e.Size.Width;
                _rootGrid.Height = e.Size.Height;
            }
        }

        private void XamlRoot_SizeChanged(XamlRoot sender, XamlRootChangedEventArgs args)
        {
            if (_rootGrid != null)
            {
                _rootGrid.Width = sender.Size.Width;
                _rootGrid.Height = sender.Size.Height;
            }
        }

        public void Show(UIElement element)
        {
            if (Parent is Grid grid)
            {
                grid.Children.Remove(this);
            }

            Size size = this.GetXAMLRootSize();

            _rootGrid = new Grid
            {
                Width = size.Width,
                Height = size.Height
            };

            _popup = new Popup
            {
                Child = _rootGrid
            };
            _rootGrid.Children.Add(this);

            _popup.SetXAMLRoot(element);

            _popup.IsOpen = true;
        }

        public void Hide()
        {
            _rootGrid?.Children.Remove(this);
            _rootGrid = null;

            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
            _popup = null;
        }
    }
}
