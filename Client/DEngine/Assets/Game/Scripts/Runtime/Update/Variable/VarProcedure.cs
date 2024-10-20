using System;
using DEngine;

namespace Game.Update
{
    public class VarProcedure : VarType
    {
        /// <summary>
        /// 从 Type 到 Type 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarProcedure(Type value)
        {
            VarProcedure varValue = ReferencePool.Acquire<VarProcedure>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 VariableProcedure 变量类到 VariableProcedure 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator Type(VarProcedure value)
        {
            return value.Value;
        }
    }
}
