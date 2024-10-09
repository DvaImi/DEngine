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
        private static readonly Dictionary<int, UniTaskCompletionSource<UIForm>> UIFormResult = new();

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static UniTask<UIForm> OpenUIFormAsync(this UIComponent self, string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
            int serialId = self.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
            UniTaskCompletionSource<UIForm> result = new UniTaskCompletionSource<UIForm>();
            UIFormResult.Add(serialId, result);
            return result.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            if (e is OpenUIFormSuccessEventArgs ne && UIFormResult.Remove(ne.UIForm.SerialId, out var result))
            {
                result?.TrySetResult(ne.UIForm);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            if (e is OpenUIFormFailureEventArgs ne && UIFormResult.Remove(ne.SerialId, out var result))
            {
                result?.TrySetException(new DEngineException(ne.ErrorMessage));
            }
        }
    }
}