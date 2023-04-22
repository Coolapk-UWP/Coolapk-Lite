using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class BookmarkDialog : ContentDialog
    {
        public string BookmarkURL { get; set; }
        public string BookmarkTitle { get; set; }

        public BookmarkDialog()
        {
            InitializeComponent();
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
            {
                DefaultButton = ContentDialogButton.Primary;
            }
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "CloseButtonText"))
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
