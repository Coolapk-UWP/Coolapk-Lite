using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels.Providers
{
    public class CoolapkListProvider
    {
        private readonly string _idName;
        private string _firstItem, _lastItem;
        private readonly Func<int, string, string, Uri> _getUri;

        public Uri Uri => _getUri(1, string.Empty, string.Empty);
        public Func<JObject, IEnumerable<Entity>> GetEntities { get; }

        public CoolapkListProvider(Func<int, string, string, Uri> getUri, Func<JObject, IEnumerable<Entity>> getEntities, string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            GetEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空") : idName;
        }

        public void Clear() => _lastItem = _firstItem = string.Empty;

        public async Task GetEntityAsync<T>(ICollection<T> models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count <= 0) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = GetEntityID(array.First as JObject, _idName);
                }
                _lastItem = GetEntityID(array.Last as JObject, _idName);
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    models.AddRange(entities.OfType<T>());
                }
            }
        }

        public async Task<IEnumerable<T>> GetEntityAsync<T>(IEnumerable<T> models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count <= 0) { return models; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = GetEntityID(array.First as JObject, _idName);
                }
                _lastItem = GetEntityID(array.Last as JObject, _idName);
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    models = models.Concat(entities.OfType<T>());
                }
            }
            return models;
        }

        public async Task<uint> GetEntityAsync<T>(IncrementalLoadingBase<T> models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count <= 0) { return 0; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = GetEntityID(array.First as JObject, _idName);
                }
                _lastItem = GetEntityID(array.Last as JObject, _idName);
                uint count = 0;
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    foreach (T entity in entities.OfType<T>())
                    {
                        if (await models.AddItemAsync(entity))
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
            return 0;
        }

        private static string GetEntityID(JObject token, string _idName)
        {
            return token == null
                ? string.Empty
                : token.TryGetValue(_idName, out JToken idName)
                    ? idName.ToString()
                    : token.TryGetValue("entityId", out JToken entityId)
                        ? entityId.ToString()
                        : token.TryGetValue("id", out JToken id)
                            ? id.ToString()
                            : string.Empty;
        }
    }
}
