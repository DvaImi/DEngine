using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using UnityGameFramework.Runtime;

namespace Game
{
    public static class GameAddressSerializerCallback
    {
        private const int CachedHashBytesLength = 4;
        private static readonly byte[] s_CachedHashBytes = new byte[CachedHashBytesLength];

        public static bool Serializer(Stream stream, Dictionary<string, Dictionary<Type, string>> data)
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
                    binaryWriter.Write(address.Value.Count);
                    foreach (var item in address.Value)
                    {
                        binaryWriter.WriteEncryptedString(item.Key.FullName, s_CachedHashBytes);
                        binaryWriter.WriteEncryptedString(item.Value, s_CachedHashBytes);
                    }
                }
            }

            Array.Clear(s_CachedHashBytes, 0, CachedHashBytesLength);
            return true;
        }

        public static Dictionary<string, Dictionary<Type, string>> Deserialize(Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
            {
                byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
                int resourceCount = binaryReader.ReadInt32();
                Dictionary<string, Dictionary<Type, string>> address = resourceCount > 0 ? new Dictionary<string, Dictionary<Type, string>>() : null;
                for (int i = 0; i < resourceCount; i++)
                {
                    string key = binaryReader.ReadEncryptedString(encryptBytes);
                    int typeCount = binaryReader.ReadInt32();
                    Dictionary<Type, string> typeInfo = new Dictionary<Type, string>();
                    for (int j = 0; j < typeCount; j++)
                    {
                        string typeFullName = binaryReader.ReadEncryptedString(encryptBytes);
                        Type type = Utility.Assembly.GetType(typeFullName);
                        if (type == null)
                        {
                            Log.Warning($"不存在的类型 <{typeFullName}>");
                            continue;
                        }
                        typeInfo.Add(type, binaryReader.ReadEncryptedString(encryptBytes));
                    }
                    address.Add(key, typeInfo);
                }

                return address;
            }
        }
    }
}