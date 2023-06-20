using System.IO;
using UnityEngine;
using DEngine.Editor.ResourceTools;
using DEngine;
using System.Collections.Generic;

namespace Game.Editor.Builder
{
    public static partial class GameBuilder
    {
        public static void PutToLocalSimulator(Platform platform, string outputFullPath)
        {
            if (!GameSetting.Instance.AutoCopyToVirtualServer)
            {
                return;
            }

            string virtualServerAddress = GameSetting.Instance.VirtualServerAddress;

            if (!Directory.Exists(virtualServerAddress))
            {
                return;
            }

            //先拷贝版本文件
            string versionJson = Path.Combine(GameSetting.Instance.BundlesOutput, GetPlatformPath(platform) + "Version.json");
            string versionpath = Path.Combine(virtualServerAddress, GameSetting.Instance.LatestGameVersion);
            IOUtility.CreateDirectoryIfNotExists(versionpath);
            if (File.Exists(versionJson))
            {
                File.Copy(versionJson, Path.Combine(versionpath, GetPlatformPath(platform) + "Version.json"), true);
            }

            string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
            List<string> bundleFileName = new List<string>();
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(versionpath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }
                bundleFileName.Add(destFileInfo.Name[..destFileInfo.Name.IndexOf(".")]);
                File.Copy(fileName, destFileName);
            }
            //由于差异包只包含存在差异的资源，终端本地清空将无法正确下载资源，在此将未产生变化的资源拷贝到服务器
            if (GameSetting.Instance.Difference)
            {
                GetLastBuildBuildPath(out string version, out string FullPath);
                foreach (var item in bundleFileName)
                {
                    Debug.Log(item);
                }
                string[] oldfileNames = Directory.GetFiles(FullPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in oldfileNames)
                {
                    string destFileName = Utility.Path.GetRegularPath(Path.Combine(FullPath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                    FileInfo destFileInfo = new(destFileName);
                    if (bundleFileName.Contains(destFileInfo.Name[..destFileInfo.Name.IndexOf(".")]))
                    {
                        //跳过差异文件
                        continue;
                    }

                    if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }

                    File.Copy(fileName, destFileName);
                }
            }

            Debug.Log("Copy Bundles to virtualServer success");
        }
    }
}
