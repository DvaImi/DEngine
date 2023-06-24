using System.IO;
using UnityEngine;
using DEngine.Editor.ResourceTools;
using DEngine;
using System.Collections.Generic;

namespace Game.Editor.Builder
{
    public static partial class GameBuilder
    {
        private static string m_lastVersion;
        private static string m_lastVersionFullPath;

        public static void SetLastVersion()
        {
            GetLastBuildBuildPath(out m_lastVersion, out m_lastVersionFullPath);
        }
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

            Debug.Log("最近打包版本：" + m_lastVersion);
            List<string> differencebundleFileName = new List<string>();

            //先拷贝版本文件
            string versionJson = Path.Combine(GameSetting.Instance.BundlesOutput, GetPlatformPath(platform) + "Version.json");
            string versionpath = Path.Combine(virtualServerAddress, GameSetting.Instance.LatestGameVersion);
            IOUtility.CreateDirectoryIfNotExists(versionpath);
            if (File.Exists(versionJson))
            {
                File.Copy(versionJson, Path.Combine(versionpath, GetPlatformPath(platform) + "Version.json"), true);
            }

            string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);

            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(versionpath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }
                File.Copy(fileName, destFileName);
                if (GameSetting.Instance.Difference)
                {
                    differencebundleFileName.Add(destFileInfo.Name[..destFileInfo.Name.IndexOf(".")]);
                }
            }

            #region Difference

            //由于差异包只包含存在差异的资源，终端本地清空将无法正确下载资源，在此将未产生变化的资源拷贝到服务器
            if (GameSetting.Instance.Difference)
            {
                if (string.IsNullOrEmpty(m_lastVersion) || string.IsNullOrEmpty(m_lastVersionFullPath))
                {
                    return;
                }
                string[] oldfileNames = Directory.GetFiles(m_lastVersionFullPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in oldfileNames)
                {
                    FileInfo oldFileInfo = new FileInfo(fileName);
                    string destFileName = Utility.Path.GetRegularPath(Path.Combine(versionpath, GameSetting.Instance.InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                    FileInfo destFileInfo = new(destFileName);
                    if (differencebundleFileName.Contains(oldFileInfo.Name[..oldFileInfo.Name.IndexOf(".")]))
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
            #endregion
            Debug.Log("Copy Bundles to virtualServer success");
        }
    }
}
