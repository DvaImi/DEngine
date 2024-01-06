using System.IO;
using DEngine;
using DEngine.Editor.ResourceTools;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        /// <summary>
        /// 上传到本地资源服务器
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="outputFullPath"></param>
        public static void PutToLocalFileServer(Platform platform, string outputFullPath)
        {
            if (!GameSetting.Instance.AutoCopyToFileServer)
            {
                return;
            }

            string virtualServerAddress = GameSetting.Instance.FileServerAddress;

            if (!Directory.Exists(virtualServerAddress))
            {
                return;
            }

            IOUtility.Delete(virtualServerAddress);

            //先拷贝版本文件
            string versionJson = Path.Combine(GameSetting.Instance.BundlesOutput, GetPlatformPath(platform) + "Version.json");
            string versionPath = Path.Combine(virtualServerAddress, GameSetting.Instance.LatestGameVersion);
            IOUtility.CreateDirectoryIfNotExists(versionPath);
            if (File.Exists(versionJson))
            {
                File.Copy(versionJson, Path.Combine(versionPath, GetPlatformPath(platform) + "Version.json"), true);
            }

            string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);

            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(versionPath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }
                File.Copy(fileName, destFileName);
            }

            Debug.Log("Copy Bundles to local file server success");
        }

        /// <summary>
        /// 上传到远端服务器
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="outputFullPath"></param>
        /// <param name="fileNames"></param>
        public static void PutToRemoteFileServer(Platform platform, string outputFullPath, string[] fileNames)
        {


        }
    }
}
