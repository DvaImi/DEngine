using System;

namespace Game
{
    [Serializable]
    public class VersionInfo
    {
        /// <summary>
        /// 是否需要强制更新游戏应用
        /// </summary>
        public bool ForceUpdateGame { get; set; }

        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion { get; set; }

        /// <summary>
        /// 最新的游戏内部版本号
        /// </summary>
        public int InternalGameVersion { get; set; }

        /// <summary>
        /// 最新的资源内部版本号
        /// </summary>
        public int InternalResourceVersion { get; set; }

        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri { get; set; }

        /// <summary>
        /// 更新模式，True 表示整包更新（压缩包模式），False 表示分包更新（独立资源模式）
        /// </summary>
        public bool IsCompressedMode { get; set; } = false;

        /// <summary>
        /// 分包更新信息（独立资源模式）
        /// </summary>
        public ResourceVersionInfo ResourceVersionInfo { get; set; } = new();

        /// <summary>
        /// 资源包信息
        /// </summary>
        public ResourcePackInfo ResourcePackInfo { get; set; } = new();

        /// <summary>
        /// 更新描述
        /// </summary>
        public string UpdateDescription { get; set; }
    }
}