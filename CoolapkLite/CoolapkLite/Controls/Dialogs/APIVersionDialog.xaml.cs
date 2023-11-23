using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class APIVersionDialog : ContentDialog
    {
        #region APIVersion

        /// <summary>
        /// Identifies the <see cref="APIVersion"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty APIVersionProperty =
            DependencyProperty.Register(
                nameof(APIVersion),
                typeof(APIVersion),
                typeof(APIVersionDialog),
                null);

        public APIVersion APIVersion
        {
            get => (APIVersion)GetValue(APIVersionProperty);
            private set => SetValue(APIVersionProperty, value);
        }

        #endregion

        public APIVersionDialog(string line)
        {
            InitializeComponent();
            APIVersion = APIVersion.Parse(line);
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
                SettingsHelper.Set(SettingsHelper.CustomAPI, APIVersion);
                NetworkHelper.SetRequestHeaders();
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            _ = this.ShowProgressBarAsync();
            try
            {
                if (await APIVersion.GetLatestAsync() is APIVersion version)
                {
                    APIVersion = version;
                }
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
        }
    }
}
