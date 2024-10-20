using System.Collections.Generic;
using DEngine.Localization;
using UnityEngine;

namespace Game.Localization
{
    [CreateAssetMenu(menuName = "Game/BuildinLanguage", order = 1)]
    public class BuildinLanguage : ScriptableObject
    {
        public List<OriginalPhrases> OriginalPhrases = new List<OriginalPhrases>();

        public byte[] this[Language language]
        {
            get
            {
                for (int i = 0; i < OriginalPhrases.Count; i++)
                {
                    if (OriginalPhrases[i].Language == language)
                    {
                        return OriginalPhrases[i].GetAllPhrasesData();
                    }
                }

                return null;
            }
        }
    }
}