﻿using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DecimalProcessor : GenericDataProcessor<decimal>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "decimal";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "decimal",
                    "system.decimal"
                };
            }

            public override decimal Parse(string value)
            {
                return decimal.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}