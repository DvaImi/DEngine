using System.IO;
using System.Linq;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class StringProcessor : GenericDataProcessor<string>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "string";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "string",
                    "system.string"
                };
            }

            public override string Parse(string value)
            {
                return m_EscapeStrings.Aggregate(value, (current, escapeString) => current.Replace(escapeString.Key, escapeString.Value));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}