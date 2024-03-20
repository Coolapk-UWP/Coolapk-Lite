using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkLite.Models
{
    public class RemarkModel
    {
        public int ID { get; private set; }
        public int UID { get; private set; }
        public int RemarkUID { get; private set; }
        public string RemarkName { get; private set; }
        public DateTimeOffset Dateline { get; private set; }

        public RemarkModel(JObject token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                UID = uid.ToObject<int>();
            }

            if (token.TryGetValue("remark_uid", out JToken remark_uid))
            {
                RemarkUID = remark_uid.ToObject<int>();
            }

            if (token.TryGetValue("remark_name", out JToken remark_name))
            {
                RemarkName = remark_name.ToString();
            }

            if (token.TryGetValue("dateline", out JToken dateline))
            {
                Dateline = dateline.ToObject<long>().ConvertUnixTimeStampToDateTimeOffset();
            }
        }

        public static async Task<IEnumerable<RemarkModel>> GetRemarkList(string uid)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetRemarkList, uid)).ConfigureAwait(false);
            if (isSucceed)
            {
                return result.OfType<JObject>().Select(x => new RemarkModel(x));
            }
            return Enumerable.Empty<RemarkModel>();
        }

        public static async Task<ImmutableDictionary<int, string>> GetRemarkDictionary(string uid)
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            foreach (RemarkModel remark in await GetRemarkList(uid))
            {
                dictionary[remark.RemarkUID] = remark.RemarkName;
            }
            return dictionary.ToImmutableDictionary();
        }
    }
}
