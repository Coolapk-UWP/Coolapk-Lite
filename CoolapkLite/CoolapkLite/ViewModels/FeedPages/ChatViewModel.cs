using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Message;
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
    public class ChatViewModel : EntityItemSource, IViewModel
    {
        public string ID { get; }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ChatViewModel(string id, string title, CoreDispatcher dispatcher) : base(dispatcher)
        {
            Title = title;
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.GetMessageChat,
                        id,
                        p,
                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "dateline");
        }

        bool IViewModel.IsEqual(IViewModel other) => other is ChatViewModel model && IsEqual(model);
        public bool IsEqual(ChatViewModel other) => ID == other.ID;

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items.Reverse())
                {
                    if (!(item is NullEntity))
                    {
                        await AddAsync(item).ConfigureAwait(false);
                    }
                }
            }
        }

        public override async Task AddAsync(Entity item)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            InsertItem(0, item);
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            switch (json.Value<string>("entityType"))
            {
                case "message":
                    yield return new MessageModel(json);
                    break;
                case "messageExtra":
                    yield return new MessageExtraModel(json);
                    break;
            }
            yield break;
        }
    }
}
