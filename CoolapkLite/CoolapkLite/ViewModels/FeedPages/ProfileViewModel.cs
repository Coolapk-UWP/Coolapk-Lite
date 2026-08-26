using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Pages;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class ProfileViewModel : EntityItemSource, IViewModel
    {
        public string UID = string.Empty;
        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("ProfilePage").GetString("Title");

        private bool isLogin;
        public bool IsLogin
        {
            get => isLogin;
            private set => SetProperty(ref isLogin, value);
        }

        private ProfileDetailModel profileDetail;
        public ProfileDetailModel ProfileDetail
        {
            get => profileDetail;
            private set => SetProperty(ref profileDetail, value);
        }

        private NotificationsModel _notificationsModel;
        public NotificationsModel NotificationsModel
        {
            get => _notificationsModel;
            private set => SetProperty(ref _notificationsModel, value);
        }

        public ProfileViewModel(CoreDispatcher dispatcher) : base(dispatcher)
        {
            Provider = new CoolapkListProvider(
                (_, __, ___) => UriHelper.GetUri(UriType.GetMyPageCard),
                GetEntities,
                "entityType");
        }

        public override async Task Refresh(bool reset)
        {
            IsLogin = await SettingsHelper.CheckLoginAsync().ConfigureAwait(false);
            if (IsLogin)
            {
                if (NotificationsModel == null)
                {
                    NotificationsModel = NotificationsModel.Caches.TryGetValue(Dispatcher, out NotificationsModel model) ? model : new NotificationsModel(Dispatcher);
                }
                UID = SettingsHelper.Get<string>(SettingsHelper.Uid);
                ProfileDetail = await GetFeedDetailAsync(UID).ConfigureAwait(false);
                await NotificationsModel.UpdateAsync().ConfigureAwait(false);
                await Reset().ConfigureAwait(false);
            }
            else
            {
                NotificationsModel = null;
                ProfileDetail = null;
                Clear();
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is ProfileViewModel model && IsEqual(model);

        public bool IsEqual(ProfileViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;

        protected override async Task<uint> LoadItemsAsync(uint count)
        {
            if (Provider != null && _currentPage == 1)
            {
                return await Provider.GetEntityAsync(this, _currentPage).ConfigureAwait(false);
            }
            return 0;
        }

        protected override async Task<uint> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            uint result = await base.LoadMoreItemsOverrideAsync(c, count).ConfigureAwait(false);
            _hasMoreItems = false;
            return result;
        }

        private static async Task<ProfileDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserProfile, id), true).ConfigureAwait(false);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new ProfileDetailModel(detail) : null;
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return GetEntity(json);
        }

        private static Entity GetEntity(JObject json)
        {
            switch (json.Value<string>("entityType"))
            {
                case "entity_type_user_card_manager": return null;
                default:
                    if (json.TryGetValue("entityTemplate", out JToken entityTemplate))
                    {
                        switch (entityTemplate.Value<string>())
                        {
                            case "imageTextGridCard":
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(json, EntityType.Others);
                            case "iconListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(json, EntityType.TextLinks);
                            case "titleCard": return new IndexPageOperationCardModel(json, OperationType.ShowTitle);
                            default: return null;
                        }
                    }
                    else { return null; }
            }
        }
    }
}
