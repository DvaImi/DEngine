using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    public sealed class FileArchiveHelper : IArchiveHelper
    {
        private readonly Regex m_Regex = new(@"^[a-zA-Z0-9_]{3,16}$");

        public bool Query(string fileUri)
        {
            return File.Exists(fileUri);
        }

        public async UniTask SaveAsync(string fileName, byte[] bytes)
        {
            FileInfo fileInfo = new(fileName);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            await File.WriteAllBytesAsync(fileName, bytes);
        }

        public async UniTask<byte[]> LoadAsync(string fileUri)
        {
            return await File.ReadAllBytesAsync(fileUri);
        }

        public bool Match(string userIdentifier)
        {
            return m_Regex.IsMatch(userIdentifier);
        }
    }
}