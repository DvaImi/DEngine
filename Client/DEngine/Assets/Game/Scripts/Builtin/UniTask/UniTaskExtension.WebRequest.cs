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
        private static readonly Dictionary<int, UniTaskCompletionSource<WebRequestResult>> WebRequestResult = new();

        public static UniTask<WebRequestResult> Get(this WebRequestComponent self, string webRequestUri)
        {
            int serialId = self.AddWebRequest(webRequestUri);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        public static UniTask<WebRequestResult> Get(this WebRequestComponent self, string webRequestUri, UnityWebRequestHeader requestParams)
        {
            int serialId = self.AddWebRequest(webRequestUri, userData: requestParams);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm = null)
        {
            int serialId = self.AddWebRequest(webRequestUri, wwwForm);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm, UnityWebRequestHeader requestParams)
        {
            int serialId = self.AddWebRequest(webRequestUri, wwwForm, userData: requestParams);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, byte[] postData)
        {
            int serialId = self.AddWebRequest(webRequestUri, postData);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
        }

        public static UniTask<WebRequestResult> Post(this WebRequestComponent self, string webRequestUri, byte[] postData, UnityWebRequestHeader requestParams)
        {
            int serialId = self.AddWebRequest(webRequestUri, postData, userData: requestParams);
            UniTaskCompletionSource<WebRequestResult> result = new UniTaskCompletionSource<WebRequestResult>();
            WebRequestResult.Add(serialId, result);
            return result.Task;
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