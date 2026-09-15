using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    public interface IDispatcherNotifyPropertyChanged : INotifyPropertyChanged
    {
        CoreDispatcher Dispatcher { get; }
    }

    public interface IViewModel : IDispatcherNotifyPropertyChanged
    {
        Task Refresh(bool reset);
    }

    public interface IListViewModel<T> : IViewModel, IList<T>, IList, INotifyCollectionChanged
    {
    }
}
