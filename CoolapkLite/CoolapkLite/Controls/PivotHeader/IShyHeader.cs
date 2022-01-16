using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    internal interface IShyHeader
    {
        int ShyHeaderSelectedIndex { get; set; }
        object ShyHeaderSelectedItem { get; set; }

        event SelectionChangedEventHandler ShyHeaderSelectionChanged;
    }
}
