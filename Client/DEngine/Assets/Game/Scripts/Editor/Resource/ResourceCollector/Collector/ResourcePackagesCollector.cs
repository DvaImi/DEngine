using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [CreateAssetMenu(menuName = "Game/ResourcePackagesCollector", order = 2)]
    public class ResourcePackagesCollector : ScriptableObject
    {
        public List<ResourceGroupsCollector> PackagesCollector = new List<ResourceGroupsCollector>();

        public static ResourcePackagesCollector GetPackageCollector()
        {
            return EditorTools.LoadScriptableObject<ResourcePackagesCollector>();
        }

        public static ResourceGroupsCollector GetResourceGroupsCollector()
        {
            return GetResourceGroupsCollector(GameSetting.Instance.AssetBundleCollectorIndex);
        }

        public static ResourceGroupsCollector GetResourceGroupsCollector(int index)
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