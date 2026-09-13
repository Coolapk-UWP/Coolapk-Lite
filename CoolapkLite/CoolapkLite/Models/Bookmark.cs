using System;
using System.Collections.Generic;

namespace CoolapkLite.Models
{
    public sealed class Bookmark : IHasTitle, IEquatable<Bookmark>
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public Bookmark(string url, string title)
        {
            Url = url;
            Title = title;
        }

        public static Bookmark[] GetDefaultBookmarks()
        {
            return new[]
            {
                new Bookmark("/page?url=V11_FIND_COOLPIC", "酷图"),
                new Bookmark("/page?url=V11_HOME_TAB_NEWS", "快讯"),
                new Bookmark("/page?url=V9_HOME_TAB_SHIPIN", "视频"),
                new Bookmark("/dyh/1480", "铺路根据地")
            };
        }

        public override string ToString() => string.Join(" - ", Title, Url);

        public override bool Equals(object obj) => Equals(obj as Bookmark);

        public override int GetHashCode() => (Title, Url).GetHashCode();

        public bool Equals(Bookmark other) => other is Bookmark && Title == other.Title && Url == other.Url;

        public static bool operator ==(Bookmark left, Bookmark right) => EqualityComparer<Bookmark>.Default.Equals(left, right);

        public static bool operator !=(Bookmark left, Bookmark right) => !(left == right);
    }
}
