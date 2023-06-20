﻿using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ShortProcessor : GenericDataProcessor<short>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "short";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "short",
                    "int16",
                    "system.int16"
                };
            }

            public override short Parse(string value)
            {
                return short.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}