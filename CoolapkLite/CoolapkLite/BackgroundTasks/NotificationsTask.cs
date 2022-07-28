using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace CoolapkLite.BackgroundTasks
{
    public sealed class NotificationsTask : IBackgroundTask, INotifyPropertyChanged
    {
        public static NotificationsTask Instance = new NotificationsTask(false);

        private double badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum, cloudInstall, notification;

        /// <summary>
        /// 新的消息总数。
        /// </summary>
        public double BadgeNum
        {
            get => badgeNum;
            private set
            {
                if (value != badgeNum)
                {
                    badgeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新增粉丝数。
        /// </summary>
        public double FollowNum
        {
            get => followNum;
            private set
            {
                if (value != followNum)
                {
                    followNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新私信数。
        /// </summary>
        public double MessageNum
        {
            get => messageNum;
            private set
            {
                if (value != messageNum)
                {
                    messageNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“@我的动态”数。
        /// </summary>
        public double AtMeNum
        {
            get => atMeNum;
            private set
            {
                if (value != atMeNum)
                {
                    atMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“@我的回复”数。
        /// </summary>
        public double AtCommentMeNum
        {
            get => atCommentMeNum;
            private set
            {
                if (value != atCommentMeNum)
                {
                    atCommentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新回复数。
        /// </summary>
        public double CommentMeNum
        {
            get => commentMeNum;
            private set
            {
                if (value != commentMeNum)
                {
                    commentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“收到的赞”数。
        /// </summary>
        public double FeedLikeNum
        {
            get => feedLikeNum;
            private set
            {
                if (value != feedLikeNum)
                {
                    feedLikeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“云安装”数。
        /// </summary>
        public double CloudInstall
        {
            get => cloudInstall;
            private set
            {
                if (value != cloudInstall)
                {
                    cloudInstall = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// 新“通知”数。
        /// </summary>
        public double Notification
        {
            get => notification;
            private set
            {
                if (value != notification)
                {
                    notification = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public NotificationsTask(bool renew = true)
        {
            if (renew) { Instance = this; }
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            GetNums();
            deferral.Complete();
        }

        /// <summary>
        /// 将数字归零。
        /// </summary>
        public void ClearNums() => BadgeNum = FollowNum = MessageNum = AtMeNum = AtCommentMeNum = CommentMeNum = FeedLikeNum = CloudInstall = Notification = 0;

        public async void GetNums()
        {
            try
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
                if (!isSucceed) { return; }
                ChangeNumber((JObject)result);
            }
            catch { ClearNums(); }
        }

        private void ChangeNumber(JObject o)
        {
            if (o != null)
            {
                if (o.TryGetValue("cloudInstall", out JToken cloudInstall) && cloudInstall != null)
                {
                    CloudInstall = o.Value<int>("cloudInstall");
                }
                if (o.TryGetValue("notification", out JToken notification) && notification != null)
                {
                    Notification = o.Value<int>("notification");
                }
                if (o.TryGetValue("badge", out JToken badge) && badge != null)
                {
                    BadgeNum = o.Value<int>("badge");
                    UIHelper.SetBadgeNumber(BadgeNum.ToString());
                }
                if (o.TryGetValue("contacts_follow", out JToken contacts_follow) && contacts_follow != null)
                {
                    FollowNum = o.Value<int>("contacts_follow");
                }
                if (o.TryGetValue("message", out JToken message) && message != null)
                {
                    MessageNum = o.Value<int>("message");
                }
                if (o.TryGetValue("atme", out JToken atme) && atme != null)
                {
                    AtMeNum = o.Value<int>("atme");
                }
                if (o.TryGetValue("atcommentme", out JToken atcommentme) && atcommentme != null)
                {
                    AtCommentMeNum = o.Value<int>("atcommentme");
                }
                if (o.TryGetValue("commentme", out JToken commentme) && commentme != null)
                {
                    CommentMeNum = o.Value<int>("commentme");
                }
                if (o.TryGetValue("feedlike", out JToken feedlike) && feedlike != null)
                {
                    FeedLikeNum = o.Value<int>("feedlike");
                }
            }
        }
    }
}
