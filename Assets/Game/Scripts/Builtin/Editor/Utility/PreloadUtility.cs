// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-19 23:21:01
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Game.Editor
{
    public static class PreloadUtility
    {
        /// <summary>
        /// 生成数据表信息文件
        /// </summary>
        /// <param name="dataTables"></param>
        public static void GenerateDataTableInfoFile(List<string> dataTables)
        {
            PreloadInfo preloadInfo = GetPreloadInfo();
            preloadInfo.DateTable = dataTables;
            string preloadInfoJson = JsonConvert.SerializeObject(preloadInfo);
            SavePreloadInfoToJson(preloadInfoJson);
        }

        /// <summary>
        /// 生成字典信息文件
        /// </summary>
        /// <param name="dictionary"></param>
        public static void GenerateDictionaryInfoFile(List<string> dictionary)
        {
            PreloadInfo preloadInfo = GetPreloadInfo();
            preloadInfo.Dictionary = dictionary;
            string preloadInfoJson = JsonConvert.SerializeObject(preloadInfo);
            SavePreloadInfoToJson(preloadInfoJson);
        }

        public static void GenerateConfigInfoFile(List<string> config)
        {
            PreloadInfo preloadInfo = GetPreloadInfo();
            preloadInfo.Config = config;
            string preloadInfoJson = JsonConvert.SerializeObject(preloadInfo);
            SavePreloadInfoToJson(preloadInfoJson);
        }

        /// <summary>
        /// 保存Preload info 文件
        /// </summary>
        /// <param name="preloadInfoJson"></param>
        public static void SavePreloadInfoToJson(string preloadInfoJson)
        {
            using (FileStream stream = new(GameSetting.Instance.PreloadInfoPath, FileMode.Create, FileAccess.Write))
            {
                UTF8Encoding utf8Encoding = new(false);
                using StreamWriter writer = new(stream, utf8Encoding);
                writer.Write(preloadInfoJson);
            }
        }

        public static PreloadInfo GetPreloadInfo()
        {
            if (!File.Exists(GameSetting.Instance.PreloadInfoPath))
            {
                PreloadInfo newPreloadInfo = new();
                SavePreloadInfoToJson(JsonConvert.SerializeObject(newPreloadInfo));
                return newPreloadInfo;
            }
            string preloadInfoJson = File.ReadAllText(GameSetting.Instance.PreloadInfoPath);
            return JsonConvert.DeserializeObject<PreloadInfo>(preloadInfoJson);
        }
    }
}