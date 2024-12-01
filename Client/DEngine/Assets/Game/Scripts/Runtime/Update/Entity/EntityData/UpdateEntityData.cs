// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:39:45
// 版 本：1.0
// ========================================================

using DEngine;
using UnityEngine;

namespace Game.Update.Entity
{
    /// <summary>
    /// 热更新层实体数据
    /// </summary>
    public abstract class UpdateEntityData : IReference
    {
        /// <summary>
        /// 实体编号。
        /// </summary>
        public int Id { get; private set; } = 0;

        /// <summary>
        /// 实体类型编号。
        /// 对应数据表的Id
        /// </summary>
        public int TypeId { get; private set; } = 0;

        /// <summary>
        /// 实体位置。
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.zero;

        /// <summary>
        /// 实体朝向。
        /// </summary>
        public Quaternion Rotation { get; set; } = Quaternion.identity;

        public void Fill(int typeId)
        {
            Id     = GameEntry.Entity.GenerateSerialId();
            TypeId = typeId;
        }

        public virtual void Clear()
        {
            Id       = 0;
            TypeId   = 0;
            Position = Vector3.zero;
            Rotation = default(Quaternion);
        }

        public static T Creat<T>(int typeId) where T : UpdateEntityData, new()
        {
            var data = ReferencePool.Acquire<T>();
            data.Fill(typeId);
            return data;
        }
    }
}