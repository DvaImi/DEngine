using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class BuildPipeline
    {
        /// <summary>
        /// 提取差异包
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="lastFullVersionOutputFullPath"></param>
        public static void OutputDifferenceBundles(Platform platform)
        {
            //获取上次整包的资源列表
            GetLastFullBuildPath(out string lastFullVersionOutputFullPath);
            //获取当前版本的资源列表
            GetLastBuildPath(out string currentVersion, out string currentVersionFullPath);

            UpdatableVersionListSerializer updatableVersionListSerializer = new UpdatableVersionListSerializer();
            updatableVersionListSerializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V0);
            updatableVersionListSerializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V1);
            updatableVersionListSerializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V2);

            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionFullPath, GetPlatformPath(platform)));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("DEngineVersion.*.block", SearchOption.TopDirectoryOnly);
            byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);
            sourceVersionListBytes = Utility.Compression.Decompress(sourceVersionListBytes);

            UpdatableVersionList sourceUpdatableVersionList = default;
            using (Stream stream = new MemoryStream(sourceVersionListBytes))
            {
                sourceUpdatableVersionList = updatableVersionListSerializer.Deserialize(stream);
            }
            List<string> updatableVersionList = new List<string>();

            UpdatableVersionList.Resource[] array = sourceUpdatableVersionList.GetResources();
            int index = 0;
            int count = array.Length;
            for (int i = 0; i < array.Length; i++)
            {
                index = i;
                EditorUtility.DisplayProgressBar("Analyzing Difference", Utility.Text.Format("Analyzing Difference, {0}/{1} analyzed.", index, count), (float)index / count);
                UpdatableVersionList.Resource resource = array[i];
                string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                updatableVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
            }

            //特殊字符需要转义
            string pattern = @"DEngineVersion\..*\.block";
            Regex regex = new Regex(pattern);
            string[] lastFullVersionFiles = Directory.GetFiles(lastFullVersionOutputFullPath, "*", SearchOption.AllDirectories);
            //过滤版本文件
            string[] filteredFiles = lastFullVersionFiles.Where(file => !regex.IsMatch(file)).ToArray();
            index = 0;
            count = filteredFiles.Length;
            for (int i = 0; i < filteredFiles.Length; i++)
            {
                string lastFileFullName = filteredFiles[i];
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFileFullName);
                if (File.Exists(lastFileFullName))
                {
                    string fileNameWithoutCrcHashCode = fileNameWithoutExtension[..fileNameWithoutExtension.IndexOf(".")];
                    if (updatableVersionList.Contains(fileNameWithoutCrcHashCode))
                    {
                        index = i;
                        EditorUtility.DisplayProgressBar("Copy Difference", Utility.Text.Format("Copy Difference, {0}/{1} copy.", index, count), (float)index / count);
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(currentVersionFullPath, lastFileFullName[lastFullVersionOutputFullPath.Length..]));
                        FileInfo destFileInfo = new(destFileName);
                        if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                        {
                            destFileInfo.Directory.Create();
                        }

                        File.Copy(lastFileFullName, destFileName, true);
                    }
                }
            }

            Debug.Log("Copy difference success.");
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 判断是否可以进行差异化打包
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static bool CanDifference()
        {
            GetLastFullBuildPath(out string lastFullVersionOutputFullPath);
            return !string.IsNullOrEmpty(lastFullVersionOutputFullPath) && Directory.Exists(lastFullVersionOutputFullPath);
        }
    }
}