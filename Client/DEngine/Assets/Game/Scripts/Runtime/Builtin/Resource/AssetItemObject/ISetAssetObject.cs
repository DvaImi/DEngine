using DEngine;

namespace Game.AssetItemObject
{
    public interface ISetAssetObject : IReference
    {
        /// <summary>
        /// 资源名称。
        /// </summary>
        string AssetName { get; }

        /// <summary>
        /// 设置资源。
        /// </summary>
        void SetAsset(UnityEngine.Object asset);

        /// <summary>
        /// 是否可以回收。
        /// </summary>
        bool IsCanRelease();
    }
}