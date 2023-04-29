// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-24 20:29:42
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using GameFramework;

namespace GeminiLion
{
    [Serializable]
    public class AssetInfoMap
    {
        /// <summary>
        /// 存储资源路径映射
        /// </summary>
        public Dictionary<string, HashSet<string>> AssetPathMap { get; set; } = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> AssetSuffixMap { get; set; } = new Dictionary<string, HashSet<string>>();

        public string ToJson()
        {
            return Utility.Json.ToJson(this);
        }
    }

}