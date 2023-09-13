using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels
{
    public interface IComboBoxChangeSelectedIndex
    {
        List<string> ItemSource { get; }
        int ComboBoxSelectedIndex { get; }
        void SetComboBoxSelectedIndex(int value);
    }

    public interface IToggleChangeSelectedIndex
    {
        bool ToggleIsOn { get; }
    }

    public interface IViewModel : INotifyPropertyChanged
    {
        string Title { get; }
        CoreDispatcher Dispatcher { get; }
        Task Refresh(bool reset);
        bool IsEqual(IViewModel other);
    }
}
