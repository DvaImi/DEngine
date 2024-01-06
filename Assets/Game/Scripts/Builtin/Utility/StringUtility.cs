// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 22:34:00
// 版 本：1.0
// ========================================================

using DEngine;
using DEngine.Localization;

namespace Game
{
    public static class StringUtility
    {
        public static string GetVariant()
        {
            string currentVariant;
            switch (GameEntry.Localization.Language)
            {
                case Language.English:
                    currentVariant = "en-us";
                    break;

                case Language.ChineseSimplified:
                    currentVariant = "zh-cn";
                    break;

                case Language.ChineseTraditional:
                    currentVariant = "zh-tw";
                    break;

                case Language.Korean:
                    currentVariant = "ko-kr";
                    break;

                default:
                    currentVariant = "zh-cn";
                    break;
            }
            return currentVariant;
        }

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
        /// 截取字符串
        /// 获取匹配到的后面内容
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="key">关键字</param>
        /// <param name="includeKey">分割的结果里是否包含关键字</param>
        /// <param name="searchBegin">是否使用初始匹配的位置，否则使用末尾匹配的位置</param>
        public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                return content;
            }

            int startIndex;
            if (firstMatch)
            {
                startIndex = content.IndexOf(key); //返回子字符串第一次出现位置		
            }
            else
            {
                startIndex = content.LastIndexOf(key); //返回子字符串最后出现的位置
            }

            // 如果没有找到匹配的关键字
            return startIndex == -1 ? content : includeKey ? content[startIndex..] : content[(startIndex + key.Length)..];
        }
    }
}