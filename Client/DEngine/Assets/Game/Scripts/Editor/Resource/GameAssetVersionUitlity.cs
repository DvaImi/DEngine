using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;

namespace Game.Editor.ResourceTools
{
    public static class GameAssetVersionUitlity
    {
        public static void CreateAssetVersion(string[] version, string writePath)
        {
            using FileStream stream = new(writePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter binaryWriter = new(stream, Encoding.UTF8);
            binaryWriter.Write(version.Length);
            for (int i = 0; i < version.Length; i++)
            {
                binaryWriter.Write(version[i]);
            }

            binaryWriter.Flush();
        }

        public static void CreateAssetVersion<T>(T version, string writePath)
        {
            if (File.Exists(writePath))
            {
                File.Delete(writePath);
            }

            var metaPath = writePath + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            var jsonContent = JsonConvert.SerializeObject(version);
            File.WriteAllText(writePath, jsonContent);
            AssetDatabase.Refresh();
        }
    }
}