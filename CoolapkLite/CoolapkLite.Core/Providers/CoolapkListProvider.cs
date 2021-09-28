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
        private int page;
        private readonly string _idName;
        private string _firstItem, _lastItem;
        private readonly MessageType _messageType;
        private Func<int, int, string, string, Uri> _getUri;
        private readonly Func<JObject, IEnumerable<Entity>> _getEntities;

        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<JObject, IEnumerable<Entity>> getEntities,
            string idName)
            : this(getUri, getEntities, MessageType.NoMore, idName) { }

        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<JObject, IEnumerable<Entity>> getEntities,
            MessageType messageType,
            string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            _getEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _messageType = messageType;
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                       : idName;
        }

        public void ChangeGetDataFunc(Func<int, int, string, string, Uri> getUri, Func<Entity, bool> needDeleteJudger)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            page = 0;
        }

        public void Reset(int p = 1)
        {
            page = p;
            _lastItem = _firstItem = string.Empty;
        }

        public void Clear()
        {
            page = 0;
            _lastItem = _firstItem = string.Empty;
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

        public async Task GetEntity(IEnumerable<(string, string)> cookies, int p = -1)
        {
            if (p == -2) { Reset(0); }

            (bool isSucceed, JToken result) = await Utils.GetDataAsync(_getUri(p, page, _firstItem, _lastItem), p == -2, cookies);
            if (!isSucceed) { return; }

            JArray array = (JArray)result;
            if (p == -1) { page++; }

            if (array != null && array.Count > 0)
            {
                ObservableCollection<Entity> Models = new ObservableCollection<Entity>();
                Entity[] FixedEntities = (from m in Models
                                          where m.EntityFixed
                                          select m).ToArray();
                int FixedNum = FixedEntities.Length;
                foreach (Entity item in FixedEntities)
                {
                    _ = Models.Remove(item);
                }

                for (int i = 0; i < FixedNum; i++)
                {
                    Models.Insert(i, FixedEntities[i]);
                }

                if (p == 1)
                {
                    _firstItem = GetId(array.First);
                    if (page == 1)
                    {
                        _lastItem = GetId(array.Last);
                    }

                    int modelIndex = 0;

                    for (int i = 0; i < array.Count; i++)
                    {
                        IEnumerable<Entity> entities = _getEntities((JObject)array[i]);
                        if (entities == null) { continue; }

                        foreach (Entity item in entities)
                        {
                            if (item == null) { continue; }

                            Models.Insert(modelIndex + FixedNum, item);
                            modelIndex++;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(_firstItem))
                    {
                        _firstItem = GetId(array.First);
                    }
                    _lastItem = GetId(array.Last);

                    foreach (JObject item in array)
                    {
                        IEnumerable<Entity> entities = _getEntities(item);
                        if (entities == null) { continue; }

                        foreach (Entity i in entities)
                        {
                            if (i == null) { continue; }
                            bool b = FixedEntities.Any(k => k.EntityId == i.EntityId);
                            if (b) { continue; }

                            Models.Add(i);
                        }
                    }
                }
            }
            else if (p == -1)
            {
                page--;
                Utils.ShowInAppMessage(_messageType);
            }
        }
    }
}
