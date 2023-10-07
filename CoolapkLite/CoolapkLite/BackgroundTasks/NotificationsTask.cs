using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

namespace CoolapkLite.BackgroundTasks
{
    public sealed class NotificationsTask : IBackgroundTask
    {
        public static NotificationsTask Instance = new NotificationsTask();

        public NotificationsTask() => Instance = Instance ?? this;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            await UpdateNotificationsAsync();
            deferral.Complete();
        }

        private async Task UpdateNotificationsAsync()
        {
            if (mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true).ConfigureAwait(false);
                if (!isSucceed) { return; }
                JObject token = (JObject)result;
                if (token != null)
                {
                    if (token.TryGetValue("badge", out JToken badge) && badge != null)
                    {
                        UIHelper.SetBadgeNumber(badge.ToObject<uint>());
                    }
                }
            }
        }
    }
}
