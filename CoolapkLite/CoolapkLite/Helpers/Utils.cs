using CoolapkLite.Helpers;
using Html2Markdown;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.Helpers
{
    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }

    public static partial class Utils
    {
        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (T cur in e)
            {
                yield return cur;
            }
            yield return value;
        }

        public static IEnumerable Append(this IEnumerable first, params object[] second)
        {
            return first.OfType<object>().Concat(second);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.Concat(second);
        }

        public static IEnumerable Prepend(this IEnumerable first, params object[] second)
        {
            return second.Concat(first.OfType<object>());
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> first, params T[] second)
        {
            return second.Concat(first);
        }
    }
}
