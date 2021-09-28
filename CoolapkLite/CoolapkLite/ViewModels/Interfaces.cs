using System.Threading.Tasks;

namespace CoolapkLite.ViewModels
{
    internal interface ICanComboBoxChangeSelectedIndex
    {
        int ComboBoxSelectedIndex { get; }
        Task SetComboBoxSelectedIndex(int value);
    }

    internal interface IViewModel
    {
        Task Refresh(int p);
        string Title { get; }
        double[] VerticalOffsets { get; set; }
    }
}
