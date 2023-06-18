//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://DEngine.cn/
// Feedback: mailto:ellan@DEngine.cn
//------------------------------------------------------------

using System;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        public abstract class GenericDataProcessor<T> : DataProcessor
        {
            public override Type Type => typeof(T);

            public override bool IsId => false;

            public override bool IsComment => false;
            public override bool IsEnum => false;

            public abstract T Parse(string value);
        }
    }
}