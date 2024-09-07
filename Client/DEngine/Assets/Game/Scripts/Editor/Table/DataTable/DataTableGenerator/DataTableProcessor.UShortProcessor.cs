
using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class UShortProcessor : GenericDataProcessor<ushort>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "ushort";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "ushort",
                    "uint16",
                    "system.uint16"
                };
            }

            public override ushort Parse(string value)
            {
                return ushort.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}