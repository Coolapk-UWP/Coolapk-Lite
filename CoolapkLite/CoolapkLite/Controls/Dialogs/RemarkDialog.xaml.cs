using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Users;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkLite.Controls.Dialogs
{
    public sealed partial class RemarkDialog : ContentDialog
    {
        public string UID { get; private set; }
        public string RemarkName { get; private set; }

        public RemarkDialog() : this(null, null) { }

        public RemarkDialog(string uid) : this(uid, GetRemarkName(uid)) { }

        public RemarkDialog(string uid, string name)
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

            if (uid != null)
            {
                UIDTextBox.IsReadOnly = true;
                _ = NetworkHelper.GetUserInfoByNameAsync(uid, true).ContinueWith(x => RemarkTextBox.SetValueAsync(TextBox.PlaceholderTextProperty, x.Result?.UserName ?? name)).Unwrap();
            }
            else if (name != null)
            {
                RemarkTextBox.PlaceholderText = name;
            }

            RemarkName = name;
            UID = uid;
        }

        private static string GetRemarkName(string uid) =>
            uid != null && int.TryParse(uid, out int id) && SettingsHelper.UserRemarks?.TryGetValue(id, out string name) == true
                ? name : null;

        private async void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrEmpty(UID))
                {
                    args.Cancel = true;
                    return;
                }

                _ = this.ShowProgressBarAsync();
                if (await NetworkHelper.GetUserInfoByNameAsync(UID) is UserInfoModel results)
                {
                    string id = results.UID.ToString();
                    using (MultipartFormDataContent content = new MultipartFormDataContent())
                    {
                        using (StringContent uid = new StringContent(id))
                        using (StringContent name = new StringContent(RemarkName))
                        {
                            content.Add(uid, "uid");
                            content.Add(name, "name");
                            (bool isSucceed, JToken token) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.PostUpdateRemark), content);
                            if (isSucceed)
                            {
                                _ = this.ShowMessageAsync("设置成功");
                                SettingsHelper.UserRemarks = await RemarkModel.GetRemarkDictionary(SettingsHelper.Get<string>(SettingsHelper.Uid)).ContinueWith(x => x.Result);
                            }
                        }
                    }
                }
                _ = this.HideProgressBarAsync();
            }
        }
    }
}
