using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static void GetBuildVersions(Platform platform, bool lastFull, out string fullPath, out string package, out string packed, out string patch)
        {
            string OutputDirectory = DEngineSetting.BundlesOutput;
            fullPath = package = packed = patch = null;
            string buildReportDirectory = Path.Combine(OutputDirectory, "BuildReport");
            if (!Directory.Exists(buildReportDirectory))
            {
                return;
            }

            string[] allBuildReport = Directory.GetFiles(buildReportDirectory, "*.xml", SearchOption.AllDirectories);

            if (allBuildReport == null || allBuildReport.Length == 0)
            {
                return;
            }

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
            patch = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Patch/{4}/{1}.{2}/{3}/", OutputDirectory, Application.version, maxVersion, GetPlatformPath(platform), "*patch")).FullName);
        }

        /// <summary>
        /// 差异打包后处理
        /// 合并资源包
        /// </summary>
        /// <param name="platform"></param>
        public static void OnPostprocessDifference(Platform platform)
        {
            //获取上次整包的资源列表
            GetBuildVersions(platform, true, out string lastFullVersionOutputFullPath, out string lastPackageVersionOutpuPath, out string lastPackedVersionOutputPath, out _);

            //获取当前版本的资源列表
            GetBuildVersions(platform, false, out string currentVersionFullPath, out string currentVersionPackagePath, out string currentVersionPackedPath, out string patchVersionPath);

            string[] lastFullVersionFiles = Directory.GetFiles(lastFullVersionOutputFullPath, "*", SearchOption.AllDirectories);
            string[] lastPackageVersionFiles = Directory.GetFiles(lastPackageVersionOutpuPath, "*", SearchOption.AllDirectories);
            string[] lastPackedVersionFiles = Directory.GetFiles(lastPackedVersionOutputPath, "*", SearchOption.AllDirectories);

            MergeFullVersionList(currentVersionFullPath, lastFullVersionOutputFullPath, patchVersionPath.Replace("*patch", "Full"), lastFullVersionFiles);
            MergePackageVersionList(currentVersionPackagePath, lastPackageVersionOutpuPath, patchVersionPath.Replace("*patch", "Package"), lastPackageVersionFiles);
            MergePackedVersionList(currentVersionPackedPath, lastPackedVersionOutputPath, patchVersionPath.Replace("*patch", "Packed"), lastPackedVersionFiles);

            var resourceMode = DEngineSetting.Instance.ResourceMode;

            string sourcePath = currentVersionPackagePath;

            switch (resourceMode)
            {
                case DEngine.Resource.ResourceMode.Unspecified:
                case DEngine.Resource.ResourceMode.Package:
                    break;
                case DEngine.Resource.ResourceMode.Updatable:
                case DEngine.Resource.ResourceMode.UpdatableWhilePlaying:
                    sourcePath = currentVersionPackedPath;
                    break;
                default:
                    sourcePath = currentVersionPackagePath;
                    break;
            }

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
        private static void MergeFullVersionList(string currentVersionFullPath, string lastFullVersionOutputFullPath, string patchVersionPath, string[] filteredFullFiles)
        {
            UpdatableVersionListSerializer updatableVersionListSerializer = new UpdatableVersionListSerializer();
            updatableVersionListSerializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V0);
            updatableVersionListSerializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V1);
            updatableVersionListSerializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.UpdatableVersionListDeserializeCallback_V2);

            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionFullPath));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("RemoteVersionList.*.block", SearchOption.TopDirectoryOnly);
            List<string> updatableVersionList = new List<string>();
            if (sourceVersionListFiles.Length > 0)
            {
                UpdatableVersionList sourceUpdatableVersionList = default;
                byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);
                sourceVersionListBytes = Utility.Compression.Decompress(sourceVersionListBytes);
                using Stream stream = new MemoryStream(sourceVersionListBytes);
                sourceUpdatableVersionList = updatableVersionListSerializer.Deserialize(stream);
                UpdatableVersionList.Resource[] array = sourceUpdatableVersionList.GetResources();

                foreach (UpdatableVersionList.Resource resource in array)
                {
                    string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                    updatableVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
                }

                string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, sourceVersionListFiles[0].FullName[lastFullVersionOutputFullPath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(sourceVersionListFiles[0].FullName, destFileName, true);
            }

            foreach (string lastFileFullName in filteredFullFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFileFullName);
                if (File.Exists(lastFileFullName))
                {
                    string fileNameWithoutCrcHashCode = fileNameWithoutExtension[..fileNameWithoutExtension.IndexOf(".")];
                    if (updatableVersionList.Contains(fileNameWithoutCrcHashCode))
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, lastFileFullName[lastFullVersionOutputFullPath.Length..]));
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
        private static void MergePackageVersionList(string currentVersionPackagePath, string lastFullVersionOutputPackagePath, string patchVersionPath, string[] filteredPackageFiles)
        {
            PackageVersionListSerializer packageVersionListSerializer = new PackageVersionListSerializer();
            packageVersionListSerializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V0);
            packageVersionListSerializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V1);
            packageVersionListSerializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.PackageVersionListDeserializeCallback_V2);

            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackagePath));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("RemoteVersionList.block", SearchOption.TopDirectoryOnly);
            List<string> packageVersionList = new List<string>();
            if (sourceVersionListFiles.Length > 0)
            {
                byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);

                PackageVersionList sourcePackageVersionList = default;
                using (Stream stream = new MemoryStream(sourceVersionListBytes))
                {
                    sourcePackageVersionList = packageVersionListSerializer.Deserialize(stream);
                }

                PackageVersionList.Resource[] array = sourcePackageVersionList.GetResources();

                foreach (PackageVersionList.Resource resource in array)
                {
                    string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                    packageVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
                }

                string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, sourceVersionListFiles[0].FullName[lastFullVersionOutputPackagePath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(sourceVersionListFiles[0].FullName, destFileName, true);
            }

            foreach (string lastFilePackageName in filteredPackageFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFilePackageName);
                if (File.Exists(lastFilePackageName))
                {
                    if (packageVersionList.Contains(fileNameWithoutExtension))
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, lastFilePackageName[lastFullVersionOutputPackagePath.Length..]));
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
        private static void MergePackedVersionList(string currentVersionPackedPath, string lastFullVersionOutputPackedPath, string patchVersionPath, string[] filteredPackedFiles)
        {
            ReadOnlyVersionListSerializer serializer = new ReadOnlyVersionListSerializer();
            serializer.RegisterDeserializeCallback(0, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V0);
            serializer.RegisterDeserializeCallback(1, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V1);
            serializer.RegisterDeserializeCallback(2, BuiltinVersionListSerializer.LocalVersionListDeserializeCallback_V2);
            string sourcePath = Utility.Path.GetRegularPath(Path.Combine(currentVersionPackedPath));
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            FileInfo[] sourceVersionListFiles = sourceDirectoryInfo.GetFiles("LocalVersionList.block", SearchOption.TopDirectoryOnly);
            List<string> readOnlyVersionList = new List<string>();
            if (sourceVersionListFiles.Length > 0)
            {
                byte[] sourceVersionListBytes = File.ReadAllBytes(sourceVersionListFiles[0].FullName);
                LocalVersionList versionList = default;

                using (Stream stream = new MemoryStream(sourceVersionListBytes))
                {
                    versionList = serializer.Deserialize(stream);
                }


                LocalVersionList.Resource[] resources = versionList.GetResources();

                foreach (LocalVersionList.Resource resource in resources)
                {
                    string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension) : Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
                    readOnlyVersionList.Add(Path.GetFileNameWithoutExtension(fullName));
                }

                string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, sourceVersionListFiles[0].FullName[lastFullVersionOutputPackedPath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(sourceVersionListFiles[0].FullName, destFileName, true);
            }

            foreach (string lastFilePackedName in filteredPackedFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(lastFilePackedName);
                if (File.Exists(lastFilePackedName))
                {
                    if (readOnlyVersionList.Contains(fileNameWithoutExtension))
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(patchVersionPath, lastFilePackedName[lastFullVersionOutputPackedPath.Length..]));
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
            if (DEngineSetting.Instance.ForceRebuildAssetBundle)
            {
                return false;
            }

            Platform platform =DEngineSetting.Instance.BuildPlatform;
            GetBuildVersions(platform, true, out string lastFullVersionOutputFullPath, out string lastPackageVersionOutputFullPath, out string lastPackedVersionOutputFullPath, out _);
            return !string.IsNullOrEmpty(lastFullVersionOutputFullPath) && Directory.Exists(lastFullVersionOutputFullPath) && !string.IsNullOrEmpty(lastPackageVersionOutputFullPath) && Directory.Exists(lastPackageVersionOutputFullPath) && !string.IsNullOrEmpty(lastPackedVersionOutputFullPath) && Directory.Exists(lastPackedVersionOutputFullPath);
        }
    }
}