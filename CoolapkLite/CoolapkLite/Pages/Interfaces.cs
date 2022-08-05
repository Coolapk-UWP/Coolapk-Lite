using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Pages
{
    internal interface IHaveTitleBar
    {
        void ShowProgressBar();
        void HideProgressBar();
        void ErrorProgressBar();
        void PausedProgressBar();
        void ShowProgressBar(double value);
        void ShowMessage(string message = null);
        Frame MainFrame { get; }
    }
}
