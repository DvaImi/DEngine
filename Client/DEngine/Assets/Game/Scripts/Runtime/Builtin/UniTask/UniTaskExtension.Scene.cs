using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> LoadSceneResult = new();
        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> UnLoadSceneResult = new();

        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static async UniTask<bool> LoadSceneAsync(this SceneComponent self, string sceneAssetName, object userData = null)
        {
            UniTaskCompletionSource<bool> result = new UniTaskCompletionSource<bool>();
            var isUnLoadScene = UnLoadSceneResult.TryGetValue(sceneAssetName, out var unloadSceneTcs);
            if (isUnLoadScene)
            {
                await unloadSceneTcs.Task;
            }

            LoadSceneResult.Add(sceneAssetName, result);

            try
            {
                self.LoadScene(sceneAssetName, userData);
            }
            catch (Exception e)
            {
                result.TrySetException(e);
                LoadSceneResult.Remove(sceneAssetName);
            }

            return await result.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            if (e is LoadSceneSuccessEventArgs ne && LoadSceneResult.Remove(ne.SceneAssetName, out var result))
            {
                result?.TrySetResult(true);
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            if (e is LoadSceneFailureEventArgs ne && LoadSceneResult.Remove(ne.SceneAssetName, out var result))
            {
                result.TrySetException(new DEngineException(ne.ErrorMessage));
            }
        }

        /// <summary>
        /// 卸载场景（可等待）
        /// </summary>
        public static async UniTask<bool> UnLoadSceneAsync(this SceneComponent self, string sceneAssetName)
        {
            var result = new UniTaskCompletionSource<bool>();
            if (LoadSceneResult.TryGetValue(sceneAssetName, out var loadSceneTcs))
            {
                Debug.Log("Unload  loading scene");
                await loadSceneTcs.Task;
            }

            UnLoadSceneResult.Add(sceneAssetName, result);
            try
            {
                self.UnloadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                result.TrySetException(e);
                UnLoadSceneResult.Remove(sceneAssetName);
            }

            return await result.Task;
        }

        private static void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            if (e is UnloadSceneSuccessEventArgs ne && UnLoadSceneResult.Remove(ne.SceneAssetName, out var result))
            {
                result?.TrySetResult(true);
            }
        }

        private static void OnUnloadSceneFailure(object sender, GameEventArgs e)
        {
            if (e is UnloadSceneFailureEventArgs ne && UnLoadSceneResult.Remove(ne.SceneAssetName, out var result))
            {
                result?.TrySetException(new DEngineException($"Unload scene {ne.SceneAssetName} failure."));
            }
        }
    }
}