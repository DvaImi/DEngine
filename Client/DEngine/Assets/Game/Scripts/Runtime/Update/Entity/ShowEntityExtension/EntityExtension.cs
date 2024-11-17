using System;
using Cysharp.Threading.Tasks;
using DEngine.DataTable;
using DEngine.Runtime;

namespace Game.Update.Entity
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId = 0;

        public static void ShowEntity(this EntityComponent self, Type logicType, string entityGroup, int priority, UpdateEntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity             drEntity = dtEntity.GetDataRow(data.TypeId);
            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }

            self.ShowEntity(data.Id, logicType, UpdateAssetUtility.GetEntityAsset(drEntity.AssetName), entityGroup, priority, data);
        }

        public static async UniTask<T> ShowEntityAsync<T>(this EntityComponent self, string entityGroup, int priority, UpdateEntityData data) where T : UpdateEntityLogic
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return null;
            }

            IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity             drEntity = dtEntity.GetDataRow(data.TypeId);
            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return null;
            }

            DEngine.Runtime.Entity entity = await self.ShowEntityAsync(data.Id, typeof(T), UpdateAssetUtility.GetEntityAsset(drEntity.AssetName), entityGroup, priority, data);

            if (entity == null)
            {
                return null;
            }

            return entity.Logic as T;
        }

        public static int GenerateSerialId(this EntityComponent self)
        {
            return --s_SerialId;
        }
    }
}