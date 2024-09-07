using System;
using DEngine;

namespace Game.Update
{
    public class VarType : Variable<Type>
    {
        /// <summary>
        /// 初始化 Type 变量类的新实例。
        /// </summary>
        public VarType()
        {
        }

        /// <summary>
        /// 从 Type 到 Type 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarType(Type value)
        {
            VarType varValue = ReferencePool.Acquire<VarType>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 VariableProcedure 变量类到 VariableProcedure 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator Type(VarType value)
        {
            return value.Value;
        }
    }
}