using CoolapkLite.Helpers;
using CoolapkLite.Models.Users;
using System;
using System.Net;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string _UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
        public string UID
        {
            get => _UID;
            private set
            {
                if (_UID != value)
                {
                    _UID = value;
                    CheckText();
                }
            }
        }

        private string username = SettingsHelper.Get<string>(SettingsHelper.UserName);
        public string UserName
        {
            get => username;
            private set
            {
                if (username != value)
                {
                    username = value;
                    CheckText();
                }
            }
        }

        private string token = SettingsHelper.Get<string>(SettingsHelper.Token);
        public string Token
        {
            get => token;
            private set
            {
                if (token != value)
                {
                    token = value;
                    CheckText();
                }
            }
        }

        public LoginDialog()
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

        private void CheckText() => IsPrimaryButtonEnabled = !string.IsNullOrEmpty(Token) && (!string.IsNullOrEmpty(UID) || !string.IsNullOrEmpty(UserName));

        private void GetText(string name)
        {
            if (NetworkHelper.GetUserInfoByNameAsync(name).AwaitByTaskCompleteSource() is UserInfoModel results)
            {
                UID = results.UID.ToString();
                if (!string.IsNullOrWhiteSpace(results.UserName))
                {
                    UserName = WebUtility.UrlEncode(results.UserName);
                }
            }
        }
    }
}
