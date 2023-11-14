using CoolapkLite.Helpers;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class BookmarkDialog : ContentDialog
    {
        public string BookmarkURL { get; private set; }
        public string BookmarkTitle { get; private set; }

        public BookmarkDialog()
        {
            InitializeComponent();
            if (ApiInfoHelper.IsDefaultButtonSupported)
            {
                DefaultButton = ContentDialogButton.Primary;
            }
            if (ApiInfoHelper.IsCloseButtonTextSupported)
            {
                CloseButtonText = ResourceLoader.GetForViewIndependentUse().GetString("Cancel");
            }
            else
            {
                SecondaryButtonText = ResourceLoader.GetForViewIndependentUse().GetString("Cancel");
            }
        }
    }
}
