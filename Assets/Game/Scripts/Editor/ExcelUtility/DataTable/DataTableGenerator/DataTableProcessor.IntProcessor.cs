﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://DEngine.cn/
// Feedback: mailto:ellan@DEngine.cn
//------------------------------------------------------------

using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class IntProcessor : GenericDataProcessor<int>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "int";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "int",
                    "int32",
                    "system.int32"
                };
            }

            public override int Parse(string value)
            {
                return int.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write7BitEncodedInt32(Parse(value));
            }
        }
    }
}