using DEngine;
using DEngine.ObjectPool;

namespace Game.AssetItemObject
{
    public class AssetItemObject : ObjectBase
    {
        public static AssetItemObject Create(string location, UnityEngine.Object target)
        {
            var item = ReferencePool.Acquire<AssetItemObject>();
            item.Initialize(location, target);
            return item;
        }

        protected override void Release(bool isShutdown)
        {
            if (Target == null)
            {
                return;
            }

            GameEntry.Resource.UnloadAsset(Target);
        }
    }
}