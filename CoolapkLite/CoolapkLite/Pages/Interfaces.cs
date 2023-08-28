using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Pages
{
    public interface IHaveTitleBar
    {
        void ShowProgressBar();
        void HideProgressBar();
        void ErrorProgressBar();
        void PausedProgressBar();
        void ShowProgressBar(double value);
        void ShowMessage(string message = null);
        CoreDispatcher Dispatcher { get; }
        Frame MainFrame { get; }
    }
}
