using System;
using System.IO;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ColorProcessor : GenericDataProcessor<Color>
        {
            public override bool IsSystem => false;

            public override string LanguageKeyword => "Color";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "color",
                    "unityengine.color"
                };
            }

            public override Color Parse(string value)
            {
                var splitedValue = value.Split(',') ?? throw new ArgumentNullException(nameof(value));
                return new Color(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]), float.Parse(splitedValue[3]));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                var color = Parse(value);
                binaryWriter.Write(color.r);
                binaryWriter.Write(color.g);
                binaryWriter.Write(color.b);
                binaryWriter.Write(color.a);
            }
        }
    }
}