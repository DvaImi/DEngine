using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        /// <summary>
        /// 获取最新的资源构建列表
        /// </summary>
        /// <param name="lastFull">是否是最新的整包构建列表</param>
        /// <param name="fullPath">整包列表</param>
        /// <param name="package">单机资源列表</param>
        /// <param name="packed">为可更新模式生成的本地资源列表</param>
        public static void GetBuildVersions(Platform platform, bool lastFull, out string fullPath, out string package, out string packed)
        {
            string OutputDirectory = GameSetting.Instance.BundlesOutput;
            fullPath = package = packed = null;
            string buildReportDirectory = Path.Combine(OutputDirectory, "BuildReport");
            if (!Directory.Exists(buildReportDirectory))
            {
                return;
            }
            string[] allBuildReport = Directory.GetFiles(buildReportDirectory, "*.xml", SearchOption.AllDirectories);
            int[] lastBuildVersions = new int[allBuildReport.Length];
            for (int i = 0; i < allBuildReport.Length; i++)
            {
                string item = allBuildReport[i];
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(item);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("DEngine");
                XmlNode xmlBuildReport = xmlRoot.SelectSingleNode("BuildReport");
                XmlNode xmlSummary = xmlBuildReport.SelectSingleNode("Summary");
                XmlNode xmlInternalResourceVersion = xmlSummary.SelectSingleNode("InternalResourceVersion");
                XmlNode xmlLastFullBuildVersion = xmlSummary.SelectSingleNode("LastFullBuildVersion");
                lastBuildVersions[i] = lastFull ? int.Parse(xmlLastFullBuildVersion.InnerText) : int.Parse(xmlInternalResourceVersion.InnerText);
            }
            int maxVersion = lastBuildVersions.Max();
            fullPath = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Full/{1}.{2}/{3}/", OutputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
            package = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Package/{1}.{2}/{3}/", OutputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
            packed = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Packed/{1}.{2}/{3}/", OutputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
        }

        /// <summary>
        /// 差异打包后处理
        /// </summary>
        /// <param name="platform"></param>
        public static void OnPostprocessDifference(Platform platform)
        {
            //获取上次整包的资源列表
            GetBuildVersions(platform, true, out string lastFullVersionOutputFullPath, out string lastPackageVersionOutpuPath, out string lastPackedVersionOutputPath);

            //获取当前版本的资源列表
            GetBuildVersions(platform, false, out string currentVersionFullPath, out string currentVersionPackagePath, out string currentVersionPackedPath);

            //特殊字符需要转义
            string pattern = @"DEngineVersion\..*\.block";
            Regex regex = new(pattern);
            string[] lastFullVersionFiles = Directory.GetFiles(lastFullVersionOutputFullPath, "*", SearchOption.AllDirectories);
            string[] lastPackageVersionFiles = Directory.GetFiles(lastPackageVersionOutpuPath, "*", SearchOption.AllDirectories);
            string[] lastPackedVersionFiles = Directory.GetFiles(lastPackedVersionOutputPath, "*", SearchOption.AllDirectories);

            //过滤版本文件
            string[] filteredFullFiles = lastFullVersionFiles.Where(file => !regex.IsMatch(file)).ToArray();
            string[] filteredPackageFiles = lastPackageVersionFiles.Where(file => !regex.IsMatch(file)).ToArray();
            string[] filteredPackedFiles = lastPackedVersionFiles.Where(file => !regex.IsMatch(file)).ToArray();

            CopyUpdatableVersionList(currentVersionFullPath, lastFullVersionOutputFullPath, filteredFullFiles);
            CopyPackageVersionList(currentVersionPackagePath, lastPackageVersionOutpuPath, filteredPackageFiles);
            CopyPackedVersionList(currentVersionPackedPath, lastPackedVersionOutputPath, filteredPackedFiles);

            if (GameSetting.Instance.AutoCopyToFileServer)
            {
                PutToLocalFileServer(platform, currentVersionFullPath);
            }

            int resourceMode = GameSetting.Instance.ResourceModeIndex;
            string sourcePath = resourceMode <= 1 ? currentVersionPackagePath : currentVersionPackedPath;
            CopyFileToStreamingAssets(sourcePath);
            Debug.Log("Difference postprocess complete.");
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 拷贝可更新资源列表
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="currentVersionFullPath"></param>
        /// <param name="lastFullVersionOutputFullPath"></param>
        /// <param name="filteredFullFiles"></param>
        private static void CopyUpdatableVersionList(string currentVersionFullPath, string lastFullVersionOutputFullPath, string[] filteredFullFiles)
        {
            UpdatableVersionListSerializer updatableVersionListSerializer = new UpdatableVersionListSerializer();
            updatableVersionListSerializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V0);
            updatableVersionListSerializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V1);
            updatableVersionListSerializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V2);

            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionFullPath));
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

            foreach (UpdatableVersionList.Resource resource in array)
            {
                string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                updatableVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
            }

            foreach (string lastFileFullName in filteredFullFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFileFullName);
                if (File.Exists(lastFileFullName))
                {
                    string fileNameWithoutCrcHashCode = fileNameWithoutExtension[..fileNameWithoutExtension.IndexOf(".")];
                    if (updatableVersionList.Contains(fileNameWithoutCrcHashCode))
                    {
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
        }

        /// <summary>
        /// 拷贝单机版本资源
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="currentVersionPackagePath"></param>
        /// <param name="lastFullVersionOutputPackagePath"></param>
        /// <param name="filteredPackageFiles"></param>
        private static void CopyPackageVersionList(string currentVersionPackagePath, string lastFullVersionOutputPackagePath, string[] filteredPackageFiles)
        {
            PackageVersionListSerializer packageVersionListSerializer = new PackageVersionListSerializer();
            packageVersionListSerializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V0);
            packageVersionListSerializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V1);
            packageVersionListSerializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V2);

            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackagePath));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("DEngineVersion.block", SearchOption.TopDirectoryOnly);
            byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);

            PackageVersionList sourcePackageVersionList = default;
            using (Stream stream = new MemoryStream(sourceVersionListBytes))
            {
                sourcePackageVersionList = packageVersionListSerializer.Deserialize(stream);
            }

            List<string> packageVersionList = new List<string>();

            PackageVersionList.Resource[] array = sourcePackageVersionList.GetResources();

            foreach (PackageVersionList.Resource resource in array)
            {
                string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                packageVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
            }

            foreach (string lastFilePackageName in filteredPackageFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFilePackageName);
                if (File.Exists(lastFilePackageName))
                {
                    if (packageVersionList.Contains(fileNameWithoutExtension))
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackagePath, lastFilePackageName[lastFullVersionOutputPackagePath.Length..]));
                        FileInfo destFileInfo = new(destFileName);
                        if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                        {
                            destFileInfo.Directory.Create();
                        }

                        File.Copy(lastFilePackageName, destFileName, true);
                    }
                }
            }
        }

        /// <summary>
        /// 拷贝可更新模式本地只读资源
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="currentVersionPackedPath"></param>
        /// <param name="lastFullVersionOutputPackedPath"></param>
        /// <param name="filteredPackedFiles"></param>
        private static void CopyPackedVersionList(string currentVersionPackedPath, string lastFullVersionOutputPackedPath, string[] filteredPackedFiles)
        {
            ReadOnlyVersionListSerializer serializer = new ReadOnlyVersionListSerializer();
            serializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V0);
            serializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V1);
            serializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V2);
            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackedPath));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("DEngineList.block", SearchOption.TopDirectoryOnly);
            byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);
            LocalVersionList versionList = default;

            using (Stream stream = new MemoryStream(sourceVersionListBytes))
            {
                versionList = serializer.Deserialize(stream);
            }

            List<string> readOnlyVersionList = new List<string>();

            LocalVersionList.Resource[] resources = versionList.GetResources();

            foreach (LocalVersionList.Resource resource in resources)
            {
                string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                readOnlyVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
            }

            foreach (string lastFilePackedName in filteredPackedFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFilePackedName);
                if (File.Exists(lastFilePackedName))
                {
                    if (readOnlyVersionList.Contains(fileNameWithoutExtension))
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackedPath, lastFilePackedName[lastFullVersionOutputPackedPath.Length..]));
                        FileInfo destFileInfo = new(destFileName);
                        if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                        {
                            destFileInfo.Directory.Create();
                        }

                        File.Copy(lastFilePackedName, destFileName, true);
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否可以进行差异化打包
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static bool CanDifference()
        {
            Platform platform = GetPlatform(GameSetting.Instance.BuildPlatform);
            GetBuildVersions(platform, true, out string lastFullVersionOutputFullPath, out string lastPackageVersionOutputFullPath, out string lastPackedVersionOutputFullPath);
            return !string.IsNullOrEmpty(lastFullVersionOutputFullPath)
                   && Directory.Exists(lastFullVersionOutputFullPath)
                   && !string.IsNullOrEmpty(lastPackageVersionOutputFullPath)
                   && Directory.Exists(lastPackageVersionOutputFullPath)
                   && !string.IsNullOrEmpty(lastPackedVersionOutputFullPath)
                   && Directory.Exists(lastPackedVersionOutputFullPath);
        }
    }
}