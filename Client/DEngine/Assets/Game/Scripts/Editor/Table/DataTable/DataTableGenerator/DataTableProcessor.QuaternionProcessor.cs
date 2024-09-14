﻿using System.IO;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class QuaternionProcessor : GenericDataProcessor<Quaternion>
        {
            public override bool IsSystem => false;

            public override string LanguageKeyword => "Quaternion";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "quaternion",
                    "unityengine.quaternion"
                };
            }

            public override Quaternion Parse(string value)
            {
                var splitedValue = value.Split(',');
                return new Quaternion(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]), float.Parse(splitedValue[3]));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                var quaternion = Parse(value);
                binaryWriter.Write(quaternion.x);
                binaryWriter.Write(quaternion.y);
                binaryWriter.Write(quaternion.z);
                binaryWriter.Write(quaternion.w);
            }
        }
    }
}