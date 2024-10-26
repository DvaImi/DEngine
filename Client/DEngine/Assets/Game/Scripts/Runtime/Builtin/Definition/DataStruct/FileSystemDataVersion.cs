using System;
using System.Collections.Generic;
using DEngine.Runtime;
using Newtonsoft.Json;

namespace Game.FileSystem
{
    [Serializable]
    public class FileSystemDataVersion
    {
        /// <summary>
        /// 资源文件系统
        /// </summary>
        public string FileSystem { get; set; }

        /// <summary>
        /// 资源文件偏移和长度
        /// </summary>
        public Dictionary<string, FileInfo> FileInfos { get; set; } = new();

        public static FileSystemDataVersion Deserialize(byte[] data)
        {
            if (data == null)
            {
                Log.Error("data is invalid");
                return null;
            }

            return GameUtility.Bson.FormBson<FileSystemDataVersion>(data);
        }

        public static byte[] ToBson(FileSystemDataVersion data)
        {
            if (data == null)
            {
                Log.Error("data is invalid");
                return null;
            }

            return GameUtility.Bson.ToBson(data);
        }

        public static FileSystemDataVersion Deserialize(string json)
        {
            if (json == null)
            {
                Log.Error("json is invalid");
                return null;
            }

            return JsonConvert.DeserializeObject<FileSystemDataVersion>(json);
        }

        public static string ToJson(FileSystemDataVersion data)
        {
            if (data == null)
            {
                Log.Error("data is invalid");
                return null;
            }

            return JsonConvert.SerializeObject(data);
        }
    }

    /// <summary>
    /// 文件信息
    /// </summary>
    [Serializable]
    public class FileInfo
    {
        public FileInfo(long offset, int length, int hashCode)
        {
            Offset = offset;
            Length = length;
            HashCode = hashCode;
        }

        /// <summary>
        /// 文件偏移
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 资源哈希值
        /// </summary>
        public int HashCode { get; set; }
    }
}