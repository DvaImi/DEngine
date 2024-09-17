
using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ULongProcessor : GenericDataProcessor<ulong>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "ulong";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "ulong",
                    "uint64",
                    "system.uint64"
                };
            }

            public override ulong Parse(string value)
            {
                return ulong.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write7BitEncodedUInt64(Parse(value));
            }
        }
    }
}