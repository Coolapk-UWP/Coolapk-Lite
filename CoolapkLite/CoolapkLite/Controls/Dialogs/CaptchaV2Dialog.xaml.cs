using CoolapkLite.Helpers;
using CoolapkLite.Models.Exceptions;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class CaptchaV2Dialog : ContentDialog
    {
        private readonly CaptchaChallenge captcha;
        private string validate;
        public string CaptchaToken => $"NEC:{captcha.CaptchaID.Substring(0, 8)}:{validate}";

        public CaptchaV2Dialog(CaptchaChallenge captcha)
        {
            this.captcha = captcha;
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
            WebView.AddWebAllowedObject("captchaId", this.captcha.CaptchaID);
            WebView.Navigate(new Uri("ms-appx-web:///Assets/WebView/CaptchaV2.html"));
        }

        private void SetWebViewHeight()
        {
            double width = WebView.ActualWidth;
            if (width > 0)
            {
                WebView.MinHeight = width * (19d / 27d);
            }
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            validate = e.Value;
            IsPrimaryButtonEnabled = true;
        }
    }
}
