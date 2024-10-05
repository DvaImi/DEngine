using System.IO;
using System.Security.Cryptography;

namespace Game.Archive
{
    public sealed class DefaultEncryptorHelper : IEncryptorHelper
    {
        private readonly byte[] m_Key;
        private readonly byte[] m_Iv;

        public DefaultEncryptorHelper()
        {
            m_Key = new[]
            {
                (byte)'d', (byte)'e', (byte)'a', (byte)'f', (byte)'u', (byte)'l', (byte)'t', (byte)'k',
                (byte)'e', (byte)'y', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6'
            };
            m_Iv = new[]
            {
                (byte)'d', (byte)'e', (byte)'a', (byte)'f', (byte)'u', (byte)'l', (byte)'t', (byte)'i',
                (byte)'v', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7'
            };
        }

        public byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = m_Key;
            aes.IV = m_Iv;
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(data, encryptor);
        }

        public byte[] Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = m_Key;
            aes.IV = m_Iv;
            using var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            return PerformCryptography(data, decrypt);
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}