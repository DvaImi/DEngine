// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 01:20:01
// 版 本：1.0
// ========================================================
using System.Threading.Tasks;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 1:20:01
namespace GeminiLion.Hotfix
{
    public static partial class ShowEntityExtension
    {
        public static void ShowEnitityPlane(this EntityComponent entityComponent, EnitityPlaneData data)
        {
            entityComponent.ShowHotfixEntity(typeof(EnitityPlaneLogic), "Player", 0, data);
        }

        //public static async Task<Entity> AwaitShowEnitityPlane(this EntityComponent entityComponent, EnitityPlaneData data)
        //{
        //    Dvalmi.HotfixEntityData tData = GameFramework.ReferencePool.Acquire<Dvalmi.HotfixEntityData>();
        //    tData.Fill(data.Id, data.TypeId);
        //    tData.Position = data.Position;
        //    tData.Rotation = data.Rotation;

        //    Entity entity = await entityComponent.AwaitShowEntity(typeof(EnitityPlaneLogic), 0, tData);
        //    return entity;
        //}

    }
}
