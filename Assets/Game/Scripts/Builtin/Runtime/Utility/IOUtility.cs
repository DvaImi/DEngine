// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-26 16:41:12
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using UnityEngine;

namespace Game
{
    public static class IOUtility
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void SaveFileSafe(string filePath, string text)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            SaveFileSafe(path, fileName, text);
        }

        public static void SaveFileSafe(string filePath, byte[] binary)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            SaveFileSafe(path, fileName, binary);
        }

        public static void SaveFileSafe(string path, string fileName, string text)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("File name is null.");
                return;
            }

            CreateDirectoryIfNotExists(path);

            string filePath = Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new UTF8Encoding(false);
                using (StreamWriter writer = new StreamWriter(stream, utf8Encoding))
                {
                    writer.Write(text);
                }
            }
        }

        public static void SaveFileSafe(string path, string fileName, byte[] binary)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("File name is null.");
                return;
            }

            CreateDirectoryIfNotExists(path);

            string filePath = Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(binary);
                }
            }
        }

        public static List<FileInfo> GetFilesWithExtension(string directoryPath, string fileExtension)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
            List<FileInfo> result = new List<FileInfo>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                result.Add(fileInfos[i]);
            }
            return result;
        }

        /// <summary>
        /// 删除指定目录下的文件以及文件夹
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void Delete(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning("Path is invalid ");
                return;
            }

            DirectoryInfo directoryInfo = new(directoryPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

            foreach (var file in fileInfos)
            {
                file.Delete();
            }

            foreach (var item in directoryInfos)
            {
                item.Delete(true);
            }
        }

        public static void Delete(string directoryPath, string searchPattern = "*")
        {
            Utility.Path.RemoveEmptyDirectory(directoryPath);
            string[] fileNames = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                File.Delete(fileName);
            }
        }
    }
}
