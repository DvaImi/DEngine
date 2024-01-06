using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public static partial class AwaitableUtility
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<UIForm>> m_UIFormResult = new Dictionary<int, UniTaskCompletionSource<UIForm>>();
        private static readonly Dictionary<int, UniTaskCompletionSource<Entity>> m_EntityResult = new Dictionary<int, UniTaskCompletionSource<Entity>>();
        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> m_LoadSceneResult = new Dictionary<string, UniTaskCompletionSource<bool>>();
        private static readonly Dictionary<string, UniTaskCompletionSource<bool>> m_UnLoadSceneResult = new Dictionary<string, UniTaskCompletionSource<bool>>();
        private static readonly Dictionary<int, UniTaskCompletionSource<WebRequestResult>> m_WebRequestResult = new Dictionary<int, UniTaskCompletionSource<WebRequestResult>>();
        private static readonly Dictionary<int, UniTaskCompletionSource<DownLoadResult>> m_DownloadResult = new Dictionary<int, UniTaskCompletionSource<DownLoadResult>>();

        /// <summary>
        /// 注册需要的事件，需要在流程入口注册以防框架重启导致事件被取消
        /// </summary>
        public static void Subscribe()
        {
            EventComponent eventComponent = DEngine.Runtime.GameEntry.GetComponent<EventComponent>();

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

        public static void Unsubscribe()
        {
            EventComponent eventComponent = DEngine.Runtime.GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }

            eventComponent.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            eventComponent.Unsubscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.Unsubscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            eventComponent.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
        }


        #region UI

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static UniTask<UIForm> OpenUIFormAsync(this UIComponent uiComponent, string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
            int serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
            UniTaskCompletionSource<UIForm> result = new UniTaskCompletionSource<UIForm>();
            m_UIFormResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            m_UIFormResult.TryGetValue(ne.UIForm.SerialId, out UniTaskCompletionSource<UIForm> result);
            if (result != null)
            {
                result.TrySetResult(ne.UIForm);
                m_UIFormResult.Remove(ne.UIForm.SerialId);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = (OpenUIFormFailureEventArgs)e;
            m_UIFormResult.TryGetValue(ne.SerialId, out UniTaskCompletionSource<UIForm> result);
            if (result != null)
            {
                Debug.LogError(ne.ErrorMessage);
                result.TrySetException(new DEngineException(ne.ErrorMessage));
                m_UIFormResult.Remove(ne.SerialId);
            }
        }

        #endregion

        #region Entity

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static UniTask<Entity> ShowEntityAsync(this EntityComponent entityComponent, int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            UniTaskCompletionSource<Entity> result = new UniTaskCompletionSource<Entity>();
            m_EntityResult.Add(entityId, result);
            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, userData);
            return result.Task;
        }

        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            m_EntityResult.TryGetValue(ne.Entity.Id, out var result);
            if (result != null)
            {
                result.TrySetResult(ne.Entity);
                m_EntityResult.Remove(ne.Entity.Id);
            }
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            m_EntityResult.TryGetValue(ne.EntityId, out var result);
            if (result != null)
            {
                Debug.LogError(ne.ErrorMessage);
                result.TrySetException(new DEngineException(ne.ErrorMessage));
                m_EntityResult.Remove(ne.EntityId);
            }
        }

        #endregion

        #region Scene

        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static async UniTask<bool> LoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
            UniTaskCompletionSource<bool> result = new UniTaskCompletionSource<bool>();
            var isUnLoadScene = m_UnLoadSceneResult.TryGetValue(sceneAssetName, out var unloadSceneTcs);
            if (isUnLoadScene)
            {
                await unloadSceneTcs.Task;
            }

            m_LoadSceneResult.Add(sceneAssetName, result);

            try
            {
                sceneComponent.LoadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                result.TrySetException(e);
                m_LoadSceneResult.Remove(sceneAssetName);
            }

            return await result.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            m_LoadSceneResult.TryGetValue(ne.SceneAssetName, out var result);
            if (result != null)
            {
                result.TrySetResult(true);
                m_LoadSceneResult.Remove(ne.SceneAssetName);
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            m_LoadSceneResult.TryGetValue(ne.SceneAssetName, out var result);
            if (result != null)
            {
                Debug.LogError(ne.ErrorMessage);
                result.TrySetException(new DEngineException(ne.ErrorMessage));
                m_LoadSceneResult.Remove(ne.SceneAssetName);
            }
        }

        /// <summary>
        /// 卸载场景（可等待）
        /// </summary>
        public static async UniTask<bool> UnLoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
            var result = new UniTaskCompletionSource<bool>();
            var isLoadSceneTcs = m_LoadSceneResult.TryGetValue(sceneAssetName, out var loadSceneTcs);
            if (isLoadSceneTcs)
            {
                Debug.Log("Unload  loading scene");
                await loadSceneTcs.Task;
            }

            m_UnLoadSceneResult.Add(sceneAssetName, result);
            try
            {
                sceneComponent.UnloadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                result.TrySetException(e);
                m_UnLoadSceneResult.Remove(sceneAssetName);
            }

            return await result.Task;
        }

        private static void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            UnloadSceneSuccessEventArgs ne = (UnloadSceneSuccessEventArgs)e;
            m_UnLoadSceneResult.TryGetValue(ne.SceneAssetName, out var result);
            if (result != null)
            {
                result.TrySetResult(true);
                m_UnLoadSceneResult.Remove(ne.SceneAssetName);
            }
        }

        private static void OnUnloadSceneFailure(object sender, GameEventArgs e)
        {
            UnloadSceneFailureEventArgs ne = (UnloadSceneFailureEventArgs)e;
            m_UnLoadSceneResult.TryGetValue(ne.SceneAssetName, out var result);
            if (result != null)
            {
                Debug.LogError($"Unload scene {ne.SceneAssetName} failure.");
                result.TrySetException(new DEngineException($"Unload scene {ne.SceneAssetName} failure."));
                m_UnLoadSceneResult.Remove(ne.SceneAssetName);
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
            resourceComponent.LoadAsset(assetName, typeof(T), new LoadAssetCallbacks((tempAssetName, asset, duration, userdata) =>
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
                    source.TrySetException(new DEngineException($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}."));
                }
            },
             (tempAssetName, status, errorMessage, userdata) =>
             {
                 Debug.LogError(errorMessage);
                 loadAssetTcs.TrySetException(new DEngineException(errorMessage));
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

            UniTask<T>[] tasks = new UniTask<T>[assetName.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = resourceComponent.LoadAssetAsync<T>(assetName[i]);
            }
            return await UniTask.WhenAll(tasks);
        }

        #endregion

        #region WebRequest

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址</param>
        /// <param name="wwwForm">WWW 表单</param>
        /// <param name="userdata">用户自定义数据。</param>
        /// <returns></returns>
        public static UniTask<WebRequestResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri, WWWForm wwwForm = null, object userdata = null)
        {
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm, userdata);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            m_WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        /// <summary>
        /// 增加Web请求任务，带请求头（可等待）
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址</param>
        /// <param name="wwwForm">WWW 表单</param>
        /// <param name="requestParams">自定义请求参数。</param>
        /// <returns></returns>
        public static UniTask<WebRequestResult> AddWebRequestWithHeaderAsync(this WebRequestComponent webRequestComponent, string webRequestUri, UnityWebRequestHeader requestParams = null, WWWForm wwwForm = null)
        {
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm, requestParams);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            m_WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebRequestResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent, string webRequestUri, byte[] postData, object userdata = null)
        {
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData, userdata);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            m_WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static UniTask<WebRequestResult> AddWebRequestWithHeaderAsync(this WebRequestComponent webRequestComponent, string webRequestUri, byte[] postData, UnityWebRequestHeader requestParams = null)
        {
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData, requestParams);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            m_WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (m_WebRequestResult.TryGetValue(ne.SerialId, out UniTaskCompletionSource<WebRequestResult> result))
            {
                if (result != null)
                {
                    WebRequestResult requestResult = WebRequestResult.Create(ne.GetWebResponseBytes(), true, null, ne.UserData);
                    result.TrySetResult(requestResult);
                    ReferencePool.Release(requestResult);
                    m_WebRequestResult.Remove(ne.SerialId);
                }
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;

            if (m_WebRequestResult.TryGetValue(ne.SerialId, out UniTaskCompletionSource<WebRequestResult> result))
            {
                if (result != null)
                {
                    WebRequestResult requestResult = WebRequestResult.Create(null, false, ne.ErrorMessage, ne.UserData);
                    result.TrySetResult(requestResult);
                    ReferencePool.Release(requestResult);
                    m_WebRequestResult.Remove(ne.SerialId);
                }
            }
        }

        #endregion

        #region DownLoad

        /// <summary>
        /// 增加下载任务。
        /// </summary>
        /// <param name="downloadPath">下载后存放路径。</param>
        /// <param name="downloadUri">原始下载地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public static UniTask<DownLoadResult> AddDownloadAsync(this DownloadComponent downloadComponent, string downloadPath, string downloadUri, object userdata = null)
        {
            int serialId = downloadComponent.AddDownload(downloadPath, downloadUri, userdata);
            UniTaskCompletionSource<DownLoadResult> result = new UniTaskCompletionSource<DownLoadResult>();
            m_DownloadResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            if (m_DownloadResult.TryGetValue(ne.SerialId, out UniTaskCompletionSource<DownLoadResult> result))
            {
                DownLoadResult downLoadResult = DownLoadResult.Create(true, null, ne.DownloadPath, ne.DownloadUri, ne.CurrentLength, ne.UserData);
                result.TrySetResult(downLoadResult);
                ReferencePool.Release(downLoadResult);
                m_DownloadResult.Remove(ne.SerialId);
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;

            if (m_DownloadResult.TryGetValue(ne.SerialId, out UniTaskCompletionSource<DownLoadResult> result))
            {
                DownLoadResult downLoadResult = DownLoadResult.Create(false, ne.ErrorMessage, ne.DownloadPath, ne.DownloadUri, 0, ne.UserData);
                result.TrySetResult(downLoadResult);
                ReferencePool.Release(downLoadResult);
                m_DownloadResult.Remove(ne.SerialId);
            }
        }

        #endregion
    }
}