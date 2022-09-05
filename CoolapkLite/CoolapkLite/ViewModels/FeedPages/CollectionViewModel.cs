using CoolapkLite.Controls;
using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class CollectionViewModel : IViewModel, INotifyPropertyChanged
    {
        protected string ID;
        public double[] VerticalOffsets { get; set; } = new double[3];

        private string title = string.Empty;
        public string Title
        {
            get => title;
            protected set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

        private CollectionModel detail;
        public CollectionModel Detail
        {
            get => detail;
            protected set
            {
                detail = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                itemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public CollectionViewModel(string id)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentException(nameof(id)); }
            ID = id;
        }

        protected static async Task<CollectionModel> GetDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionDetail, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new CollectionModel(detail) : null;
        }

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }

        public async Task Refresh(bool reset = false)
        {
            if (Detail == null || reset)
            {
                Detail = await GetDetailAsync(ID);
                Title = Detail.Title;
            }
            if (ItemSource == null)
            {
                (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetCollectionContents, ID, "1", ""), true);
                if (isSucceed)
                {
                    JArray array = (JArray)result;
                    foreach (JObject item in array)
                    {
                        if (item.TryGetValue("entityTemplate", out JToken entityTemplate) && entityTemplate.ToString() == "selectorLinkCard")
                        {
                            if (item.TryGetValue("entities", out JToken v1))
                            {
                                JArray entities = (JArray)v1;
                                List<ShyHeaderItem> ItemSource = new List<ShyHeaderItem>();
                                foreach (JObject entity in entities)
                                {
                                    if (entity.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
                                    {
                                        CoolapkListProvider Provider = new CoolapkListProvider(
                                            (p, firstItem, lastItem) => UriHelper.GetUri(UriType.DataList, url.ToString().Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"), $"&page={p}" + (string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}") + (string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}")),
                                            GetEntities,
                                            "id");
                                        CollectionItemSourse CollectionItemSourse = new CollectionItemSourse(ID, Provider);
                                        ShyHeaderItem ShyHeaderItem = new ShyHeaderItem { ItemSource = CollectionItemSourse };
                                        if (entity.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
                                        {
                                            ShyHeaderItem.Header = title.ToString();
                                        }
                                        ItemSource.Add(ShyHeaderItem);
                                    }
                                }
                                this.ItemSource = ItemSource;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public class CollectionItemSourse : EntityItemSourse
    {
        public string ID;

        public CollectionItemSourse(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullModel) { continue; }
                    Add(item);
                }
            }
        }
    }
}
