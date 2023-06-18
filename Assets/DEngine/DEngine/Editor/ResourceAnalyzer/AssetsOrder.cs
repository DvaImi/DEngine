//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://DEngine.cn/
// Feedback: mailto:ellan@DEngine.cn
//------------------------------------------------------------

namespace DEngine.Editor.ResourceTools
{
    public enum AssetsOrder : byte
    {
        AssetNameAsc,
        AssetNameDesc,
        DependencyResourceCountAsc,
        DependencyResourceCountDesc,
        DependencyAssetCountAsc,
        DependencyAssetCountDesc,
        ScatteredDependencyAssetCountAsc,
        ScatteredDependencyAssetCountDesc,
    }
}
