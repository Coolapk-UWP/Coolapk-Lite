using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Core.Providers
{
    public class CoolapkListProvider
    {
        private readonly string _idName;
        private string _firstItem, _lastItem;
        private Func<int, string, string, Uri> _getUri;
        private readonly Func<JObject, IEnumerable<Entity>> _getEntities;

        public CoolapkListProvider(Func<int, string, string, Uri> getUri, Func<JObject, IEnumerable<Entity>> getEntities, string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            _getEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空") : idName;
        }

        private string GetId(JToken token)
        {
            return token == null
                ? string.Empty
                : (token as JObject).TryGetValue(_idName, out JToken jToken)
                    ? jToken.ToString()
                    : (token as JObject).TryGetValue("entityId", out JToken v1)
                                    ? v1.ToString()
                                    : (token as JObject).TryGetValue("id", out JToken v2) ? v2.ToString() : throw new ArgumentException(nameof(_idName));
        }

        public void Clear() => _lastItem = _firstItem = string.Empty;

        public async Task<List<Entity>> GetEntity(List<Entity> Models, int p = 1)
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JToken result) result = await Utils.GetDataAsync(_getUri(p, _firstItem, _lastItem), false);
            if (result.isSucceed)
            {
                JArray array = (JArray)result.result;
                if (array.Count < 1) { return Models; }
                if (string.IsNullOrEmpty(_firstItem))
                {
                    _firstItem = Utils.GetId(array.First, _idName);
                }
                _lastItem = Utils.GetId(array.Last, _idName);
                foreach (JObject item in array)
                {
                    IEnumerable<Entity> entities = _getEntities(item);
                    if (entities == null) { continue; }

                    foreach (Entity i in entities)
                    {
                        if (i == null) { continue; }
                        Models.Add(i);
                    }
                }
            }
            return Models;
        }
    }
}
