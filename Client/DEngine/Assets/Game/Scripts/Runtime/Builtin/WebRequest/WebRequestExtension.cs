using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Runtime;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public static class WebRequestExtension
    {
        public static async UniTask<string> AddHeadWebRequest(this WebRequestComponent self, string webRequestUri, string headerResopnse)
        {
            UnityWebRequest request = UnityWebRequest.Head(webRequestUri);
            await request.SendWebRequest();
            return request.GetResponseHeader(headerResopnse);
        }

        public static void Get(this WebRequestComponent self, string webRequestUri, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Get(webRequestUri);

            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }

        public static void Get(this WebRequestComponent self, string webRequestUri, UnityWebRequestHeader requestHeader, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Get(webRequestUri, requestHeader);

            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }

        public static void Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Post(webRequestUri, wwwForm);
            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }

        public static void Post(this WebRequestComponent self, string webRequestUri, WWWForm wwwForm, UnityWebRequestHeader requestHeader, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Post(webRequestUri, wwwForm, requestHeader);
            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }

        public static void Post(this WebRequestComponent self, string webRequestUri, byte[] postData, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Post(webRequestUri, postData);
            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }

        public static void Post(this WebRequestComponent self, string webRequestUri, byte[] postData, UnityWebRequestHeader requestHeader, DEngineAction<WebRequestResult> callback)
        {
            var webTask = self.Post(webRequestUri, postData, requestHeader);
            webTask.GetAwaiter().OnCompleted(Continuation);
            return;

            void Continuation()
            {
                callback?.Invoke(webTask.GetAwaiter().GetResult());
            }
        }
    }
}