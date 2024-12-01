using DEngine;
using DEngine.ObjectPool;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.AssetObject
{
    [DisallowMultipleComponent]
    public class AssetObjectComponent : DEngineComponent
    {
        /// <summary>
        /// 检查是否可以释放间隔
        /// </summary>
        [SerializeField, Range(60, 120)] private float checkCanReleaseInterval = 60f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        [SerializeField, Range(60, 120)] private float autoReleaseInterval = 60f;

        /// <summary>
        /// 
        /// </summary>
        private DEngineLinkedList<LoadAssetObject> m_LoadAssetObjectsLinkedList;

        /// <summary>
        /// 
        /// </summary>
        private IObjectPool<AssetItemObject> m_AssetItemPool;

        private float m_CheckCanReleaseTime;
        private LoadAssetCallbacks m_LoadAssetCallbacks;

        private void Start()
        {
            m_LoadAssetObjectsLinkedList = new DEngineLinkedList<LoadAssetObject>();
            m_AssetItemPool = GameEntry.ObjectPool.CreateMultiSpawnObjectPool<AssetItemObject>("AssetItemObject", autoReleaseInterval, 16, 60, 0);
            m_LoadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);
        }

        private void Update()
        {
            m_CheckCanReleaseTime += Time.unscaledDeltaTime;
            if (m_CheckCanReleaseTime < (double)checkCanReleaseInterval)
            {
                return;
            }

            ReleaseUnused();
        }

        public void SetAssetByResources<T>(ISetAssetObject setAssetObject) where T : Object
        {
            if (m_AssetItemPool.CanSpawn(setAssetObject.AssetName))
            {
                var assetObject = (T)m_AssetItemPool.Spawn(setAssetObject.AssetName).Target;
                SetAsset(setAssetObject, assetObject);
            }
            else
            {
                GameEntry.Resource.LoadAsset(setAssetObject.AssetName, typeof(T), m_LoadAssetCallbacks, setAssetObject);
            }
        }

        private void ReleaseUnused()
        {
            if (m_LoadAssetObjectsLinkedList is not { Count: > 0 })
            {
                return;
            }

            var current = m_LoadAssetObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.AssetObject.IsCanRelease())
                {
                    m_AssetItemPool.Unspawn(current.Value.AssetTarget);
                    ReferencePool.Release(current.Value.AssetObject);
                    m_LoadAssetObjectsLinkedList.Remove(current);
                }

                current = next;
            }

            Log.Info("Unload unused assets...");
            m_CheckCanReleaseTime = 0f;
        }

        private void SetAsset(ISetAssetObject setAssetObject, Object assetObject)
        {
            m_LoadAssetObjectsLinkedList.AddLast(new LoadAssetObject(setAssetObject, assetObject));
            setAssetObject.SetAsset(assetObject);
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            var setAssetObject = (ISetAssetObject)userdata;
            var assetObject = asset as Object;
            if (assetObject)
            {
                m_AssetItemPool.Register(AssetItemObject.Create(setAssetObject.AssetName, assetObject), true);
                SetAsset(setAssetObject, assetObject);
            }
            else
            {
                Log.Error($"Load failure asset type is {asset.GetType()}.");
            }
        }

        private static void OnLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Error("Can not load asset from '{1}' with error message '{2}'.", assetName, errormessage);
        }
    }
}