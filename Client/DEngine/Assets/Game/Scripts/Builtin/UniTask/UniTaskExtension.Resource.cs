using System.Threading;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public static partial class UniTaskExtension
    {
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static UniTask<T> LoadAssetAsync<T>(this ResourceComponent self, string assetName, CancellationToken cancellationToken = default) where T : Object
        {
            UniTaskCompletionSource<T> loadAssetTcs = new UniTaskCompletionSource<T>();

            self.LoadAsset(assetName, typeof(T), new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback));
            return loadAssetTcs.Task;

            void LoadAssetFailureCallback(string localAssetName, LoadResourceStatus status, string errorMessage, object userData)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    loadAssetTcs.TrySetCanceled();
                    return;
                }

                Log.Warning("Load asset failure status is '{0}' errorMessage is '{1}'.", status, errorMessage);
                loadAssetTcs.TrySetException(new DEngineException(errorMessage));
            }

            void LoadAssetSuccessCallback(string localAssetName, object asset, float duration, object userData)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    loadAssetTcs.TrySetCanceled();
                    return;
                }

                var tAsset = asset as T;
                if (tAsset)
                {
                    Log.Info("Load asset '{0}' OK.", assetName);
                    loadAssetTcs.TrySetResult(tAsset);
                }
                else
                {
                    Log.Warning($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}.");
                    loadAssetTcs.TrySetException(new DEngineException($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}."));
                }
            }
        }

        /// <summary>
        /// 加载多个资源（可等待）
        /// </summary>
        public static async UniTask<T[]> LoadAssetsAsync<T>(this ResourceComponent self, string[] assetName) where T : Object
        {
            if (assetName == null)
            {
                return null;
            }

            UniTask<T>[] tasks = new UniTask<T>[assetName.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = self.LoadAssetAsync<T>(assetName[i]);
            }

            return await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 异步加载二进制资源(可等待)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="binaryAssetName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static UniTask<byte[]> LoadBinaryAsync(this ResourceComponent self, string binaryAssetName, CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<byte[]> loadAssetTcs = new UniTaskCompletionSource<byte[]>();

            void LoadAssetSuccessCallback(string localBinaryAssetName, byte[] binaryBytes, float duration, object userData)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    loadAssetTcs.TrySetCanceled();
                }

                if (binaryBytes != null)
                {
                    loadAssetTcs.TrySetResult(binaryBytes);
                }
                else
                {
                    loadAssetTcs.TrySetException(new DEngineException("Load Binary failure."));
                }
            }

            void LoadAssetFailureCallback(string localBinaryAssetName, LoadResourceStatus status, string errorMessage, object userData)
            {
                loadAssetTcs.TrySetException(new DEngineException(errorMessage));
            }

            self.LoadBinary(binaryAssetName, new LoadBinaryCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback));
            return loadAssetTcs.Task;
        }
    }
}