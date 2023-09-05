using CoolapkLite.Common;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

namespace CoolapkLite.Models
{
    public class NotificationsModel : INotifyPropertyChanged
    {
        public static Dictionary<CoreDispatcher, NotificationsModel> Caches { get; } = new Dictionary<CoreDispatcher, NotificationsModel>();

        private static readonly DispatcherTimer timer;
        private static int badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum, cloudInstall, notification;

        public CoreDispatcher Dispatcher { get; }

        /// <summary>
        /// 新的消息总数。
        /// </summary>
        public int BadgeNum
        {
            get => badgeNum;
            private set => SetProperty(ref badgeNum, value);
        }

        public static void SetBadgeNum(int value)
        {
            badgeNum = value;
            RaisePropertyChangedEvent(nameof(BadgeNum));
        }

        /// <summary>
        /// 新增粉丝数。
        /// </summary>
        public int FollowNum
        {
            get => followNum;
            private set => SetProperty(ref followNum, value);
        }

        public static void SetFollowNum(int value)
        {
            followNum = value;
            RaisePropertyChangedEvent(nameof(FollowNum));
        }

        /// <summary>
        /// 新私信数。
        /// </summary>
        public int MessageNum
        {
            get => messageNum;
            private set => SetProperty(ref messageNum, value);
        }

        public static void SetMessageNum(int value)
        {
            messageNum = value;
            RaisePropertyChangedEvent(nameof(MessageNum));
        }

        /// <summary>
        /// 新“@我的动态”数。
        /// </summary>
        public int AtMeNum
        {
            get => atMeNum;
            private set => SetProperty(ref atMeNum, value);
        }

        public static void SetAtMeNum(int value)
        {
            atMeNum = value;
            RaisePropertyChangedEvent(nameof(AtMeNum));
        }

        /// <summary>
        /// 新“@我的回复”数。
        /// </summary>
        public int AtCommentMeNum
        {
            get => atCommentMeNum;
            private set => SetProperty(ref atCommentMeNum, value);
        }

        public static void SetAtCommentMeNum(int value)
        {
            atCommentMeNum = value;
            RaisePropertyChangedEvent(nameof(AtCommentMeNum));
        }

        /// <summary>
        /// 新回复数。
        /// </summary>
        public int CommentMeNum
        {
            get => commentMeNum;
            private set => SetProperty(ref commentMeNum, value);
        }

        public static void SetCommentMeNum(int value)
        {
            commentMeNum = value;
            RaisePropertyChangedEvent(nameof(CommentMeNum));
        }

        /// <summary>
        /// 新“收到的赞”数。
        /// </summary>
        public int FeedLikeNum
        {
            get => feedLikeNum;
            private set => SetProperty(ref feedLikeNum, value);
        }

        public static void SetFeedLikeNum(int value)
        {
            feedLikeNum = value;
            RaisePropertyChangedEvent(nameof(FeedLikeNum));
        }

        /// <summary>
        /// 新“云安装”数。
        /// </summary>
        public int CloudInstall
        {
            get => cloudInstall;
            private set => SetProperty(ref cloudInstall, value);
        }

        public static void SetCloudInstall(int value)
        {
            cloudInstall = value;
            RaisePropertyChangedEvent(nameof(CloudInstall));
        }

        /// <summary>
        /// 新“通知”数。
        /// </summary>
        public int Notification
        {
            get => notification;
            private set => SetProperty(ref notification, value);
        }

        public static void SetNotification(int value)
        {
            notification = value;
            RaisePropertyChangedEvent(nameof(Notification));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, NotificationsModel> cache in Caches)
                {
                    if (cache.Key?.HasThreadAccess == false)
                    {
                        await cache.Key.ResumeForegroundAsync();
                    }
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        static NotificationsModel()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += async (o, a) =>
            {
                if (mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    await Update(true);
                }
            };
            timer.Start();
        }

        public NotificationsModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
        }

        /// <summary>
        /// 将数字归零。
        /// </summary>
        public static void Clear()
        {
            SetBadgeNum(0);
            SetFollowNum(0);
            SetMessageNum(0);
            SetAtMeNum(0);
            SetAtCommentMeNum(0);
            SetCommentMeNum(0);
            SetFeedLikeNum(0);
            SetCloudInstall(0);
            SetNotification(0);
        }

        public static async Task Update(bool notify = false)
        {
            try
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
                if (!isSucceed) { return; }
                ChangeNumber((JObject)result, notify);
            }
            catch { Clear(); }
        }

        private static void ChangeNumber(JObject token, bool notify)
        {
            if (token != null)
            {
                int increase = 0;

                if (token.TryGetValue("cloudInstall", out JToken cloudInstall))
                {
                    SetCloudInstall(cloudInstall.ToObject<int>());
                }

                if (token.TryGetValue("notification", out JToken notification))
                {
                    SetNotification(notification.ToObject<int>());
                }

                if (token.TryGetValue("badge", out JToken badge))
                {
                    int value = badge.ToObject<int>();
                    increase = value - badgeNum;
                    SetBadgeNum(value);
                    UIHelper.SetBadgeNumber((uint)badgeNum);
                }

                if (token.TryGetValue("contacts_follow", out JToken contacts_follow))
                {
                    SetFollowNum(contacts_follow.ToObject<int>());
                }

                if (token.TryGetValue("message", out JToken message))
                {
                    SetMessageNum(message.ToObject<int>());
                }

                if (token.TryGetValue("atme", out JToken atme))
                {
                    SetAtMeNum(atme.ToObject<int>());
                }

                if (token.TryGetValue("atcommentme", out JToken atcommentme))
                {
                    SetAtCommentMeNum(atcommentme.ToObject<int>());
                }

                if (token.TryGetValue("commentme", out JToken commentme))
                {
                    SetCommentMeNum(commentme.ToObject<int>());
                }

                if (token.TryGetValue("feedlike", out JToken feedlike))
                {
                    SetFeedLikeNum(feedlike.ToObject<int>());
                }

                if (notify && increase > 0)
                {
                    CreateToast(increase);
                }
            }
        }

        private static void CreateToast(int increase)
        {
            List<string> builder = new List<string>();

            if (commentMeNum > 0)
            {
                builder.Add($"{commentMeNum} 个未读回复");
            }

            if (feedLikeNum > 0)
            {
                builder.Add($"{feedLikeNum} 个点赞");
            }

            if (messageNum > 0)
            {
                builder.Add($"{messageNum} 个未读私信");
            }

            if (followNum > 0)
            {
                builder.Add($"{followNum} 位新增粉丝");
            }

            if (atMeNum > 0)
            {
                builder.Add($"{atMeNum} 个@我的动态");
            }

            if (atCommentMeNum > 0)
            {
                builder.Add($"{atCommentMeNum} 个@我的回复");
            }

            if (cloudInstall > 0)
            {
                builder.Add($"{cloudInstall} 个云安装");
            }

            new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddArgument("action", "hasNotification")
                .AddText($"新增 {increase} 个未读通知")
                .AddText($"共有 {badgeNum} 个未读消息")
                .AddText(string.Join("，", builder))
                .AddButton(new ToastButton()
                    .SetContent("查看")
                    .SetProtocolActivation(new Uri("coolapk://notifications")))
                .AddButton(new ToastButton()
                    .SetDismissActivation())
                .Show();
        }
    }
}
