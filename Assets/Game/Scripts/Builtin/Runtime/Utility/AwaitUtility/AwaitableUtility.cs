using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace Game
{
    public static partial class AwaitableUtility
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<UIForm>> m_UIFormTcs = new Dictionary<int, UniTaskCompletionSource<UIForm>>();

        private static readonly Dictionary<int, UniTaskCompletionSource<Entity>> m_EntityTcs = new Dictionary<int, UniTaskCompletionSource<Entity>>();

        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> m_LoadSceneTcs = new Dictionary<string, UniTaskCompletionSource<bool>>();

        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> m_UnLoadSceneTcs = new Dictionary<string, UniTaskCompletionSource<bool>>();

        private static readonly HashSet<int> m_WebSerialIDs = new HashSet<int>();
        private static readonly List<WebRequestResult> m_DelayReleaseWebResult = new List<WebRequestResult>();

        private static readonly HashSet<int> m_DownloadSerialIds = new HashSet<int>();
        private static readonly List<DownLoadResult> m_DelayReleaseDownloadResult = new List<DownLoadResult>();

        /// <summary>
        /// 注册需要的事件，需要在流程入口注册以防框架重启导致事件被取消
        /// </summary>
        public static void Subscribe()
        {
            EventComponent eventComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }

            eventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            eventComponent.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        }


        #region UI

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static UniTask<UIForm> OpenUIFormAsync(this UIComponent uiComponent, string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
            int serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
            UniTaskCompletionSource<UIForm> tcs = new UniTaskCompletionSource<UIForm>();
            m_UIFormTcs.Add(serialId, tcs);
            return tcs.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            m_UIFormTcs.TryGetValue(ne.UIForm.SerialId, out UniTaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                tcs.TrySetResult(ne.UIForm);
                m_UIFormTcs.Remove(ne.UIForm.SerialId);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = (OpenUIFormFailureEventArgs)e;
            m_UIFormTcs.TryGetValue(ne.SerialId, out UniTaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.TrySetException(new GameFrameworkException(ne.ErrorMessage));
                m_UIFormTcs.Remove(ne.SerialId);
            }
        }

        #endregion

        #region Entity

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static UniTask<Entity> ShowEntityAsync(this EntityComponent entityComponent, int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            UniTaskCompletionSource<Entity> tcs = new UniTaskCompletionSource<Entity>();
            m_EntityTcs.Add(entityId, tcs);
            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, userData);
            return tcs.Task;
        }


        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            m_EntityTcs.TryGetValue(ne.Entity.Id, out var tcs);
            if (tcs != null)
            {
                tcs.TrySetResult(ne.Entity);
                m_EntityTcs.Remove(ne.Entity.Id);
            }
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            m_EntityTcs.TryGetValue(ne.EntityId, out var tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.TrySetException(new GameFrameworkException(ne.ErrorMessage));
                m_EntityTcs.Remove(ne.EntityId);
            }
        }

        #endregion

        #region Scene

        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static async UniTask<bool> LoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
            UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();
            var isUnLoadScene = m_UnLoadSceneTcs.TryGetValue(sceneAssetName, out var unloadSceneTcs);
            if (isUnLoadScene)
            {
                await unloadSceneTcs.Task;
            }

            m_LoadSceneTcs.Add(sceneAssetName, tcs);

            try
            {
                sceneComponent.LoadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                tcs.TrySetException(e);
                m_LoadSceneTcs.Remove(sceneAssetName);
            }

            return await tcs.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            m_LoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.TrySetResult(true);
                m_LoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            m_LoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.TrySetException(new GameFrameworkException(ne.ErrorMessage));
                m_LoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        /// <summary>
        /// 卸载场景（可等待）
        /// </summary>
        public static async UniTask<bool> UnLoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var isLoadSceneTcs = m_LoadSceneTcs.TryGetValue(sceneAssetName, out var loadSceneTcs);
            if (isLoadSceneTcs)
            {
                Debug.Log("Unload  loading scene");
                await loadSceneTcs.Task;
            }

            m_UnLoadSceneTcs.Add(sceneAssetName, tcs);
            try
            {
                sceneComponent.UnloadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                tcs.TrySetException(e);
                m_UnLoadSceneTcs.Remove(sceneAssetName);
            }

            return await tcs.Task;
        }

        private static void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            UnloadSceneSuccessEventArgs ne = (UnloadSceneSuccessEventArgs)e;
            m_UnLoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.TrySetResult(true);
                m_UnLoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        private static void OnUnloadSceneFailure(object sender, GameEventArgs e)
        {
            UnloadSceneFailureEventArgs ne = (UnloadSceneFailureEventArgs)e;
            m_UnLoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                Debug.LogError($"Unload scene {ne.SceneAssetName} failure.");
                tcs.TrySetException(new GameFrameworkException($"Unload scene {ne.SceneAssetName} failure."));
                m_UnLoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        #endregion

        #region Resources

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static UniTask<T> LoadAssetAsync<T>(this ResourceComponent resourceComponent, string assetName) where T : Object
        {
            UniTaskCompletionSource<T> loadAssetTcs = new UniTaskCompletionSource<T>();
            resourceComponent.LoadAsset(assetName, typeof(T), new LoadAssetCallbacks(
                (tempAssetName, asset, duration, userdata) =>
                {
                    var source = loadAssetTcs;
                    loadAssetTcs = null;
                    T tAsset = asset as T;
                    if (tAsset != null)
                    {
                        source.TrySetResult(tAsset);
                    }
                    else
                    {
                        Debug.LogError($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}.");
                        source.TrySetException(new GameFrameworkException($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}."));
                    }
                },
                (tempAssetName, status, errorMessage, userdata) =>
                {
                    Debug.LogError(errorMessage);
                    loadAssetTcs.TrySetException(new GameFrameworkException(errorMessage));
                }
            ));

            return loadAssetTcs.Task;
        }

        /// <summary>
        /// 加载多个资源（可等待）
        /// </summary>
        public static async UniTask<T[]> LoadAssetsAsync<T>(this ResourceComponent resourceComponent, string[] assetName) where T : Object
        {
            if (assetName == null)
            {
                return null;
            }

            T[] assets = new T[assetName.Length];
            UniTask<T>[] tasks = new UniTask<T>[assets.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = resourceComponent.LoadAssetAsync<T>(assetName[i]);
            }

            await UniTask.WhenAll(tasks);

            for (int i = 0; i < assets.Length; i++)
            {
                assets[i] = tasks[i].GetAwaiter().GetResult();
            }

            return assets;
        }

        #endregion

        #region WebRequest

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebRequestResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri, WWWForm wwwForm = null, object userdata = null)
        {
            UniTaskCompletionSource<WebRequestResult> tsc = new UniTaskCompletionSource<WebRequestResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm, AwaitDataWrap<WebRequestResult>.Create(userdata, tsc));
            m_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebRequestResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri, byte[] postData, object userdata = null)
        {
            UniTaskCompletionSource<WebRequestResult> tsc = new UniTaskCompletionSource<WebRequestResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData, AwaitDataWrap<WebRequestResult>.Create(userdata, tsc));
            m_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (m_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebRequestResult> webRequestUserdata)
                {
                    WebRequestResult requestResult = WebRequestResult.Create(ne.GetWebResponseBytes(), false, string.Empty, webRequestUserdata.UserData);
                    m_DelayReleaseWebResult.Add(requestResult);
                    webRequestUserdata.Source.TrySetResult(requestResult);
                    ReferencePool.Release(webRequestUserdata);
                }

                m_WebSerialIDs.Remove(ne.SerialId);
                if (m_WebSerialIDs.Count == 0)
                {
                    for (int i = 0; i < m_DelayReleaseWebResult.Count; i++)
                    {
                        ReferencePool.Release(m_DelayReleaseWebResult[i]);
                    }

                    m_DelayReleaseWebResult.Clear();
                }
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (m_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebRequestResult> webRequestUserdata)
                {
                    WebRequestResult requestResult = WebRequestResult.Create(null, true, ne.ErrorMessage, webRequestUserdata.UserData);
                    webRequestUserdata.Source.TrySetResult(requestResult);
                    m_DelayReleaseWebResult.Add(requestResult);
                    ReferencePool.Release(webRequestUserdata);
                }

                m_WebSerialIDs.Remove(ne.SerialId);
                if (m_WebSerialIDs.Count == 0)
                {
                    for (int i = 0; i < m_DelayReleaseWebResult.Count; i++)
                    {
                        ReferencePool.Release(m_DelayReleaseWebResult[i]);
                    }

                    m_DelayReleaseWebResult.Clear();
                }
            }
        }

        #endregion

        #region DownLoad

        /// <summary>
        /// 增加下载任务（可等待)
        /// </summary>
        public static UniTask<DownLoadResult> AddDownloadAsync(this DownloadComponent downloadComponent, string downloadPath, string downloadUri, object userdata = null)
        {
            var tcs = new UniTaskCompletionSource<DownLoadResult>();
            int serialId = downloadComponent.AddDownload(downloadPath, downloadUri, AwaitDataWrap<DownLoadResult>.Create(userdata, tcs));
            m_DownloadSerialIds.Add(serialId);
            return tcs.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            if (m_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(false, string.Empty, awaitDataWrap.UserData);
                    m_DelayReleaseDownloadResult.Add(result);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                m_DownloadSerialIds.Remove(ne.SerialId);
                if (m_DownloadSerialIds.Count == 0)
                {
                    for (int i = 0; i < m_DelayReleaseDownloadResult.Count; i++)
                    {
                        ReferencePool.Release(m_DelayReleaseDownloadResult[i]);
                    }

                    m_DelayReleaseDownloadResult.Clear();
                }
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;
            if (m_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(true, ne.ErrorMessage, awaitDataWrap.UserData);
                    m_DelayReleaseDownloadResult.Add(result);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                m_DownloadSerialIds.Remove(ne.SerialId);
                if (m_DownloadSerialIds.Count == 0)
                {
                    for (int i = 0; i < m_DelayReleaseDownloadResult.Count; i++)
                    {
                        ReferencePool.Release(m_DelayReleaseDownloadResult[i]);
                    }

                    m_DelayReleaseDownloadResult.Clear();
                }
            }
        }

        #endregion
    }
}