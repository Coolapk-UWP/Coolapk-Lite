using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Pages
{
    public interface IHaveTitleBar
    {
        Frame MainFrame { get; }
        CoreDispatcher Dispatcher { get; }
        Task ShowProgressBarAsync();
        Task HideProgressBarAsync();
        Task ErrorProgressBarAsync();
        Task PausedProgressBarAsync();
        Task ShowProgressBarAsync(double value);
        Task ShowMessageAsync(string message = null);
    }
}
