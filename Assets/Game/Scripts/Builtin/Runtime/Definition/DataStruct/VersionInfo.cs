//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Game
{
    public class VersionInfo : IEquatable<VersionInfo>
    {
        /// <summary>
        /// 是否需要强制更新游戏应用
        /// </summary>
        public bool ForceUpdateGame
        {
            get;
            set;
        }

        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 最新的游戏内部版本号
        /// </summary>
        public int InternalGameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 最新的资源内部版本号
        /// </summary>
        public int InternalResourceVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri
        {
            get;
            set;
        }

        /// <summary>
        /// 资源版本列表长度
        /// </summary>
        public int VersionListLength
        {
            get;
            set;
        }

        /// <summary>
        /// 资源版本列表哈希值
        /// </summary>
        public int VersionListHashCode
        {
            get;
            set;
        }

        /// <summary>
        /// 资源版本列表压缩后长度
        /// </summary>
        public int VersionListCompressedLength
        {
            get;
            set;
        }

        /// <summary>
        /// 资源版本列表压缩后哈希值
        /// </summary>
        public int VersionListCompressedHashCode
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as VersionInfo);
        }

        public bool Equals(VersionInfo other)
        {
            return other is not null &&
                   LatestGameVersion == other.LatestGameVersion &&
                   InternalGameVersion == other.InternalGameVersion &&
                   InternalResourceVersion == other.InternalResourceVersion &&
                   UpdatePrefixUri == other.UpdatePrefixUri &&
                   VersionListLength == other.VersionListLength &&
                   VersionListHashCode == other.VersionListHashCode &&
                   VersionListCompressedLength == other.VersionListCompressedLength &&
                   VersionListCompressedHashCode == other.VersionListCompressedHashCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LatestGameVersion, InternalGameVersion, InternalResourceVersion, UpdatePrefixUri, VersionListLength, VersionListHashCode, VersionListCompressedLength, VersionListCompressedHashCode);
        }

        public static bool operator ==(VersionInfo left, VersionInfo right)
        {
            return EqualityComparer<VersionInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(VersionInfo left, VersionInfo right)
        {
            return !(left == right);
        }
    }
}
