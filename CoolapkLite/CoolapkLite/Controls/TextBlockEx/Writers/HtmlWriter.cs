using HtmlAgilityPack;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.Controls.Writers
{
    public abstract class HtmlWriter
    {
        public abstract string[] TargetTags { get; }

        public abstract DependencyObject GetControl(HtmlNode fragment);

        public virtual bool Match(HtmlNode fragment)
        {
            return fragment != null && fragment.NodeType == HtmlNodeType.Element && !string.IsNullOrEmpty(fragment.Name) && TargetTags.Any(t => t.Equals(fragment.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        protected static Binding CreateBinding(object source, string path)
        {
            return new Binding
            {
                Path = new PropertyPath(path),
                Source = source
            };
        }
    }
}
