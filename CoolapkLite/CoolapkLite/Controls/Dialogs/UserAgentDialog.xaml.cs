using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class UserAgentDialog : ContentDialog
    {
        internal UserAgent UserAgent { get; set; }

        public UserAgentDialog(string line)
        {
            InitializeComponent();
            UserAgent = UserAgent.Parse(line);
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

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                SettingsHelper.Set(SettingsHelper.CustomUA, UserAgent);
                NetworkHelper.SetRequestHeaders();
            }
        }
    }
}
