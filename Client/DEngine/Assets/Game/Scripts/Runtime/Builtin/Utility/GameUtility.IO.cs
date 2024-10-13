// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-26 16:41:12
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DEngine;
using UnityEngine;

namespace Game
{
    public static partial class GameUtility
    {
        public static class IO
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

            public static string[] GetFileNamesWithExtension(string directoryPath, string fileExtension)
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                var fileInfos = directoryInfo.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
                var result = new string[fileInfos.Length];
                for (var i = 0; i < fileInfos.Length; i++)
                {
                    result[i] = fileInfos[i].FullName;
                }

                return result;
            }

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

            public static void CopyFiles(string sourceDir, string targetDir, string excludeDir)
            {
                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    if (dirPath.Contains(excludeDir))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));
                }

                foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    if (filePath.Contains(excludeDir))
                    {
                        continue;
                    }

                    File.Copy(filePath, filePath.Replace(sourceDir, targetDir), true);
                }
            }

            /// <summary>
            /// 文件重命名
            /// </summary>
            public static void FileRename(string filePath, string newName)
            {
                string dirPath = Path.GetDirectoryName(filePath);
                string destPath;
                if (Path.HasExtension(filePath))
                {
                    string extentsion = Path.GetExtension(filePath);
                    destPath = $"{dirPath}/{newName}{extentsion}";
                }
                else
                {
                    destPath = $"{dirPath}/{newName}";
                }

                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.MoveTo(destPath);
            }

            /// <summary>
            /// 移动文件
            /// </summary>
            public static void MoveFile(string filePath, string destPath)
            {
                if (File.Exists(destPath))
                    File.Delete(destPath);

                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.MoveTo(destPath);
            }

            /// <summary>
            /// 清空文件夹
            /// </summary>
            /// <param name="folderPath">要清理的文件夹路径</param>
            public static void ClearFolder(string directoryPath)
            {
                if (Directory.Exists(directoryPath) == false)
                    return;

                // 删除文件
                string[] allFiles = Directory.GetFiles(directoryPath);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    File.Delete(allFiles[i]);
                }

                // 删除文件夹
                string[] allFolders = Directory.GetDirectories(directoryPath);
                for (int i = 0; i < allFolders.Length; i++)
                {
                    Directory.Delete(allFolders[i], true);
                }
            }

            /// <summary>
            /// 获取文件字节大小
            /// </summary>
            public static long GetFileSize(string filePath)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }

            /// <summary>
            /// 读取文件的所有文本内容
            /// </summary>
            public static string ReadFileAllText(string filePath)
            {
                if (File.Exists(filePath) == false)
                    return string.Empty;

                return File.ReadAllText(filePath, Encoding.UTF8);
            }

            /// <summary>
            /// 读取文本的所有文本内容
            /// </summary>
            public static string[] ReadFileAllLine(string filePath)
            {
                if (File.Exists(filePath) == false)
                    return null;

                return File.ReadAllLines(filePath, Encoding.UTF8);
            }

            /// <summary>
            /// 获取项目工程路径
            /// </summary>
            public static string GetProjectPath()
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                return Utility.Path.GetRegularPath(projectPath);
            }

            /// <summary>
            /// 转换文件的绝对路径为Unity资源路径
            /// 例如 D:\\YourPorject\\Assets\\Works\\file.txt 替换为 Assets/Works/file.txt
            /// </summary>
            public static string AbsolutePathToAssetPath(string absolutePath)
            {
                string content = Utility.Path.GetRegularPath(absolutePath);
                return String.Substring(content, "Assets/", true);
            }

            /// <summary>
            /// 转换Unity资源路径为文件的绝对路径
            /// 例如：Assets/Works/file.txt 替换为 D:\\YourPorject/Assets/Works/file.txt
            /// </summary>
            public static string AssetPathToAbsolutePath(string assetPath)
            {
                string projectPath = GetProjectPath();
                return $"{projectPath}/{assetPath}";
            }

            /// <summary>
            /// 递归查找目标文件夹路径
            /// </summary>
            /// <param name="root">搜索的根目录</param>
            /// <param name="folderName">目标文件夹名称</param>
            /// <returns>返回找到的文件夹路径，如果没有找到返回空字符串</returns>
            public static string FindFolder(string root, string folderName)
            {
                DirectoryInfo rootInfo = new DirectoryInfo(root);
                DirectoryInfo[] infoList = rootInfo.GetDirectories();
                for (int i = 0; i < infoList.Length; i++)
                {
                    string fullPath = infoList[i].FullName;
                    if (infoList[i].Name == folderName)
                    {
                        return fullPath;
                    }

                    string result = FindFolder(fullPath, folderName);
                    if (string.IsNullOrEmpty(result) == false)
                    {
                        return result;
                    }
                }

                return string.Empty;
            }

            /// <summary>
            /// 获取文件MD5
            /// </summary>
            public static string GetFileMD5(string filePath)
            {
                using FileStream fs = new FileStream(filePath, FileMode.Open);
                return GetFileMD5(fs);
            }

            /// <summary>
            /// 获取文件MD5
            /// </summary>
            public static string GetFileMD5(FileStream fs)
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] bytes = md5.ComputeHash(fs);
                    string result = MD5BytesToString(bytes);
                    return result;
                }
            }

            /// <summary>
            /// 获取字节数组MD5
            /// </summary>
            public static string GetBytesMD5(byte[] buffer, int offset, int count)
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    byte[] bytes = md5.ComputeHash(buffer, offset, count);
                    string result = MD5BytesToString(bytes);
                    return result;
                }
            }

            /// <summary>
            /// MD5字节数组转换为字符串
            /// </summary>
            private static string MD5BytesToString(byte[] bytes)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                string result = stringBuilder.ToString();
                return result;
            }
        }
    }
}