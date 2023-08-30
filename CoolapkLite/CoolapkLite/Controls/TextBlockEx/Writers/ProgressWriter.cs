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
    public class ProgressWriter : HtmlWriter
    {
        public override string[] TargetTags => new string[] { "progress", "meter" };

        public override DependencyObject GetControl(HtmlNode fragment, TextBlockEx textBlockEx)
        {
            ProgressBar progressBar = new ProgressBar
            {
                MinWidth = 150
            };
            HtmlNode node = fragment;
            if (node != null)
            {
                double max = GetAttributeValue(node, "max", 1);
                double min = GetAttributeValue(node, "min", 0);
                double value = GetAttributeValue(node, "value", 0);
                progressBar.Maximum = max;
                progressBar.Minimum = min;
                progressBar.Value = value;
            }
            return progressBar;
        }
    }
}
