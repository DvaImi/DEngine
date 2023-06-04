using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;

namespace Game
{
    public static class GameAddressSerializerCallback
    {
        private const int CachedHashBytesLength = 4;
        private static readonly byte[] s_CachedHashBytes = new byte[CachedHashBytesLength];

        public static bool Serializer(Stream stream, Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
            {
                return false;
            }

            Utility.Random.GetRandomBytes(s_CachedHashBytes);
            using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
            {
                binaryWriter.Write(s_CachedHashBytes);
                binaryWriter.Write(data.Count);
                foreach (var address in data)
                {
                    binaryWriter.WriteEncryptedString(address.Key, s_CachedHashBytes);
                    binaryWriter.WriteEncryptedString(address.Value, s_CachedHashBytes);
                }
            }

            Array.Clear(s_CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        public static Dictionary<string, string> Deserialize(Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
            {
                byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
                int resourceCount = binaryReader.ReadInt32();
                Dictionary<string, string> address = resourceCount > 0 ? new Dictionary<string, string>() : null;
                for (int i = 0; i < resourceCount; i++)
                {
                    address.Add(binaryReader.ReadEncryptedString(encryptBytes), binaryReader.ReadEncryptedString(encryptBytes));
                }

                return address;
            }
        }
    }
}