using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Game
{
    public static partial class GameUtility
    {
        public static class Decrypt
        {
            private static readonly byte[] AESKey = Encoding.UTF8.GetBytes("YourAESKey1234567890123456"); // 32 字节
            private static readonly byte[] AESIV = Encoding.UTF8.GetBytes("YourAESIV12345678");           // 16 字节

            /// <summary>
            /// AES 加密资源。
            /// </summary>
            /// <param name="bytes">要加密的资源二进制流。</param>
            /// <returns>加密后的资源二进制流。</returns>
            public static byte[] EncryptResource(byte[] bytes)
            {
                if (bytes == null || bytes.Length == 0)
                {
                    throw new ArgumentException("Bytes cannot be null or empty.", nameof(bytes));
                }

                using var aes = Aes.Create();
                aes.Key = AESKey;
                aes.IV = AESIV;

                using var memoryStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
                return memoryStream.ToArray();
            }

            /// <summary>
            /// AES 解密资源。
            /// </summary>
            /// <param name="bytes">要解密的资源二进制流。</param>
            /// <param name="startIndex">解密二进制流的起始位置。</param>
            /// <param name="count">解密二进制流的长度。</param>
            /// <param name="name">资源名称。</param>
            /// <param name="variant">变体名称。</param>
            /// <param name="extension">扩展名称。</param>
            /// <param name="storageInReadOnly">资源是否在只读区。</param>
            /// <param name="fileSystem">文件系统名称。</param>
            /// <param name="loadType">资源加载方式。</param>
            /// <param name="length">资源大小。</param>
            /// <param name="hashCode">资源哈希值。</param>
            public static void DecryptResource(byte[] bytes, int startIndex, int count, string name, string variant, string extension, bool storageInReadOnly, string fileSystem, byte loadType, int length, int hashCode)
            {
                if (bytes == null || bytes.Length == 0)
                {
                    throw new ArgumentException("Bytes cannot be null or empty.", nameof(bytes));
                }

                using var aes = Aes.Create();
                aes.Key = AESKey;
                aes.IV = AESIV;

                using var memoryStream = new MemoryStream(bytes, startIndex, count);
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                _ = cryptoStream.Read(bytes, startIndex, count);
            }
        }
    }
}