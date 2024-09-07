using Cysharp.Threading.Tasks;
using DEngine.DataTable;
using DEngine.Runtime;

namespace Game.Update
{
    public static class SceneExtension
    {
        public static void LoadScene(this SceneComponent self, SceneId sceneId, object userData = null)
        {
            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            DRScene drScene = dtScene.GetDataRow((int)sceneId);
            if (drScene == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", sceneId.ToString());
                return;
            }

            self.LoadScene(UpdateAssetUtility.GetSceneAsset(drScene.AssetName), Constant.AssetPriority.SceneAsset, userData);
        }

        public static async UniTask LoadSceneAsync(this SceneComponent self, SceneId sceneId, object userData = null)
        {
            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            DRScene drScene = dtScene.GetDataRow((int)sceneId);
            if (drScene == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", sceneId.ToString());
                return;
            }

            await self.LoadSceneAsync(UpdateAssetUtility.GetSceneAsset(drScene.AssetName), userData);
        }

        public static void UnloadScene(this SceneComponent self, SceneId sceneId)
        {
            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            DRScene drScene = dtScene.GetDataRow((int)sceneId);
            if (drScene == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", sceneId.ToString());
                return;
            }

            self.UnloadScene(UpdateAssetUtility.GetSceneAsset(drScene.AssetName));
        }
    }
}
