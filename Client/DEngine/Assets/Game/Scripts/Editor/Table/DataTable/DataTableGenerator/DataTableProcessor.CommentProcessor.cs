using System;
using System.IO;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class CommentProcessor : DataProcessor
        {
            public override Type Type => null;

            public override bool IsId => false;
            public override bool IsEnum => false;

            public override bool IsComment => true;

            public override bool IsSystem => false;

            public override string LanguageKeyword => null;

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    string.Empty,
                    "#",
                    "comment"
                };
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
            }
        }
    }
}