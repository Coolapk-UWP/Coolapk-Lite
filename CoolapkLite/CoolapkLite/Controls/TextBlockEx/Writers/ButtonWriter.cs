using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace CoolapkLite.Controls.Writers
{
    public class ButtonWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "button" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            Button button = new Button();
            HtmlNode node = fragment;
            if (node.GetAttributeValue("name", null) is string name)
            {
                button.Name = name;
            }
            if (node.Attributes["disabled"] != null)
            {
                button.IsEnabled = false;
            }
            return button;
        }
    }
}
