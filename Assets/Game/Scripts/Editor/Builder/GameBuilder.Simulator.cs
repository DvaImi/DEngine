using System.IO;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

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
            if (File.Exists(versionJson))
            {
                string versionpath = Path.Combine(virtualServerAddress, GameSetting.Instance.LatestGameVersion);
                IOUtility.CreateDirectoryIfNotExists(versionpath);
                File.Copy(versionJson, Path.Combine(versionpath, GetPlatformPath(platform) + "Version.json"), true);

                var fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    string destFileName = GameFramework.Utility.Path.GetRegularPath(Path.Combine(versionpath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                    FileInfo destFileInfo = new(destFileName);
                    if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }

                    File.Copy(fileName, destFileName);
                }

                Debug.Log("Copy Bundles to virtualServer success");
            }
        }
    }
}
