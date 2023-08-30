using CoolapkLite.Helpers;
using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Xml.Linq;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CoolapkLite.Controls.Writers
{
    internal class InputWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "input" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            HtmlNode node = fragment;
            if (node != null)
            {
                string type = node.GetAttributeValue("type", string.Empty);

                FrameworkElement element;
                switch (type)
                {
                    case "button":
                    case "reset":
                    case "submit":
                        element = CreateButton(node);
                        break;
                    case "checkbox":
                        element = CreateCheckBox(node);
                        break;
                    case "color":
                        element = CreateColorPicker(node);
                        break;
                    case "date":
                        element = CreateCalendarDatePicker();
                        break;
                    case "datetime":
                    case "month":
                        element = CreateDatePicker(type);
                        break;
                    case "datetime-local":
                        element = CreateDateTimePicker();
                        break;
                    case "file":
                        element = CreateButton(node);
                        (element as Button).Content = "选取文件";
                        break;
                    case "hidden":
                        element = null;
                        break;
                    case "image":
                        element = CreateButton(node);
                        (element as Button).Content = "选取图片";
                        break;
                    case "password":
                        element = CreatePasswordBox(node);
                        break;
                    case "radio":
                        element = CreateRadioButton(node);
                        break;
                    case "range":
                        element = CreateSlider(node);
                        break;
                    case "time":
                        element = CreateTimePicker();
                        break;
                    case "email":
                    case "number":
                    case "search":
                    case "tel":
                    case "text":
                    case "url":
                    default:
                        element = CreateTextBox(node);
                        break;
                }

                if (node.GetAttributeValue("name", null) is string name)
                {
                    element.Name = name;
                }

                if (node.Attributes["disabled"] != null && element is Control control)
                {
                    control.IsEnabled = false;
                }

                double height = GetAttributeValue(node, "height", 0);
                double width = GetAttributeValue(node, "width", 0);

                if (height > 0)
                {
                    element.Height = height;
                }
                if (width > 0)
                {
                    element.Width = width;
                }

                return element;
            }
            return null;
        }

        private FrameworkElement CreateButton(HtmlNode node)
        {
            string value = node.GetAttributeValue("value", string.Empty);
            Button button = new Button { Content = value };
            return button;
        }

        private FrameworkElement CreateCheckBox(HtmlNode node)
        {
            CheckBox checkBox = new CheckBox();
            if (node.Attributes["checked"] != null)
            {
                checkBox.IsChecked = true;
            }
            return checkBox;
        }

        private FrameworkElement CreateColorPicker(HtmlNode node)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.ColorPicker"))
            {
                string value = node.GetAttributeValue("value", "#0078D7");
                
                Color color;
                try
                {
                    color = value.ToColor();
                }
                catch
                {
                    color = Colors.Blue;
                }

                ColorPicker colorPicker = new ColorPicker
                {
                    Color = color,
                    IsAlphaEnabled = true,
                    IsMoreButtonVisible = true
                };

                SolidColorBrush solidColorBrush = new SolidColorBrush();
                BindingOperations.SetBinding(solidColorBrush, SolidColorBrush.ColorProperty, CreateBinding(colorPicker, nameof(colorPicker.Color)));
                
                Border Border = new Border
                {
                    Width = 32,
                    Height = 32,
                    Background = solidColorBrush
                };

                Flyout flyout = new Flyout
                {
                    Content = colorPicker,
                    Placement = FlyoutPlacementMode.Bottom
                };
                FlyoutBaseHelper.SetShouldConstrainToRootBounds(flyout, false);

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.SplitButton"))
                {
                    SplitButton splitButton = new SplitButton
                    {
                        Flyout = flyout,
                        Content = Border,
                        Padding = new Thickness(0)
                    };
                    return splitButton;
                }
                else
                {
                    Button button = new Button
                    {
                        Flyout = flyout,
                        Content = Border,
                        Padding = new Thickness(0)
                    };
                    return button;
                }
            }
            return null;
        }

        private FrameworkElement CreateCalendarDatePicker()
        {
            CalendarDatePicker calendarDatePicker = new CalendarDatePicker();
            return calendarDatePicker;
        }

        private FrameworkElement CreateDatePicker(string type)
        {
            DatePicker datePicker = new DatePicker();
            switch(type)
            {
                case "month":
                    datePicker.DayVisible = false;
                    break;
            }
            return datePicker;
        }

        private FrameworkElement CreateTimePicker()
        {
            TimePicker timePicker = new TimePicker();
            return timePicker;
        }

        private FrameworkElement CreateDateTimePicker()
        {
            StackPanelEx stackPanel = new StackPanelEx
            {
                Spacing = 4,
                Orientation = Orientation.Horizontal
            };
            DatePicker datePicker = new DatePicker();
            TimePicker timePicker = new TimePicker();
            stackPanel.Children.Add(datePicker);
            stackPanel.Children.Add(timePicker);
            return stackPanel;
        }

        private FrameworkElement CreatePasswordBox(HtmlNode node)
        {
            string value = node.GetAttributeValue("value", string.Empty);
            string placeholder = node.GetAttributeValue("placeholder", string.Empty);

            PasswordBox passwordBox = new PasswordBox
            {
                Password = value,
                PlaceholderText = placeholder
            };

            return passwordBox;
        }

        private FrameworkElement CreateRadioButton(HtmlNode node)
        {
            RadioButton radioButton = new RadioButton();
            if (node.Attributes["checked"] != null)
            {
                radioButton.IsChecked = true;
            }
            return radioButton;
        }

        private FrameworkElement CreateSlider(HtmlNode node)
        {
            double max = GetAttributeValue(node, "max", 100);
            double min = GetAttributeValue(node, "min", 0);
            double value = GetAttributeValue(node, "value", 0);
            Slider slider = new Slider
            {
                Value = value,
                Maximum = max,
                Minimum = min,
                MinWidth = 150
            };
            return slider;
        }

        private FrameworkElement CreateTextBox(HtmlNode node)
        {
            string value = node.GetAttributeValue("value", string.Empty);
            string placeholder = node.GetAttributeValue("placeholder", string.Empty);

            TextBox textBox = new TextBox
            {
                Text = value,
                PlaceholderText = placeholder
            };

            if (node.Attributes["readonly"] != null)
            {
                textBox.IsReadOnly = true;
            }

            return textBox;
        }
    }
}
