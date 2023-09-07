using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class AdaptiveViewModel : EntityItemSource, IViewModel
    {
        private readonly string Uri;
        private readonly List<Type> EntityTypes;
        protected bool IsInitPage => Uri == "/main/init";
        protected bool IsIndexPage => !Uri.Contains("?");
        protected bool IsHotFeedPage => Uri == "/main/indexV8" || Uri == "/main/index";

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set => SetProperty(ref title, value);
        }

        private bool isShowTitle;
        public bool IsShowTitle
        {
            get => isShowTitle;
            set => SetProperty(ref isShowTitle, value);
        }

        internal AdaptiveViewModel(string uri, CoreDispatcher dispatcher, List<Type> types = null) : base(dispatcher)
        {
            Uri = GetUri(uri);
            EntityTypes = types;
            Provider = new CoolapkListProvider(
                (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, IsIndexPage ? "?" : "&", p),
                GetEntities,
                "entityId");
        }

        internal AdaptiveViewModel(CoolapkListProvider provider, CoreDispatcher dispatcher, List<Type> types = null) : base(dispatcher)
        {
            Provider = provider;
            EntityTypes = types;
        }

        public static AdaptiveViewModel GetUserListProvider(string uid, bool isFollowList, string name, CoreDispatcher dispatcher)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            isFollowList ? "followList" : "fansList",
                            uid,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (o) => new Entity[] { new UserModel((JObject)(isFollowList ? o["fUserInfo"] : o["userInfo"])) },
                    "fuid"), dispatcher)
                { Title = $"{name}的{(isFollowList ? "关注" : "粉丝")}" };
        }

        public static AdaptiveViewModel GetReplyListProvider(string id, CoreDispatcher dispatcher, FeedReplyModel reply = null)
        {
            return string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : reply == null
                ? new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetHotReplies,
                                id,
                                p,
                                p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o) },
                        "uid"), dispatcher)
                { Title = $"热门回复" }
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetReplyReplies,
                                id,
                                p,
                                p > 1 ? $"&lastItem={lastItem}" : string.Empty),
                        (o) => new Entity[] { new FeedReplyModel(o, false) },
                        "uid"), dispatcher)
                { Title = $"回复({reply.ReplyNum})" };
        }

        public static AdaptiveViewModel GetHistoryProvider(string title, CoreDispatcher dispatcher)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }

            UriType type = UriType.CheckLoginInfo;

            switch (title)
            {
                case "我的常去":
                    type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            return new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            type,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (o) => new Entity[] { new HistoryModel(o) },
                    "uid"), dispatcher)
            { Title = title };
        }

        public static AdaptiveViewModel GetUserFeedsProvider(string uid, string branch, CoreDispatcher dispatcher)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetUserFeeds,
                                uid,
                                p,
                                string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                                branch),
                        EntityTemplateSelector.GetEntities,
                        "uid"), dispatcher);
        }

        public static AdaptiveViewModel GetUserCollectionListProvider(string uid, CoreDispatcher dispatcher)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetCollectionList,
                                string.Empty,
                                p,
                                string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                        EntityTemplateSelector.GetEntities,
                        "uid"), dispatcher);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AdaptiveViewModel model && IsEqual(model);

        public bool IsEqual(AdaptiveViewModel other) => !string.IsNullOrWhiteSpace(Uri) ? Uri == other.Uri : Provider == other.Provider;

        private string GetUri(string uri)
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }

            if (uri.StartsWith("url="))
            {
                uri = uri.Substring(4);
            }

            if (uri.StartsWith("/page", StringComparison.Ordinal) && !uri.Contains("/page/dataList"))
            {
                uri = uri.Replace("/page", "/page/dataList");
            }
            else if (uri.Contains("/page", StringComparison.Ordinal) && uri.StartsWith("#", StringComparison.Ordinal))
            {
                uri = $"/page/dataList?url={uri}";
            }
            else if (!uri.Contains("/main/") && !uri.Contains("/user/") && !uri.Contains("/apk/") && !uri.Contains("/appForum/") && !uri.Contains("/picture/") && !uri.Contains("/topic/") && !uri.Contains("/discovery/"))
            {
                uri = $"/page/dataList?url={uri}";
            }

            return uri.Replace("#", "%23");
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            if (json.TryGetValue("entityTemplate", out JToken t) && t?.ToString() == "configCard")
            {
                JObject j = JObject.Parse(json.Value<string>("extraData"));
                Title = j.Value<string>("pageTitle");
                yield return null;
            }
            else if (json.TryGetValue("entityTemplate", out JToken tt) && tt?.ToString() == "fabCard") { yield return null; }
            else if (tt?.ToString() == "feedCoolPictureGridCard")
            {
                foreach (JObject item in json.Value<JArray>("entities").OfType<JObject>())
                {
                    Entity entity = EntityTemplateSelector.GetEntity(item, IsHotFeedPage);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
            }
            else
            {
                yield return EntityTemplateSelector.GetEntity(json, IsHotFeedPage);
            }
            yield break;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    if (EntityTypes == null || EntityTypes.Contains(item.GetType()))
                    {
                        await AddAsync(item);
                        AddSubProvider(item);
                    }
                }
            }
        }
    }
}
