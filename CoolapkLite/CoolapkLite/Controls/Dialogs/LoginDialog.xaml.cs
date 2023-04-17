using CoolapkLite.Helpers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
        private string UserName = SettingsHelper.Get<string>(SettingsHelper.UserName);
        private string Token = SettingsHelper.Get<string>(SettingsHelper.Token);

        public LoginDialog()
        {
            InitializeComponent();
            Closing += OnClosing;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
            {
                DefaultButton = ContentDialogButton.Primary;
            }
            CheckText();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                args.Cancel = true;
                CheckLogin().Wait();
            }

            async Task CheckLogin()
            {
                UIHelper.ShowProgressBar();
                ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
                if (string.IsNullOrWhiteSpace(UID) && !string.IsNullOrWhiteSpace(UserName))
                {
                    await GetText(UserName);
                }
                else if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UID))
                {
                    await GetText(UID);
                }
                if (await SettingsHelper.Login(UID, UserName, Token))
                {
                    UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
                    args.Cancel = false;
                }
                else
                {
                    UIHelper.ShowMessage(loader.GetString("LoginFailed"));
                }
                UIHelper.HideProgressBar();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckText();

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckText();

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private async Task GetText(string name)
        {
            (string UID, string UserName, string UserAvatar) results = await NetworkHelper.GetUserInfoByNameAsync(name);
            if (!string.IsNullOrWhiteSpace(results.UID))
            {
                UID = results.UID;
            }
            if (!string.IsNullOrWhiteSpace(results.UserName))
            {
                UserName = results.UserName;
            }
        }
    }
}
