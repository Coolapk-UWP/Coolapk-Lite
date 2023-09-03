using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.Providers
{
    public class CoolapkListProvider
    {
        private readonly string _idName;
        private string _firstItem, _lastItem;
        private readonly Func<int, string, string, Uri> _getUri;

        public Func<JObject, IEnumerable<Entity>> GetEntities { get; }

        public CoolapkListProvider(Func<int, string, string, Uri> getUri, Func<JObject, IEnumerable<Entity>> getEntities, string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            GetEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空") : idName;
        }

        public void Clear() => _lastItem = _firstItem = string.Empty;

        public async Task GetEntityAsync<T>(ICollection<T> Models, CoreDispatcher dispatcher, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = (JArray)result.result;
                if (array.Count < 1) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = RequestHelper.GetId(array.First, _idName);
                }
                _lastItem = RequestHelper.GetId(array.Last, _idName);
                foreach (JObject item in array)
                {
                    if (dispatcher?.HasThreadAccess == false)
                    {
                        await dispatcher.ResumeForegroundAsync();
                    }

                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }

                    foreach (Entity entity in entities)
                    {
                        if (entity is T model)
                        {
                            Models.Add(model);
                        }
                    }
                }
            }
        }

        public async Task GetEntityAsync<T>(IEnumerable<T> Models, CoreDispatcher dispatcher, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = (JArray)result.result;
                if (array.Count < 1) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = RequestHelper.GetId(array.First, _idName);
                }
                _lastItem = RequestHelper.GetId(array.Last, _idName);
                foreach (JObject item in array)
                {
                    if (dispatcher?.HasThreadAccess == false)
                    {
                        await dispatcher.ResumeForegroundAsync();
                    }

                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }

                    Models = Models.Concat(entities.OfType<T>());
                }
            }
        }
    }
}
