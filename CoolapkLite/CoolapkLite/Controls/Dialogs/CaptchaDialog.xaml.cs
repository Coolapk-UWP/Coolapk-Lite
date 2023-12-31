using CoolapkLite.Helpers;
using CoolapkLite.Models.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class CaptchaDialog : ContentDialog
    {
        private static readonly double currentDpi = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

        public string Code { get; private set; }

        public CaptchaDialog()
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

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            _ = Refresh();
            if (e != null) { e.Handled = true; }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e) => _ = Refresh();

        public async Task<bool> RequestValidateAsync()
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent type = new StringContent(CoolapkMessageException.RequestCaptcha))
            using (StringContent code = new StringContent(Code))
            {
                content.Add(type, "type");
                content.Add(code, "code");
                (bool isSucceed, JToken result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.RequestValidate), content);
                if (result.Type == JTokenType.String) { _ = this.ShowMessageAsync(result.ToString()); }
                return isSucceed;
            }
        }

        private async Task Refresh()
        {
            Image.Source = null;
            long timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Image.Source = await RequestHelper.GetImageAsync(UriHelper.GetUri(UriType.GetCaptchaImage, timeSpan, GetActualPixel(Image.Width), GetActualPixel(Image.Height)));
        }

        private static double GetActualPixel(double pixel) => pixel * currentDpi;
    }
}
