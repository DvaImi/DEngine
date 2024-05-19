using System.IO;
using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    public sealed class FileArchiveHelper : IArchiveHelper
    {
        private IEncryptorHelper m_EncryptorHelper;
        private string m_ArchiveUrl;

        public string ArchiveUrl => m_ArchiveUrl;

        public bool UserEncryptor
        {
            get => m_EncryptorHelper != null;
        }

        public IArchiveSlot CreateArchiveSlot()
        {
            return new DefaultArchiveSlot();
        }

        public void SetArchiveUrl(string archiveUrl)
        {
            m_ArchiveUrl = archiveUrl;
            DirectoryInfo directoryInfo = new DirectoryInfo(archiveUrl);
            if (directoryInfo.Exists)
            {
                return;
            }
            
            directoryInfo.Create();
        }

        public void SetEncryptor(IEncryptorHelper encryptorHelper)
        {
            m_EncryptorHelper = encryptorHelper;
        }

        public byte[] Load(string filePath)
        {
            var data = File.ReadAllBytes(filePath);
            return UserEncryptor ? m_EncryptorHelper.Decrypt(data) : data;
        }

        public void Save(string filePath, byte[] data)
        {
            if (UserEncryptor)
            {
                var encryptedData = m_EncryptorHelper.Encrypt(data);
                GameUtility.IO.SaveFileSafe(filePath, encryptedData);
            }
            else
            {
                GameUtility.IO.SaveFileSafe(filePath, data);
            }
        }

        public async UniTask<byte[]> LoadAsync(string filePath)
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[stream.Length];
            _ = await stream.ReadAsync(buffer, 0, (int)stream.Length);
            return UserEncryptor ? m_EncryptorHelper.Decrypt(buffer) : buffer;
        }

        public async UniTask SaveAsync(string filePath, byte[] data)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Directory is { Exists: false })
            {
                fileInfo.Directory.Create();
            }
            var buffer =UserEncryptor ? m_EncryptorHelper.Decrypt(data) : data;
            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }


        public void Delete(string filePath)
        {
            File.Delete(filePath);
        }


        public void Backup(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath);
        }
    }
}