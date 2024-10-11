// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 22:34:00
// 版 本：1.0
// ========================================================

using System;
using System.Security.Cryptography;
using System.Text;
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

            public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return content;
                }

                var startIndex = firstMatch ? content.IndexOf(key, StringComparison.Ordinal) : content.LastIndexOf(key, StringComparison.Ordinal);
                return startIndex == -1 ? content : includeKey ? content[startIndex..] : content[(startIndex + key.Length)..];
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
        }
    }
}