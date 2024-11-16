using System.Collections.Generic;
using DEngine.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    public interface IResolveDuplicateAssetsHelper
    {
        List<string> GetDuplicateAssetNames();
        bool ResolveDuplicateAssets(List<string> duplicateAssetNames);
    }
}