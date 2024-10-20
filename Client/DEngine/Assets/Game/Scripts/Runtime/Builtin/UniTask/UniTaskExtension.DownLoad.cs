using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<DownLoadResult>> DownloadResult = new();

        /// <summary>
        /// 增加下载任务。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="downloadPath">下载后存放路径。</param>
        /// <param name="downloadUri">原始下载地址。</param>
        /// <param name="userdata"></param>
        public static UniTask<DownLoadResult> AddDownloadAsync(this DownloadComponent self, string downloadPath, string downloadUri, object userdata = null)
        {
            int serialId = self.AddDownload(downloadPath, downloadUri, userdata);
            UniTaskCompletionSource<DownLoadResult> result = new UniTaskCompletionSource<DownLoadResult>();
            DownloadResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            if (e is DownloadSuccessEventArgs ne && DownloadResult.Remove(ne.SerialId, out var result))
            {
                DownLoadResult downLoadResult = DownLoadResult.Create(true, null, ne.DownloadPath, ne.DownloadUri, ne.CurrentLength, ne.UserData);
                result.TrySetResult(downLoadResult);
                ReferencePool.Release(downLoadResult);
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            if (e is DownloadFailureEventArgs ne && DownloadResult.Remove(ne.SerialId, out var result))
            {
                DownLoadResult downLoadResult = DownLoadResult.Create(false, ne.ErrorMessage, ne.DownloadPath, ne.DownloadUri, 0, ne.UserData);
                result.TrySetResult(downLoadResult);
                ReferencePool.Release(downLoadResult);
            }
        }
    }
}