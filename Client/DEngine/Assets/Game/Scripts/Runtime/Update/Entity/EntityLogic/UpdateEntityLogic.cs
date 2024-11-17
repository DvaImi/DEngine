// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:36:41
// 版 本：1.0
// ========================================================

using DEngine;
using DEngine.Runtime;

namespace Game.Update.Entity
{
    public abstract class UpdateEntityLogic : EntityLogic
    {
        public VariableContainer VariableContainer { get; private set; }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            VariableContainer = VariableContainer.Create(this);
            VariableContainer.Clear();
        }

        protected override void OnRecycle()
        {
            if (VariableContainer == null)
            {
                return;
            }

            ReferencePool.Release(VariableContainer);
        }
    }
}