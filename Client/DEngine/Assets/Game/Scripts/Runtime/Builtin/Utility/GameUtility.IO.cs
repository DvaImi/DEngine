// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-26 16:41:12
// 版 本：1.0
// ========================================================

using System;
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
            #region 文件

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

            public static void Delete(string directoryPath, string searchPattern)
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
                var destPath = Path.HasExtension(filePath) ? $"{dirPath}/{newName}{Path.GetExtension(filePath)}" : $"{dirPath}/{newName}";
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
            /// <param name="directoryPath">要清理的文件夹路径</param>
            public static void ClearFolder(string directoryPath)
            {
                if (Directory.Exists(directoryPath) == false)
                {
                    return;
                }

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
                using var md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(fs);
                string result = String.MD5BytesToString(bytes);
                return result;
            }

            /// <summary>
            /// 创建文件所在的目录
            /// </summary>
            /// <param name="filePath">文件路径</param>
            public static void CreateFileDirectory(string filePath)
            {
                string destDirectory = Path.GetDirectoryName(filePath);
                CreateDirectory(destDirectory);
            }

            /// <summary>
            /// 创建文件夹
            /// </summary>
            public static bool CreateDirectory(string directory)
            {
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// 删除文件夹及子目录
            /// </summary>
            public static bool DeleteDirectory(string directory)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// 拷贝文件夹
            /// 注意：包括所有子目录的文件
            /// </summary>
            public static void CopyDirectory(string sourcePath, string destPath)
            {
                sourcePath = GetRegularPath(sourcePath);

                // If the destination directory doesn't exist, create it.
                if (Directory.Exists(destPath) == false)
                    Directory.CreateDirectory(destPath);

                string[] fileList = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                foreach (string file in fileList)
                {
                    string temp = GetRegularPath(file);
                    string savePath = temp.Replace(sourcePath, destPath);
                    CopyFile(file, savePath, true);
                }
            }

            /// <summary>
            /// 拷贝文件
            /// </summary>
            public static void CopyFile(string sourcePath, string destPath, bool overwrite)
            {
                if (File.Exists(sourcePath) == false)
                    throw new FileNotFoundException(sourcePath);

                // 创建目录
                CreateFileDirectory(destPath);

                // 复制文件
                File.Copy(sourcePath, destPath, overwrite);
            }

            /// <summary>
            /// 检测AssetBundle文件是否合法
            /// </summary>
            public static bool CheckBundleFileValid(byte[] fileData)
            {
                string signature = ReadStringToNull(fileData, 20);
                return signature is "UnityFS" or "UnityRaw" or "UnityWeb" or "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA";
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="folderPath"></param>
            /// <returns></returns>
            public static long CalculateFolderSize(string folderPath)
            {
                DirectoryInfo directoryInfo = new(folderPath);
                FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

                long totalSize = 0;

                foreach (FileInfo fileInfo in fileInfos)
                {
                    totalSize += fileInfo.Length;
                }

                return totalSize;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="filePath"></param>
            /// <returns></returns>
            public static long CalculateFileSize(string filePath)
            {
                FileInfo fileInfo = new(filePath);
                return fileInfo.Length;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="maxLength"></param>
            /// <returns></returns>
            public static string ReadStringToNull(byte[] data, int maxLength)
            {
                List<byte> bytes = new List<byte>();
                for (int i = 0; i < data.Length; i++)
                {
                    if (i >= maxLength)
                    {
                        break;
                    }

                    byte bt = data[i];
                    if (bt == 0)
                    {
                        break;
                    }

                    bytes.Add(bt);
                }

                return bytes.Count == 0 ? string.Empty : Encoding.UTF8.GetString(bytes.ToArray());
            }

            #endregion

            #region 路径

            /// <summary>
            /// 获取规范的路径
            /// </summary>
            public static string GetRegularPath(string path)
            {
                return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
            }

            /// <summary>
            /// 移除路径里的后缀名
            /// </summary>
            public static string RemoveExtension(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return str;

                int index = str.LastIndexOf('.');
                if (index == -1)
                    return str;
                else
                    return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
            }

            /// <summary>
            /// 获取项目工程路径
            /// </summary>
            public static string GetProjectPath()
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                return GetRegularPath(projectPath);
            }

            /// <summary>
            /// 转换文件的绝对路径为Unity资源路径
            /// 例如 D:\\Project\\Assets\\Works\\file.txt 替换为 Assets/Works/file.txt
            /// </summary>
            public static string AbsolutePathToAssetPath(string absolutePath)
            {
                string content = GetRegularPath(absolutePath);
                return Substring(content, "Assets/", true);
            }

            /// <summary>
            /// 转换文件的绝对路径为Unity 项目的相对路径
            /// </summary>
            /// <param name="absolutePath"></param>
            /// <returns></returns>
            public static string AbsolutePathToProject(string absolutePath)
            {
                string projectPath = Application.dataPath.Replace("/Assets", "");

                absolutePath = absolutePath.Replace("\\", "/");
                projectPath = projectPath.Replace("\\", "/");

                try
                {
                    var projectUri = new Uri(projectPath + "/");
                    var absoluteUri = new Uri(absolutePath);
                    var relativeUri = projectUri.MakeRelativeUri(absoluteUri);
                    return Uri.UnescapeDataString(relativeUri.ToString().Replace("/", "/"));
                }
                catch (UriFormatException)
                {
                    Debug.LogWarning("Invalid path format.");
                    return null;
                }
            }

            /// <summary>
            /// 转换Unity资源路径为文件的绝对路径
            /// 例如：Assets/Works/file.txt 替换为 D:\\YourProject/Assets/Works/file.txt
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
                        return fullPath;

                    string result = FindFolder(fullPath, folderName);
                    if (string.IsNullOrEmpty(result) == false)
                        return result;
                }

                return string.Empty;
            }

            /// <summary>
            /// 截取字符串
            /// 获取匹配到的后面内容
            /// </summary>
            /// <param name="content">内容</param>
            /// <param name="key">关键字</param>
            /// <param name="includeKey">分割的结果里是否包含关键字</param>
            /// <param name="firstMatch"></param>
            public static string Substring(string content, string key, bool includeKey, bool firstMatch = true)
            {
                if (string.IsNullOrEmpty(key))
                {
                    return content;
                }

                var startIndex = firstMatch
                    ? content.IndexOf(key, StringComparison.Ordinal)      //返回子字符串第一次出现位置		
                    : content.LastIndexOf(key, StringComparison.Ordinal); //返回子字符串最后出现的位置

                // 如果没有找到匹配的关键字
                if (startIndex == -1)
                {
                    return content;
                }

                return includeKey ? content[startIndex..] : content[(startIndex + key.Length)..];
            }

            #endregion
        }
    }
}