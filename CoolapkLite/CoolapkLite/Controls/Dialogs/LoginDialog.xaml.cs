using CoolapkLite.Helpers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http.Filters;
using Windows.Web.Http;

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
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "CloseButtonText"))
            {
                CloseButtonText = ResourceLoader.GetForViewIndependentUse().GetString("Cancel");
            }
            else
            {
                SecondaryButtonText = ResourceLoader.GetForViewIndependentUse().GetString("Cancel");
            }
            CheckText();
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(UID) && !string.IsNullOrWhiteSpace(UserName))
                {
                    GetText(UserName);
                }
                else if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(UID))
                {
                    GetText(UID);
                }
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = UID;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => CheckText();

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckText();

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private void GetText(string name)
        {
            (string UID, string UserName, string UserAvatar) results = UIHelper.AwaitByTaskCompleteSource(() => NetworkHelper.GetUserInfoByNameAsync(name));
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
