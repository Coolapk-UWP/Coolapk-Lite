using CoolapkLite.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace CoolapkLite.Helpers
{
    public static class DataHelper
    {
        /// <summary>
        /// Get the MD5 hash of the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The MD5 hash of the input string.</returns>
        public static string GetMD5(this string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            using (MD5 md5Hasher = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                string results = BitConverter.ToString(data).ToLowerInvariant();
                return results.Replace("-", string.Empty);
            }
        }

        /// <summary>
        /// Get the Base64 string of the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="isRaw"><see langword="true"/> to remove the padding characters; otherwise, <see langword="false"/>.</param>
        /// <returns>The Base64 string of the input string.</returns>
        public static string GetBase64(this string input, bool isRaw = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);
            if (!isRaw) { result = result.Replace("=", string.Empty); }
            return result;
        }

        public static string GetSizeString(this double size)
        {
            int index = 0;
            while (index <= 11)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                case 5: str = "PB"; break;
                case 6: str = "EB"; break;
                case 7: str = "ZB"; break;
                case 8: str = "YB"; break;
                case 9: str = "BB"; break;
                case 10: str = "NB"; break;
                case 11: str = "DB"; break;
                default:
                    break;
            }
            return $"{size:0.##}{str}";
        }

        public static string GetNumString(this double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }

        public static bool IsTypePresent(string AssemblyName, string TypeName)
        {
            try
            {
                Assembly assembly = Assembly.Load(new AssemblyName(AssemblyName));
                Type supType = assembly.GetType($"{AssemblyName}.{TypeName}");
                if (supType != null)
                {
                    try { Activator.CreateInstance(supType); }
                    catch (MissingMethodException) { }
                }
                return supType != null;
            }
            catch
            {
                return false;
            }
        }

        public static string HtmlToString(this string str)
        {
            try
            {
                HtmlToText HtmlToText = new HtmlToText();
                return HtmlToText.Convert(str);
            }
            catch
            {
                //换行和段落
                string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", string.Empty).Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", string.Empty).Replace("<br />", string.Empty);
                //链接彻底删除！
                while (s.Contains("<a", StringComparison.Ordinal))
                {
                    s = s.Replace($@"<a href=""{Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0]}"">", string.Empty);
                    s = s.Replace("</a>", string.Empty);
                }
                return s;
            }
        }

        public static async Task<string> ConvertJsonStringAsync(this string str)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (TextReader textReader = new StringReader(str))
            using (JsonTextReader reader = new JsonTextReader(textReader))
            {
                JToken token = null;
                try { token = await JToken.ReadFromAsync(reader).ConfigureAwait(false); } catch { }
                return token != null ? token.ToString() : str;
            }
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(this Task<TResult> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.ConfigureAwait(false);
                    _ = taskCompletionSource.TrySetResult(result);
                }
                catch (Exception e)
                {
                    _ = taskCompletionSource.TrySetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }

        /// <summary>
        /// Returns a value indicating whether a specified character occurs within this string.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The character to seek.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string; otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this string text, char value) => text.IndexOf(value) != -1;

#if !NETCORE463
        /// <summary>
        /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string,
        /// or if <paramref name="value"/> is the empty string (""); otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this string text, string value, StringComparison comparisonType) => text.IndexOf(value, comparisonType) != -1;
#endif

        /// <summary>
        /// Returns a value indicating whether all of a specified array of string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="allOf">A Unicode character array containing one or more characters to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true"/> if the <paramref name="allOf"/> parameter occurs within this string; otherwise, <see langword="false"/>.</returns>
        public static bool ContainsAll(this string text, string[] allOf, StringComparison comparisonType) => allOf.All(x => text.Contains(x, comparisonType));

        /// <summary>
        /// Returns a value indicating whether any of a specified array of string occurs within this string.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
        /// <returns><see langword="true"/> if the <paramref name="anyOf"/> parameter occurs within this string; otherwise, <see langword="false"/>.</returns>
        public static bool ContainsAny(this string text, params string[] anyOf) => anyOf.Any(text.Contains);

        /// <summary>
        /// Returns a value indicating whether any of a specified array of string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="anyOf">A Unicode character array containing one or more characters to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true"/> if the <paramref name="anyOf"/> parameter occurs within this string; otherwise, <see langword="false"/>.</returns>
        public static bool ContainsAny(this string text, string[] anyOf, StringComparison comparisonType) => anyOf.Any(x => text.Contains(x, comparisonType));

        /// <summary>
        /// Try to concatenates the strings of the provided array, using the specified separator between each string,
        /// then appends the result to the current instance of the string builder.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings
        /// only if <paramref name="value"/> has more than one element.</param>
        /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendJoin(this StringBuilder builder, string separator, params string[] values) =>
            values?.Length > 0
                ? builder.Append(string.Join(separator, values.Where(x => !string.IsNullOrWhiteSpace(x))))
                : builder;

        /// <summary>
        /// Try to appends a copy of the specified string followed by the default line terminator to the end of the current StringBuilder object.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendLine(this StringBuilder builder, string value) =>
            !string.IsNullOrWhiteSpace(value)
                ? builder.AppendLine(value)
                : builder;

        /// <summary>
        /// Try to concatenates the strings of the provided array, using the specified separator between each string,
        /// then appends the result followed by the default line terminator to the current instance of the string builder.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings
        /// only if <paramref name="value"/> has more than one element.</param>
        /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendLineJoin(this StringBuilder builder, string separator, params string[] value) =>
            value?.Length > 0
                ? builder.AppendLine(string.Join(separator, value.Where(x => !string.IsNullOrWhiteSpace(x))))
                : builder;

        /// <summary>
        /// Appends the string returned by processing a composite format string followed by the default line terminator, which contains zero or more
        /// format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/>
        /// is replaced by the string representation of the corresponding object argument.</returns>
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object[] args) =>
            builder.AppendFormat(format, args).AppendLine();

        /// <summary>
        /// Try to appends the string returned by processing a composite format string followed by the default line terminator, which contains zero or more
        /// format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="value">The string to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/>
        /// is replaced by the string representation of the corresponding object argument.</returns>
        public static StringBuilder TryAppendLineFormat(this StringBuilder builder, string format, string value) =>
            !string.IsNullOrWhiteSpace(format) && !string.IsNullOrWhiteSpace(value)
                ? builder.AppendLineFormat(format, value)
                : builder;

        public static async Task<IBuffer> GetBufferAsync(this IRandomAccessStream randomStream)
        {
            using (Stream stream = randomStream.GetInputStreamAt(0).AsStreamForRead())
            {
                return await stream.GetBufferAsync();
            }
        }

        public static async Task<byte[]> GetBytesAsync(this IRandomAccessStream randomStream)
        {
            using (Stream stream = randomStream.GetInputStreamAt(0).AsStreamForRead())
            {
                return await stream.GetBytesAsync();
            }
        }

        public static async Task<IBuffer> GetBufferAsync(this Stream stream)
        {
            byte[] bytes = stream != null ? await stream.GetBytesAsync() : Array.Empty<byte>();
            return bytes.AsBuffer();
        }

        public static async Task<byte[]> GetBytesAsync(this Stream stream)
        {
            if (stream.CanSeek) // stream.Length 已确定
            {
                byte[] bytes = new byte[stream.Length];
                _ = await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            else // stream.Length 不确定
            {
                int initialLength = 32768; // 32k

                byte[] buffer = new byte[initialLength];
                int read = 0;

                int chunk;
                while ((chunk = await stream.ReadAsync(buffer, read, buffer.Length - read).ConfigureAwait(false)) > 0)
                {
                    read += chunk;

                    if (read == buffer.Length)
                    {
                        int nextByte = stream.ReadByte();

                        if (nextByte == -1)
                        {
                            return buffer;
                        }

                        byte[] newBuffer = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }
                }

                byte[] ret = new byte[read];
                Array.Copy(buffer, ret, read);
                return ret;
            }
        }

        public static Stream GetStream(this byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
    }
}
