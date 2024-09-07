using System.IO;
using System.Text;

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
    }
}
