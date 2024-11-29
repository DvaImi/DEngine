// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 22:34:00
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;
using DEngine;

namespace Game
{
    public static partial class GameUtility
    {
        public static class String
        {
            public static string GetByteLengthString(long byteLength)
            {
                if (byteLength < 1024L) // 2 ^ 10
                {
                    return Utility.Text.Format("{0} Bytes", byteLength);
                }

                if (byteLength < 1048576L) // 2 ^ 20
                {
                    return Utility.Text.Format("{0:F2} KB", byteLength / 1024f);
                }

                if (byteLength < 1073741824L) // 2 ^ 30
                {
                    return Utility.Text.Format("{0:F2} MB", byteLength / 1048576f);
                }

                if (byteLength < 1099511627776L) // 2 ^ 40
                {
                    return Utility.Text.Format("{0:F2} GB", byteLength / 1073741824f);
                }

                if (byteLength < 1125899906842624L) // 2 ^ 50
                {
                    return Utility.Text.Format("{0:F2} TB", byteLength / 1099511627776f);
                }

                if (byteLength < 1152921504606846976L) // 2 ^ 60
                {
                    return Utility.Text.Format("{0:F2} PB", byteLength / 1125899906842624f);
                }

                return Utility.Text.Format("{0:F2} EB", byteLength / 1152921504606846976f);
            }

            /// <summary>
            /// 获取字节数组MD5
            /// </summary>
            public static string GetBytesMD5(byte[] buffer, int offset, int count)
            {
                using var md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(buffer, offset, count);
                string result = MD5BytesToString(bytes);
                return result;
            }

            /// <summary>
            /// MD5字节数组转换为字符串
            /// </summary>
            public static string MD5BytesToString(byte[] bytes)
            {
                var stringBuilder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                string result = stringBuilder.ToString();
                return result;
            }

            public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return content;
                }

                var startIndex = firstMatch ? content.IndexOf(key, StringComparison.Ordinal) : content.LastIndexOf(key, StringComparison.Ordinal);
                return startIndex == -1 ? content : includeKey ? content[startIndex..] : content[(startIndex + key.Length)..];
            }

            public static string RemoveFirstChar(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return str;
                return str.Substring(1);
            }

            public static string RemoveLastChar(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return str;
                return str.Substring(0, str.Length - 1);
            }

            public static List<string> StringToStringList(string str, char separator)
            {
                var result = new List<string>();
                if (!string.IsNullOrEmpty(str))
                {
                    string[] splits = str.Split(separator);
                    result.AddRange(splits.Select(split => split.Trim()).Where(value => !string.IsNullOrEmpty(value)));
                }

                return result;
            }

            public static T NameToEnum<T>(string name)
            {
                if (Enum.IsDefined(typeof(T), name) == false)
                {
                    throw new ArgumentException($"Enum {typeof(T)} is not defined name {name}");
                }

                return (T)Enum.Parse(typeof(T), name);
            }

            public static string GetHashString(string input)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }

            public static string GetColor(ColorType colorType)
            {
                int color = (int)colorType;
                string colorString = Convert.ToString(color, 16);
                while (colorString.Length < 6)
                {
                    colorString = "0" + colorString;
                }

                return colorString;
            }

            public static string Concat(params string[] values)
            {
                return ZString.Concat(values);
            }

            public static string Concat<T>(params T[] values)
            {
                return ZString.Concat(values);
            }

            public static string CollectionToString<T>(IEnumerable<T> collection)
            {
                return Utility.Text.Format("[{0}]", ZString.Join(",", collection));
            }

            public static string CollectionToString<TK, TV>(IDictionary<TK, TV> collection)
            {
                using var sb = ZString.CreateStringBuilder();
                sb.Append('{');
                foreach (var e in collection)
                {
                    sb.Append(e.Key);
                    sb.Append(':');
                    sb.Append(e.Value);
                    sb.Append(',');
                }

                sb.Append('}');
                return sb.ToString();
            }
        }
    }
}