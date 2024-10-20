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
        /// 差异打包后处理
        /// 主要功能 提取差异包并合并成完整的资源包
        /// </summary>
        /// <param name="platform"></param>
        public static void OnPostprocessDifference(Platform platform)
        {
            //获取上次整包的资源列表
            GetBuildVersions(platform, true, out string lastFullVersionOutputFullPath, out string lastPackageVersionOutputPath, out string lastPackedVersionOutputPath, out _);

            //获取当前版本的资源列表
            GetBuildVersions(platform, false, out string currentVersionFullPath, out string currentVersionPackagePath, out string currentVersionPackedPath, out string patchVersionPath);

            var resourceMode = DEngineSetting.Instance.ResourceMode;

            string[] lastFullVersionFiles = Directory.GetFiles(lastFullVersionOutputFullPath, "*", SearchOption.AllDirectories);
            MergeFullVersionList(currentVersionFullPath, lastFullVersionOutputFullPath, patchVersionPath.Replace("*patch", "Full"), lastFullVersionFiles);

            bool outputPackageSelected = resourceMode == ResourceMode.Package;
            bool outputPackedSelected = resourceMode is ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying;

            if (outputPackageSelected)
            {
                string[] lastPackageVersionFiles = Directory.GetFiles(lastPackageVersionOutputPath, "*", SearchOption.AllDirectories);
                MergePackageVersionList(currentVersionPackagePath, lastPackageVersionOutputPath, patchVersionPath.Replace("*patch", "Package"), lastPackageVersionFiles);
                CopyFileToStreamingAssets(currentVersionPackagePath);
            }

            if (outputPackedSelected)
            {
                string[] lastPackedVersionFiles = Directory.GetFiles(lastPackedVersionOutputPath, "*", SearchOption.AllDirectories);
                MergePackedVersionList(currentVersionPackedPath, lastPackedVersionOutputPath, patchVersionPath.Replace("*patch", "Packed"), lastPackedVersionFiles);
                CopyFileToStreamingAssets(currentVersionPackedPath);
            }

            Debug.Log("Difference postprocess complete.");
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 获取最新的资源构建列表
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="lastFullVersion">是否是最新的整包构建列表</param>
        /// <param name="fullPath">整包列表</param>
        /// <param name="package">单机资源列表</param>
        /// <param name="packed">为可更新模式生成的本地资源列表</param>
        /// <param name="patch"></param>
        private static bool GetBuildVersions(Platform platform, bool lastFullVersion, out string fullPath, out string package, out string packed, out string patch)
        {
            fullPath = package = packed = patch = null;
            if (!IsPlatformSelected(platform))
            {
                return true;
            }

            string outputDirectory = DEngineSetting.BundlesOutput;
            string buildReportDirectory = Path.Combine(outputDirectory, "BuildReport");
            if (!Directory.Exists(buildReportDirectory))
            {
                return false;
            }

            string[] allBuildReport = Directory.GetFiles(buildReportDirectory, "*.xml", SearchOption.AllDirectories);

            if (allBuildReport.Length == 0)
            {
                return false;
            }

            int[] lastBuildVersions = new int[allBuildReport.Length];
            for (int i = 0; i < allBuildReport.Length; i++)
            {
                string item = allBuildReport[i];
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(item);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("DEngine");
                if (xmlRoot != null)
                {
                    XmlNode xmlBuildReport = xmlRoot.SelectSingleNode("BuildReport");
                    if (xmlBuildReport != null)
                    {
                        XmlNode xmlSummary = xmlBuildReport.SelectSingleNode("Summary");
                        if (xmlSummary != null)
                        {
                            XmlNode xmlInternalResourceVersion = xmlSummary.SelectSingleNode("InternalResourceVersion");
                            XmlNode xmlLastFullBuildVersion = xmlSummary.SelectSingleNode("LastFullBuildVersion");
                            if (xmlLastFullBuildVersion != null)
                            {
                                if (xmlInternalResourceVersion != null)
                                {
                                    lastBuildVersions[i] = lastFullVersion ? int.Parse(xmlLastFullBuildVersion.InnerText) : int.Parse(xmlInternalResourceVersion.InnerText);
                                }
                            }
                        }
                    }
                }
            }

            int maxVersion = lastBuildVersions.Max();
            fullPath = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Full/{1}.{2}/{3}/", outputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
            package = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Package/{1}.{2}/{3}/", outputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
            packed = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Packed/{1}.{2}/{3}/", outputDirectory, Application.version, maxVersion, GetPlatformPath(platform))).FullName);
            patch = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Patch/{4}/{1}.{2}/{3}/", outputDirectory, Application.version, maxVersion, GetPlatformPath(platform), "*patch")).FullName);
            return true;
        }

        /// <summary>
        /// 拷贝完整资源列表
        /// </summary>
        /// <param name="currentVersionFullPath"></param>
        /// <param name="lastFullVersionOutputFullPath"></param>
        /// <param name="patchVersionPath"></param>
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
                    string fileNameWithoutCrcHashCode = fileNameWithoutExtension[..fileNameWithoutExtension.IndexOf(".", StringComparison.Ordinal)];
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
        /// <param name="currentVersionPackagePath"></param>
        /// <param name="lastFullVersionOutputPackagePath"></param>
        /// <param name="patchVersionPath"></param>
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
        /// <param name="currentVersionPackedPath"></param>
        /// <param name="lastFullVersionOutputPackedPath"></param>
        /// <param name="patchVersionPath"></param>
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
        /// <returns></returns>
        public static bool CanDifference()
        {
            if (DEngineSetting.Instance.ForceRebuildAssetBundle)
            {
                return false;
            }

            var isSuccess = GetBuildVersions(Platform.Windows, true, out _, out _, out _, out _);
            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.Windows64, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.MacOS, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.Linux, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.IOS, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.Android, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.WindowsStore, true, out _, out _, out _, out _);
            }

            if (isSuccess)
            {
                isSuccess = GetBuildVersions(Platform.WebGL, true, out _, out _, out _, out _);
            }

            return isSuccess;
        }
    }
}