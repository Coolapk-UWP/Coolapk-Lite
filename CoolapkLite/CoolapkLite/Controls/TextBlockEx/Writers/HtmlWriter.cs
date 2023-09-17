using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace CoolapkLite.Controls.Writers
{
    public abstract class HtmlWriter
    {
        public abstract HashSet<string> TargetTags { get; }

        public abstract DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx);

        public virtual bool Match(HtmlNode fragment)
        {
            return fragment != null && fragment.NodeType == HtmlNodeType.Element && !string.IsNullOrEmpty(fragment.Name) && TargetTags.Any(t => t.Equals(fragment.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static double GetAttributeValue(HtmlNode node, string name, double def)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (!node.HasAttributes)
            {
                return def;
            }

            HtmlAttribute htmlAttribute = node.Attributes[name];
            if (htmlAttribute == null)
            {
                return def;
            }

            try
            {
                return Convert.ToDouble(htmlAttribute.Value);
            }
            catch
            {
                return def;
            }
        }

        protected static Binding CreateBinding(object source, string path, BindingMode mode = BindingMode.OneWay)
        {
            return new Binding
            {
                Path = new PropertyPath(path),
                Source = source,
                Mode = mode
            };
        }

        protected static Binding CreateBinding(object source, string path, IValueConverter converter, BindingMode mode = BindingMode.OneWay)
        {
            return new Binding
            {
                Path = new PropertyPath(path),
                Source = source,
                Converter = converter,
                Mode = mode
            };
        }

        protected static Binding CreateBinding(object source, string path, IValueConverter converter, object parameter, BindingMode mode = BindingMode.OneWay)
        {
            return new Binding
            {
                Path = new PropertyPath(path),
                Source = source,
                Converter = converter,
                ConverterParameter = parameter,
                Mode = mode
            };
        }

        protected static Run CreateSymbolRun(string text, TextBlockEx textBlockEx)
        {
            Run run = new Run { Text = text };
            BindingOperations.SetBinding(run, TextElement.FontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.SymbolFontFamily)));
            return run;
        }

        protected static FontIcon CreateFontIcon(string glyph, TextBlockEx textBlockEx)
        {
            FontIcon fontIcon = new FontIcon { Glyph = glyph };
            fontIcon.SetBinding(FontIcon.FontFamilyProperty, CreateBinding(textBlockEx, nameof(textBlockEx.SymbolFontFamily)));
            return fontIcon;
        }
    }
}
