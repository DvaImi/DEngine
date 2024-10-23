using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<WebRequestResult>> WebRequestResult = new();

        public static UniTask<WebRequestResult> Get(this WebRequestComponent self, string webRequestUri, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        public static UniTask<WebRequestResult> Get(this WebRequestComponent self, string webRequestUri, UnityWebRequestHeader requestHeader, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, userData: requestHeader);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm = null, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, wwwForm);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm, UnityWebRequestHeader requestHeader, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, wwwForm, userData: requestHeader);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, byte[] postData, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, postData);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, byte[] postData, UnityWebRequestHeader requestHeader, CancellationToken? cancellationToken = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, postData, userData: requestHeader);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            cancellationToken?.Register(CancelCallback);
            return result.Task;

            void CancelCallback()
            {
                self.RemoveWebRequest(serialId);
                WebRequestResult.Remove(serialId);
                result.TrySetCanceled();
            }
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            if (e is WebRequestSuccessEventArgs ne && WebRequestResult.Remove(ne.SerialId, out var result))
            {
                if (result != null)
                {
                    WebRequestResult requestResult = Game.WebRequestResult.Create(ne.GetWebResponseBytes(), true, null, ne.UserData);
                    result.TrySetResult(requestResult);
                    ReferencePool.Release(requestResult);
                }
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            if (e is WebRequestFailureEventArgs ne && WebRequestResult.Remove(ne.SerialId, out var result))
            {
                if (result != null)
                {
                    WebRequestResult requestResult = Game.WebRequestResult.Create(null, false, ne.ErrorMessage, ne.UserData);
                    result.TrySetResult(requestResult);
                    ReferencePool.Release(requestResult);
                }
            }
        }
    }
}