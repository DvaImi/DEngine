//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
using Utility = GameFramework.Utility;

namespace Game
{
    public static class AssetUtility
    {
        private static Dictionary<string, Dictionary<Type, string>> m_Address = null;

        public static void InitAddress(byte[] bytes)
        {
            GameAddressSerializer serializer = new GameAddressSerializer();
            serializer.RegisterDeserializeCallback(0, GameAddressSerializerCallback.Deserialize);

            using (Stream stream = new MemoryStream(bytes))
            {
                m_Address = serializer.Deserialize(stream);
                Log.Debug("address serializer success");
            }
        }

        public static string GetAddress(string address)
        {
            if (m_Address == null)
            {
                return null;
            }

            if (m_Address.TryGetValue(address, out Dictionary<Type, string> typeAssets))
            {
                if (typeAssets != null)
                {
                    using Dictionary<Type, string>.Enumerator enumerator = typeAssets.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current.Value;
                    }
                }
            }
            return null;
        }

        public static string GetAddress<T>(string address)
        {
            if (m_Address == null)
            {
                return null;
            }

            if (m_Address.TryGetValue(address, out Dictionary<Type, string> typeAssets))
            {
                if (typeAssets != null)
                {
                    if (typeAssets.TryGetValue(typeof(T), out string assetPath))
                    {
                        return assetPath;
                    }
                }
            }
            return null;
        }

        public static string GetAddress(Type type, string address)
        {
            if (m_Address == null)
            {
                return null;
            }

            if (m_Address.TryGetValue(address, out Dictionary<Type, string> typeAssets))
            {
                if (typeAssets != null)
                {
                    if (typeAssets.TryGetValue(type, out string assetPath))
                    {
                        return assetPath;
                    }
                }
            }
            return null;
        }

        public static string[] GetAddress(string[] address)
        {
            if (m_Address == null)
            {
                throw new GameFrameworkException("Unable Load Address");
            }
            if (address == null)
            {
                throw new GameFrameworkException("address is  invalid");
            }
            string[] addressArray = new string[address.Length];
            for (int i = 0; i < address.Length; i++)
            {
                addressArray[i] = GetAddress(address[i]);
            }

            return addressArray;
        }

        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Game/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Game/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Game/Localization/{0}/{1}.{2}", GameEntry.Localization.Language, assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Game/Fonts/{0}.ttf", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Game/Scenes/{0}.unity", assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Music/{0}.mp3", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Sounds/{0}.mp3", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Game/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Game/UI/UIForms/{0}.prefab", assetName);
        }

        public static string GetUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/UI/UISounds/{0}.mp3", assetName);
        }
    }
}
