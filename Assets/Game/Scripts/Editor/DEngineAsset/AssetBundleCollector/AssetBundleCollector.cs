using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [CreateAssetMenu]
    public class AssetBundleCollector : ScriptableObject
    {
        public List<AssetBundleGroupCollector> Groups = new();

        internal static AssetBundleCollector Load()
        {
            return GameEditorUtility.GetScriptableObject<AssetBundleCollector>();
        }
    }
}