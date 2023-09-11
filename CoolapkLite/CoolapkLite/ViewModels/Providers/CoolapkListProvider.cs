﻿using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
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

        public Func<JObject, IEnumerable<Entity>> GetEntities { get; }

        public CoolapkListProvider(Func<int, string, string, Uri> getUri, Func<JObject, IEnumerable<Entity>> getEntities, string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            GetEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空") : idName;
        }

        public void Clear() => _lastItem = _firstItem = string.Empty;

        public async Task GetEntityAsync<T>(List<T> Models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count < 1) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = RequestHelper.GetId(array.First, _idName);
                }
                _lastItem = RequestHelper.GetId(array.Last, _idName);
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    Models.AddRange(entities.OfType<T>());
                }
            }
        }

        public async Task GetEntityAsync<T>(ICollection<T> Models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count < 1) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = RequestHelper.GetId(array.First, _idName);
                }
                _lastItem = RequestHelper.GetId(array.Last, _idName);
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    entities.OfType<T>().ForEach(Models.Add);
                }
            }
        }

        public async Task GetEntityAsync<T>(IEnumerable<T> Models, int p = 1) where T : Entity
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false).ConfigureAwait(false);
            if (result.isSucceed)
            {
                JArray array = result.result as JArray;
                if (array.Count < 1) { return; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = RequestHelper.GetId(array.First, _idName);
                }
                _lastItem = RequestHelper.GetId(array.Last, _idName);
                foreach (JObject item in array.OfType<JObject>())
                {
                    IEnumerable<Entity> entities = GetEntities(item);
                    if (entities == null) { continue; }
                    Models = Models.Concat(entities.OfType<T>());
                }
            }
        }
    }
}
