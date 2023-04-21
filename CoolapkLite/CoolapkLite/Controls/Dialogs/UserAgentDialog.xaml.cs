using CoolapkLite.Helpers;
using CoolapkLite.Models.Update;
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
