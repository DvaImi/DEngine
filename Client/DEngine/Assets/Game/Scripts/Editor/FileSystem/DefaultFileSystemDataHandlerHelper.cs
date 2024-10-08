using System.IO;

namespace Game.Editor.ResourceTools
{
    public class DefaultFileSystemDataHandlerHelper : IFileSystemDataHandlerHelper
    {
        private static readonly byte[] Empty = { };

        public byte[] GetBytes(string fullPath)
        {
            return !File.Exists(fullPath) ? Empty : File.ReadAllBytes(fullPath);
        }
    }
}