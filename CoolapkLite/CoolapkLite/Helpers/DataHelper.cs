using CoolapkLite.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace CoolapkLite.Helpers
{
    public static partial class DataHelper
    {
        public static string GetMD5(this string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            using (MD5 md5Hasher = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                string results = BitConverter.ToString(data).ToLowerInvariant();
                return results.Replace("-", "");
            }
        }

        public static string GetBase64(this string input, bool isRaw = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);
            if (!isRaw) { result = result.Replace("=", ""); }
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
                string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
                //链接彻底删除！
                while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
                {
                    s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                    s = s.Replace("</a>", "");
                }
                return s;
            }
        }

        public static string ConvertJsonString(this string str)
        {
            //格式化 json 字符串
            JsonSerializer serializer = new JsonSerializer();
            using (TextReader textReader = new StringReader(str))
            using (JsonTextReader jtr = new JsonTextReader(textReader))
            {
                object obj = null;
                try { obj = serializer.Deserialize(jtr); } catch { }
                if (obj != null)
                {
                    using (StringWriter textWriter = new StringWriter())
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    })
                    {
                        serializer.Serialize(jsonWriter, obj);
                        return textWriter.ToString();
                    }
                }
                else
                {
                    return str;
                }
            }
        }

#if !NETCORE463
        /// <summary>
        /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string,
        /// or if <paramref name="value"/> is the empty string (""); otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this string text, string value, StringComparison comparisonType)
        {
            return text.IndexOf(value, comparisonType) != -1;
        }
#endif

        /// <summary>
        /// Try to concatenates the strings of the provided array, using the specified separator between each string,
        /// then appends the result to the current instance of the string builder.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings
        /// only if <paramref name="value"/> has more than one element.</param>
        /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendJoin(this StringBuilder builder, string separator, params string[] values)
        {
            if (values?.Any() == true)
            {
                return builder.Append(string.Join(separator, values.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            return builder;
        }

        /// <summary>
        /// Try to appends a copy of the specified string followed by the default line terminator to the end of the current StringBuilder object.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendLine(this StringBuilder builder, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return builder.AppendLine(value);
            }
            return builder;
        }

        /// <summary>
        /// Try to concatenates the strings of the provided array, using the specified separator between each string,
        /// then appends the result followed by the default line terminator to the current instance of the string builder.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings
        /// only if <paramref name="value"/> has more than one element.</param>
        /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder TryAppendLineJoin(this StringBuilder builder, string separator, params string[] value)
        {
            if (value?.Any() == true)
            {
                return builder.AppendLine(string.Join(separator, value.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            return builder;
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string followed by the default line terminator, which contains zero or more
        /// format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/>
        /// is replaced by the string representation of the corresponding object argument.</returns>
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object[] args)
        {
            return builder.AppendFormat(format, args).AppendLine();
        }

        /// <summary>
        /// Try to appends the string returned by processing a composite format string followed by the default line terminator, which contains zero or more
        /// format items, to this instance. Each format item is replaced by the string representation of a corresponding argument in a parameter array.
        /// </summary>
        /// <param name="builder">The builder to append.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="value">The string to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/>
        /// is replaced by the string representation of the corresponding object argument.</returns>
        public static StringBuilder TryAppendLineFormat(this StringBuilder builder, string format, string value)
        {
            if (!string.IsNullOrWhiteSpace(format) && !string.IsNullOrWhiteSpace(value))
            {
                return builder.AppendLineFormat(format, value);
            }
            return builder;
        }

        public static IBuffer GetBuffer(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBuffer();
            }
        }

        public static byte[] GetBytes(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBytes();
            }
        }

        public static IBuffer GetBuffer(this Stream stream)
        {
            byte[] bytes = new byte[0];
            if (stream != null)
            {
                bytes = stream.GetBytes();
            }
            return bytes.AsBuffer();
        }

        public static byte[] GetBytes(this Stream stream)
        {
            if (stream.CanSeek) // stream.Length 已确定
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            else // stream.Length 不确定
            {
                int initialLength = 32768; // 32k

                byte[] buffer = new byte[initialLength];
                int read = 0;

                int chunk;
                while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
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
