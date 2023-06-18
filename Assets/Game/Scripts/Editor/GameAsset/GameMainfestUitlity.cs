using System.IO;
using System.Text;

namespace Game.Editor.ResourceTools
{
    public static class GameMainfestUitlity
    {
        public static void CreatMainfest(string mainfest, string writePath)
        {
            using (FileStream stream = new(writePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(mainfest);
                    binaryWriter.Flush();
                }
            }
        }

        public static void CreatMainfest(string[] mainfests, string writePath)
        {
            using (FileStream stream = new(writePath, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(mainfests.Length);
                    for (int i = 0; i < mainfests.Length; i++)
                    {
                        binaryWriter.Write(mainfests[i]);
                    }
                    binaryWriter.Flush();
                }
            }
        }
    }
}
