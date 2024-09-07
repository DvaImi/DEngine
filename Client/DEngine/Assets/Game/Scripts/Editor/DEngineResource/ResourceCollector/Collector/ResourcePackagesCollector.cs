using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [CreateAssetMenu]
    public class ResourcePackagesCollector : ScriptableObject
    {
        public List<ResourceGroupsCollector> PackagesCollector = new List<ResourceGroupsCollector>();

        public static ResourcePackagesCollector GetPackageCollector()
        {
            return GameEditorUtility.LoadScriptableObject<ResourcePackagesCollector>();
        }

        public static ResourceGroupsCollector GetBundleCollectorByIndex(int index)
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