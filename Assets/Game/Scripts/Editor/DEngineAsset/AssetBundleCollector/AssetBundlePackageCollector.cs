using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class AssetBundlePackageCollector : ScriptableObject
    {
        public List<AssetBundleCollector> PackagesCollector = new List<AssetBundleCollector>();

        public static AssetBundlePackageCollector GetPackageCollector()
        {
            return GameEditorUtility.GetScriptableObject<AssetBundlePackageCollector>();
        }

        public static AssetBundleCollector GetBundleCollectorByIndex(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            try
            {
                return GetPackageCollector().PackagesCollector[index];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }
    }
}