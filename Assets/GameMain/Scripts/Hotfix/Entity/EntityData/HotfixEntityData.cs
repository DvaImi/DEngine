// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:39:45
// 版 本：1.0
// ========================================================
using GameFramework;
using UnityEngine;

namespace GeminiLion
{
    /// <summary>
    /// 热更新层实体数据
    /// </summary>
    public class HotfixEntityData : IReference
    {
        /// <summary>
        /// 实体编号。
        /// </summary>
        public int Id { get; private set; } = 0;

        /// <summary>
        /// 实体类型编号。
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

        public HotfixEntityData()
        {

        }

        /// <summary>
        /// 填充实体数据
        /// </summary>
        public void Fill(int id, int typeId)
        {
            Id = id;
            TypeId = typeId;
        }

        public virtual void Clear()
        {
            Id = 0;
            TypeId = 0;
            Position = Vector3.zero;
            Rotation = default(Quaternion);
        }
    }
}