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
