using System;
using Cysharp.Threading.Tasks;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameResourceHelper : ResourceHelperBase
    {
        /// <summary>
        /// 直接从指定文件路径加载数据流。
        /// </summary>
        /// <param name="fileUri">文件路径。</param>
        /// <param name="loadBytesCallbacks">加载数据流回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void LoadBytes(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            LoadBytesByUniTask(fileUri, loadBytesCallbacks, userData).Forget();
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            if (gameObject.activeInHierarchy)
            {
                UnloadSceneByUniTask(sceneAssetName, unloadSceneCallbacks, userData).Forget();
            }
            else
            {
                SceneManager.UnloadSceneAsync(SceneComponent.GetSceneName(sceneAssetName));
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="objectToRelease">要释放的资源。</param>
        public override void Release(object objectToRelease)
        {
            Log.Info("Release" + objectToRelease.GetType().Name);
            var assetBundle = objectToRelease as AssetBundle;
            if (assetBundle)
            {
                assetBundle.Unload(true);
            }
        }

        private static async UniTaskVoid LoadBytesByUniTask(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            var startTime = DateTime.UtcNow;
            using var unityWebRequest = UnityWebRequest.Get(fileUri);
            await unityWebRequest.SendWebRequest();
            var isError = unityWebRequest.result != UnityWebRequest.Result.Success;
            var bytes = unityWebRequest.downloadHandler.data;
            var errorMessage = isError ? unityWebRequest.error : null;

            if (!isError)
            {
                float elapseSeconds = (float)(DateTime.UtcNow - startTime).TotalSeconds;
                loadBytesCallbacks.LoadBytesSuccessCallback(fileUri, bytes, elapseSeconds, userData);
            }
            else
            {
                loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(fileUri, errorMessage, userData);
            }
        }

        private static async UniTaskVoid UnloadSceneByUniTask(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
        {
            var asyncOperation = SceneManager.UnloadSceneAsync(SceneComponent.GetSceneName(sceneAssetName));
            if (asyncOperation == null)
            {
                return;
            }

            await asyncOperation;

            if (asyncOperation.allowSceneActivation)
            {
                unloadSceneCallbacks.UnloadSceneSuccessCallback?.Invoke(sceneAssetName, userData);
            }
            else
            {
                unloadSceneCallbacks.UnloadSceneFailureCallback?.Invoke(sceneAssetName, userData);
            }
        }
    }
}