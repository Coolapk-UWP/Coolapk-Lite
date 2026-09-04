using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using CoolapkLite.Models.Network;
using CoolapkLite.Models.Users;
using System;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class AccountCard : UserControl
    {
        #region Account

        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(
                nameof(Account),
                typeof(Account),
                typeof(AccountCard),
                new PropertyMetadata(new Account(), OnAccountChanged));

        public Account Account
        {
            get => (Account)GetValue(AccountProperty);
            set => SetValue(AccountProperty, value);
        }

        private static void OnAccountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _ = ((AccountCard)d).UpdateUserInfoAsync((Account)e.NewValue);
        }

        #endregion

        #region UserInfo

        public static readonly DependencyProperty UserInfoProperty =
            DependencyProperty.Register(
                nameof(UserInfo),
                typeof(UserInfoModel),
                typeof(AccountCard),
                null);

        public UserInfoModel UserInfo
        {
            get => (UserInfoModel)GetValue(UserInfoProperty);
            private set => SetValue(UserInfoProperty, value);
        }

        #endregion

        public AccountCard()
        {
            InitializeComponent();
        }

        public async Task UpdateUserInfoAsync(Account account)
        {
            if (account != null)
            {
                string uid = account.UID;
                string name = string.IsNullOrEmpty(uid) ? account.UserName : uid;
                if (await NetworkHelper.GetUserInfoByNameAsync(name) is UserInfoModel results)
                {
                    UserInfo = results;
                    return;
                }
            }
            UserInfo = null;
        }
    }

    public sealed class UrlDecodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = WebUtility.UrlDecode(value?.ToString());
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string result = WebUtility.UrlEncode(value?.ToString());
            return ConverterTools.Convert(result, targetType);
        }
    }
}
