using System;
using DEngine;
using DEngine.Runtime;
using DEngine.WebRequest;
using UnityEngine.Networking;

namespace Game
{
    public class GameWebRequestAgentHelper : WebRequestAgentHelperBase, IDisposable
    {
        private UnityWebRequest m_UnityWebRequest;
        private bool m_Disposed;

        private EventHandler<WebRequestAgentHelperCompleteEventArgs> m_WebRequestAgentHelperCompleteEventHandler;
        private EventHandler<WebRequestAgentHelperErrorEventArgs> m_WebRequestAgentHelperErrorEventHandler;

        /// <summary>
        /// Web 请求代理辅助器完成事件。
        /// </summary>
        public override event EventHandler<WebRequestAgentHelperCompleteEventArgs> WebRequestAgentHelperComplete
        {
            add { m_WebRequestAgentHelperCompleteEventHandler    += value; }
            remove { m_WebRequestAgentHelperCompleteEventHandler -= value; }
        }

        /// <summary>
        /// Web 请求代理辅助器错误事件。
        /// </summary>
        public override event EventHandler<WebRequestAgentHelperErrorEventArgs> WebRequestAgentHelperError
        {
            add { m_WebRequestAgentHelperErrorEventHandler    += value; }
            remove { m_WebRequestAgentHelperErrorEventHandler -= value; }
        }

        /// <summary>
        /// 通过 Web 请求代理辅助器发送请求。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Request(string webRequestUri, object userData)
        {
            if (m_WebRequestAgentHelperCompleteEventHandler == null || m_WebRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }

            var wwwFormInfo = (WWWFormInfo)userData;
            m_UnityWebRequest = wwwFormInfo.WWWForm == null ? UnityWebRequest.Get(webRequestUri) : UnityWebRequest.Post(webRequestUri, wwwFormInfo.WWWForm);

            if (wwwFormInfo.UserData is UnityWebRequestHeader { Header: not null } requestParams)
            {
                foreach (var header in requestParams.Header)
                {
                    m_UnityWebRequest.SetRequestHeader(header.Key, header.Value);
                }

                ReferencePool.Release(requestParams);
            }

            m_UnityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 通过 Web 请求代理辅助器发送请求。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override void Request(string webRequestUri, byte[] postData, object userData)
        {
            if (m_WebRequestAgentHelperCompleteEventHandler == null || m_WebRequestAgentHelperErrorEventHandler == null)
            {
                Log.Fatal("Web request agent helper handler is invalid.");
                return;
            }
#if UNITY_2022_1_OR_NEWER
            m_UnityWebRequest = UnityWebRequest.PostWwwForm(webRequestUri, Utility.Converter.GetString(postData));
#else
            m_UnityWebRequest = UnityWebRequest.Post(webRequestUri, Utility.Converter.GetString(postData));
#endif


            var wwwFormInfo = (WWWFormInfo)userData;

            if (wwwFormInfo.UserData is UnityWebRequestHeader { Header: not null } requestParams)
            {
                foreach (var header in requestParams.Header)
                {
                    m_UnityWebRequest.SetRequestHeader(header.Key, header.Value);
                }

                ReferencePool.Release(requestParams);
            }

            m_UnityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// 重置 Web 请求代理辅助器。
        /// </summary>
        public override void Reset()
        {
            if (m_UnityWebRequest != null)
            {
                m_UnityWebRequest.Dispose();
                m_UnityWebRequest = null;
            }
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">释放资源标记。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (m_UnityWebRequest != null)
                {
                    m_UnityWebRequest.Dispose();
                    m_UnityWebRequest = null;
                }
            }

            m_Disposed = true;
        }

        private void Update()
        {
            if (m_UnityWebRequest == null || !m_UnityWebRequest.isDone)
            {
                return;
            }

            var isError = m_UnityWebRequest.result != UnityWebRequest.Result.Success;

            if (isError)
            {
                WebRequestAgentHelperErrorEventArgs webRequestAgentHelperErrorEventArgs = WebRequestAgentHelperErrorEventArgs.Create(m_UnityWebRequest.error);
                m_WebRequestAgentHelperErrorEventHandler(this, webRequestAgentHelperErrorEventArgs);
                ReferencePool.Release(webRequestAgentHelperErrorEventArgs);
            }
            else if (m_UnityWebRequest.downloadHandler.isDone)
            {
                WebRequestAgentHelperCompleteEventArgs webRequestAgentHelperCompleteEventArgs = WebRequestAgentHelperCompleteEventArgs.Create(m_UnityWebRequest.downloadHandler.data);
                m_WebRequestAgentHelperCompleteEventHandler(this, webRequestAgentHelperCompleteEventArgs);
                ReferencePool.Release(webRequestAgentHelperCompleteEventArgs);
            }
        }
    }
}