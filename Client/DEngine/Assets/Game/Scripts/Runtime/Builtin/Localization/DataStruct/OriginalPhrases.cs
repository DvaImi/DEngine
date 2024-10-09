using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine.Localization;

namespace Game.Localization
{
    [Serializable]
    public class OriginalPhrases
    {
        /// <summary>
        /// 语言
        /// </summary>
        public Language Language;

        /// <summary>
        /// 语言字典
        /// </summary>
        public List<Phrase> Phrases = new();

        public string this[string key]
        {
            get
            {
                for (int i = 0; i < Phrases.Count; i++)
                {
                    if (Phrases[i].key == key)
                    {
                        return Phrases[i].value;
                    }
                }
                return "Key not found.";
            }
        }

        /// <summary>
        /// 获取字典二进制数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetAllPhrasesData()
        {
            using MemoryStream memoryStream = new();
            using (BinaryWriter binaryWriter = new(memoryStream, Encoding.UTF8))
            {
                foreach (var phrase in Phrases)
                {
                    binaryWriter.Write(phrase.key);
                    binaryWriter.Write(phrase.value);
                }
            }
            return memoryStream.ToArray();
        }
    }
}