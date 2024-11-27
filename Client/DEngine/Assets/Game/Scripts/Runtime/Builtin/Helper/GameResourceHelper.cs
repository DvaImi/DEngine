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
            var assetBundle = objectToRelease as AssetBundle;
            if (assetBundle)
            {
                Log.Info("Release [{0}] Name {1} at {2}", objectToRelease.GetType().Name, assetBundle.name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                assetBundle.Unload(true);
            }
        }

        private static async UniTask LoadBytesByUniTask(string fileUri, LoadBytesCallbacks loadBytesCallbacks, object userData)
        {
            var startTime = DateTime.UtcNow;
            using var unityWebRequest = UnityWebRequest.Get(fileUri);

            try
            {
                await unityWebRequest.SendWebRequest();

                bool isError = unityWebRequest.result != UnityWebRequest.Result.Success;
                byte[] bytes = unityWebRequest.downloadHandler.data;
                string errorMessage = isError ? unityWebRequest.error : null;

                if (!isError && bytes != null)
                {
                    float elapseSeconds = (float)(DateTime.UtcNow - startTime).TotalSeconds;
                    loadBytesCallbacks.LoadBytesSuccessCallback?.Invoke(fileUri, bytes, elapseSeconds, userData);
                }
                else
                {
                    loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(fileUri, errorMessage ?? "Unknown error", userData);
                }
            }
            catch (Exception ex)
            {
                loadBytesCallbacks.LoadBytesFailureCallback?.Invoke(fileUri, $"Exception: {ex.Message}", userData);
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