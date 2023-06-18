﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://
// .cn/
// Feedback: mailto:ellan@DEngine.cn
//------------------------------------------------------------

using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ByteProcessor : GenericDataProcessor<byte>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "byte";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "byte",
                    "system.byte"
                };
            }

            public override byte Parse(string value)
            {
                return byte.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}