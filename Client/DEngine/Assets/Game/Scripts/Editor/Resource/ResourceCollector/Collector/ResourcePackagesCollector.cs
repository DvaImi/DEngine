using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [GameFilePath("Assets/Game/Configuration/ResourcePackagesCollector.asset"), CreateAssetMenu(menuName = "Game/ResourcePackagesCollector", order = 2)]
    public class ResourcePackagesCollector : ScriptableSingleton<ResourcePackagesCollector>
    {
        public List<ResourceGroupsCollector> PackagesCollector = new();

        public static ResourceGroupsCollector GetResourceGroupsCollector()
        {
            return GetResourceGroupsCollector(DEngineSetting.Instance.AssetBundleCollectorIndex);
        }

        public static ResourceGroupsCollector GetResourceGroupsCollector(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            try
            {
                return Instance.PackagesCollector[index];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }
    }
}