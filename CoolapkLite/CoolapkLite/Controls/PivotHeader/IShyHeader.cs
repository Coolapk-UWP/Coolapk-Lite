using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    public interface IShyHeader
    {
        int ShyHeaderSelectedIndex { get; set; }
        ShyHeaderItem ShyHeaderSelectedItem { get; set; }

        event SelectionChangedEventHandler ShyHeaderSelectionChanged;
    }
}
