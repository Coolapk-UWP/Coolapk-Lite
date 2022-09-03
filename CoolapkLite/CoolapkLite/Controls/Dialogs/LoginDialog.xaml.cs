using CoolapkLite.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            PrimaryButtonClick += OnPrimaryButtonClick;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
            {
                DefaultButton = ContentDialogButton.Primary;
            }
        }

        private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            UIHelper.ShowProgressBar();
            ResourceLoader loader = ResourceLoader.GetForCurrentView("BrowserPage");
            if (SettingsHelper.LoginIn(UID, UserName, Token))
            {
                UIHelper.ShowMessage(loader.GetString("LoginSuccessfully"));
            }
            else
            {
                UIHelper.ShowMessage("LoginFailed");
            }
            UIHelper.HideProgressBar();
        }
    }
}
