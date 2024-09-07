using System.IO;
using Cysharp.Threading.Tasks;
using DEngine.Runtime;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public static class WebRequestExtension
    {
        public static async UniTask<string> AddHeadWebRequest(this WebRequestComponent webRequestComponent, string webRequestUri, string headerResopnse)
        {
            UnityWebRequest request = UnityWebRequest.Head(webRequestUri);
            await request.SendWebRequest();
            return request.GetResponseHeader(headerResopnse);
        }
    }
}