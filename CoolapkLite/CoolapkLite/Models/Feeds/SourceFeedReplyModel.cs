using CoolapkLite.Controls;
using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Feeds
{
    public class SourceFeedReplyModel : Entity
    {
        public int ID { get; private set; }
        public string Uurl { get; private set; }
        public string Rurl { get; private set; }
        public string PicUri { get; private set; }
        public string Message { get; private set; }
        public int BlockStatus { get; private set; }
        public string UserName { get; private set; }
        public string Rusername { get; private set; }
        public bool IsFeedAuthor { get; private set; }

        public SourceFeedReplyModel(JObject token) : base(token)
        {
            if (token.TryGetValue("id", out JToken id))
            {
                ID = id.ToObject<int>();
            }

            if (token.TryGetValue("uid", out JToken uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetValue("isFeedAuthor", out JToken isFeedAuthor))
            {
                IsFeedAuthor = isFeedAuthor.ToObject<int>() == 1;
            }

            if (token.TryGetValue("ruid", out JToken ruid))
            {
                Rurl = $"/u/{ruid}";
            }

            if (token.TryGetValue("rusername", out JToken rusername))
            {
                Rusername = rusername.ToString();
            }

            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("Feed");

            if (token.TryGetValue("message", out JToken message))
            {
                Message =
                string.IsNullOrEmpty(Rusername)
                ? $"{GetUserLink(Uurl, UserName) + GetAuthorString(IsFeedAuthor)}: {message}"
                : $"{GetUserLink(Uurl, UserName) + GetAuthorString(IsFeedAuthor)}@{GetUserLink(Rurl, Rusername)}: {message}";
            }

            if(token.TryGetValue("pic", out JToken pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                PicUri = pic.ToString();
                Message += $" <a href=\"{PicUri}\">{loader.GetString("seePic")}</a>";
            }

            if (token.TryGetValue("block_status", out JToken block_status))
            {
                BlockStatus = block_status.ToObject<int>();
            }
        }

        private static string GetAuthorString(bool isFeedAuthor)
        {
            return isFeedAuthor ? TextBlockEx.AuthorBorder : string.Empty;
        }

        private static string GetUserLink(string url, string name)
        {
            return $"<a href=\"{url}\" type=\"user-detail\">{name}</a>";
        }
    }
}
