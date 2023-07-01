using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class AssetBundlePreview
    {
        public List<AssetPreview> assetPreviews = new List<AssetPreview>();
    }

    public class AssetPreview
    {
        public string assetPath = string.Empty;
        public string groups = string.Empty;
        public Object[] assets = new Object[0];
        public int Count;
        public long Length;
    }
}