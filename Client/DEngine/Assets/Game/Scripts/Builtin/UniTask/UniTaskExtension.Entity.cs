using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;

namespace Game
{
    public static partial class UniTaskExtension
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<Entity>> EntityResult = new();

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static UniTask<Entity> ShowEntityAsync(this EntityComponent self, int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            UniTaskCompletionSource<Entity> result = new UniTaskCompletionSource<Entity>();
            EntityResult.Add(entityId, result);
            self.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, userData);
            return result.Task;
        }

        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            if (e is ShowEntitySuccessEventArgs ne && EntityResult.Remove(ne.Entity.Id, out var result))
            {
                result?.TrySetResult(ne.Entity);
            }
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            if (e is ShowEntityFailureEventArgs ne && EntityResult.Remove(ne.EntityId, out var result))
            {
                result.TrySetException(new DEngineException(ne.ErrorMessage));
            }
        }
    }
}