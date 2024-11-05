using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using DEngine.Runtime;
using Game.Editor.BuildPipeline;
using Game.Editor.ResourceTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class GameResourceBuildEventHandler : IBuildEventHandler
    {
        public bool ContinueOnFailure => true;

        private readonly Dictionary<string, string> m_WaitUploadFileNames = new();
        private const string FullPartToFind = "Full/";
        private VersionInfo m_VersionInfo = null;

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);

            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                DEngineSetting.Instance.ResourceMode = ResourceMode.Package;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameUtility.IO.Delete(Application.streamingAssetsPath);
            DEngineSetting.Instance.LatestGameVersion = applicableGameVersion;
            DEngineSetting.Instance.InternalResourceVersion = internalResourceVersion;
            DEngineSetting.Save();
            m_WaitUploadFileNames.Clear();
            m_VersionInfo = null;
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            if (DEngineSetting.Instance.ResourceMode is not (ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying))
            {
                return;
            }

            string platformPath = GameBuildPipeline.GetPlatformPath(platform);

            m_VersionInfo = new VersionInfo
            {
                ForceUpdateGame = DEngineSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = GameBuildPipeline.GetUpdatePrefixUri(platform),
                LatestGameVersion = DEngineSetting.Instance.LatestGameVersion,
                InternalGameVersion = DEngineSetting.Instance.InternalGameVersion,
                InternalResourceVersion = DEngineSetting.Instance.InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode,
                UseResourcePatchPack = false,
                PatchResourcePackName = null,
                PatchTotalCompressedLength = 0
            };

            var checkVersionInfo = new JObject()
            {
                ["CheckVersionUrl"] = GameBuildPipeline.GetCheckVersionUrl(platform)
            };
            var versionJson = JsonConvert.SerializeObject(checkVersionInfo);
            var versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "CheckVersion.json");
            File.WriteAllText(versionFilePath, versionJson);
            GameBuildPipeline.SaveBuiltinData(platform);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            string sourcePath = DEngineSetting.Instance.ResourceMode switch
            {
                ResourceMode.Unspecified or ResourceMode.Package => outputPackagePath,
                ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying => outputPackedPath,
                _ => outputPackagePath
            };

            GameBuildPipeline.CopyFileToStreamingAssets(sourcePath);

            if (DEngineSetting.Instance.ResourceMode <= ResourceMode.Package)
            {
                return;
            }

            if (m_VersionInfo == null)
            {
                Debug.LogError("Version info is null");
                return;
            }

            string platformPath = GameBuildPipeline.GetPlatformPath(platform);
            if (DEngineSetting.Instance.BuildResourcePack && ProcessResourcePatchPack(platform))
            {
                Debug.Log("Build resource patch pack success.");
            }
            else
            {
                PrecessLocalHost(outputFullPath);
            }

            m_WaitUploadFileNames.Add(SaveVersionInfo(platformPath), platformPath + "Version.json");
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();

            if (!DEngineSetting.Instance.EnableHostingService)
            {
                EditorUtility.RevealInFinder(DEngineSetting.BundlesOutput);
                return;
            }

            InternalUpload().Forget();
        }

        private string SaveVersionInfo(string platformPath)
        {
            string versionJson = JsonConvert.SerializeObject(m_VersionInfo);
            string versionFolder = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Full/{1}.{2}/", DEngineSetting.BundlesOutput, m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalResourceVersion)).FullName);
            string versionFilePath = Utility.Path.GetRegularCombinePath(versionFolder, platformPath + "Version.json");
            File.WriteAllText(versionFilePath, versionJson);
            return versionFilePath;
        }

        private void PrecessLocalHost(string outputFullPath)
        {
            if (DEngineSetting.Instance.EnableHostingService)
            {
                string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    string bundlesOutput = DEngineSetting.BundlesOutput;
                    string relativePath = fileName[bundlesOutput.Length..];

                    int indexOfFull = relativePath.IndexOf(FullPartToFind, StringComparison.Ordinal);

                    if (indexOfFull >= 0)
                    {
                        string destFileName = relativePath[(indexOfFull + FullPartToFind.Length)..];
                        m_WaitUploadFileNames.Add(fileName, destFileName);
                    }
                    else
                    {
                        Debug.LogError("'Full/' not found in the path.");
                    }
                }
            }
        }

        private bool ProcessResourcePatchPack(Platform platform)
        {
            string platformPath = GameBuildPipeline.GetPlatformPath(platform);
            if (DEngineSetting.Instance.ResourceMode >= ResourceMode.Updatable && DEngineSetting.Instance.BuildResourcePack)
            {
                ResourcePackBuilderController controller = new();
                if (controller.Load())
                {
                    controller.Platform = platform;
                    var versionNames = controller.GetVersionNames();
                    if (versionNames.Length < 2)
                    {
                        Debug.LogWarning("No version was found in the specified working directory and platform.");
                        DEngineSetting.Instance.BuildResourcePack = false;
                        DEngineSetting.Save();
                        return false;
                    }

                    controller.BackupDiff = controller.BackupVersion = true;
                    controller.CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
                    controller.RefreshCompressionHelper();
                    string targetVersion = versionNames[^1];
                    string sourceVersion = versionNames[^2];
                    if (controller.BuildResourcePack(sourceVersion, targetVersion))
                    {
                        Debug.LogFormat("Build Resource Pack {0}==>{1} Success", sourceVersion, targetVersion);

                        // Patch
                        string searchPattern = Utility.Text.Format("{0}-{1}-{2}.*.block", "DEngineResourcePack", sourceVersion, targetVersion);
                        string[] files = Directory.GetFiles(controller.OutputPath, searchPattern);
                        if (files.Length > 0)
                        {
                            FileInfo fileInfo = new(files[0]);
                            m_VersionInfo.UseResourcePatchPack = true;
                            m_VersionInfo.PatchResourcePackName = fileInfo.Name;
                            m_VersionInfo.PatchTotalCompressedLength = fileInfo.Length;
                            string destFileName = Utility.Path.GetRegularCombinePath(targetVersion, platformPath, fileInfo.FullName[controller.OutputPath.Length..]);
                            m_WaitUploadFileNames.Add(fileInfo.FullName, destFileName);
                        }

                        //Version
                        var targetDirectoryInfo = new DirectoryInfo(Utility.Text.Format("{0}/{1}-{2}-{3}", controller.OutputPath, "DEngineResourcePack", sourceVersion, targetVersion));
                        var targetVersionListFiles = targetDirectoryInfo.GetFiles("RemoteVersionList.*.block", SearchOption.TopDirectoryOnly);
                        if (targetVersionListFiles.Length > 0)
                        {
                            var destFileName = Utility.Path.GetRegularCombinePath(targetVersion, platformPath, targetVersionListFiles[0].Name);
                            m_WaitUploadFileNames.Add(targetVersionListFiles[0].FullName, destFileName);
                        }
                    }

                    return false;
                }
            }

            return false;
        }

        private async UniTask InternalUpload()
        {
            Debug.Log($"Ready to Upload To {DEngineSetting.Instance.HostURL}...");
            await UniTask.Delay(2000);
            if (!HostingServiceManager.IsListening)
            {
                HostingServiceManager.StartService();
            }

            foreach (var fileName in m_WaitUploadFileNames)
            {
                await HostingServiceManager.UploadFile(fileName.Key, fileName.Value);
            }

            Debug.Log($"Upload To {DEngineSetting.Instance.HostURL} complete...");
            HostingServiceManager.StopService();
            EditorUtility.RevealInFinder(DEngineSetting.BundlesOutput);
        }
    }
}