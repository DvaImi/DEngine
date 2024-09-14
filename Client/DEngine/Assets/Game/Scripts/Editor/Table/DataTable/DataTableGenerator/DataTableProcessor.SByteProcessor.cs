using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class SByteProcessor : GenericDataProcessor<sbyte>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "sbyte";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "sbyte",
                    "system.sbyte"
                };
            }

            public override sbyte Parse(string value)
            {
                return sbyte.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}