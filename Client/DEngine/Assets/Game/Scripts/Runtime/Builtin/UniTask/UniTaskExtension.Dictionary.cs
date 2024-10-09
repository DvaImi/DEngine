using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Localization;
using DEngine.Runtime;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<string, AwaitDataWrap<Language>> DictionaryResult = new();

        /// <summary>
        /// 异步获取本地化字典
        /// </summary>
        /// <param name="self"></param>
        /// <param name="dictionaryAssetName"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public static async UniTask<Language> LoadDictionaryAsync(this LocalizationComponent self, string dictionaryAssetName, object userData = null)
        {
            if (string.IsNullOrEmpty(dictionaryAssetName))
            {
                Log.Warning("Dictionary name is invalid.");
                return default;
            }

            if (DictionaryResult.ContainsKey(dictionaryAssetName))
            {
                Log.Warning("The dictionary '{0}' has already been loaded in the task.", dictionaryAssetName);
                return default;
            }

            var result = AwaitDataWrap<Language>.Create(new UniTaskCompletionSource<Language>(), userData);
            DictionaryResult.Add(dictionaryAssetName, result);
            self.ReadData(dictionaryAssetName, userData);
            return await result.Source.Task;
        }

        private static void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            if (e is LoadDictionarySuccessEventArgs ne && DictionaryResult.Remove(ne.DictionaryAssetName, out var result))
            {
                if (result == null)
                {
                    return;
                }

                Log.Info("Load dictionary '{0}' OK.", ne.DictionaryAssetName);
                result.Source.TrySetResult(GameEntry.Localization.Language);
                ReferencePool.Release(result);
            }
        }

        private static void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            if (e is LoadDictionaryFailureEventArgs ne && DictionaryResult.Remove(ne.DictionaryAssetName, out var result))
            {
                if (result == null)
                {
                    return;
                }
                Log.Info("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName, ne.DictionaryAssetName, ne.ErrorMessage);
                result.Source.TrySetException(new DEngineException(Utility.Text.Format("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName, ne.DictionaryAssetName, ne.ErrorMessage)));
                ReferencePool.Release(result);
            }
        }
    }
}