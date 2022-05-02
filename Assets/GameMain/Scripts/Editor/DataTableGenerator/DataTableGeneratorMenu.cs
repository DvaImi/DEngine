//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEditor;
using UnityEngine;

namespace JuvenileGemini.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("JuvenileGemini/Generator/Generate Configs")]
        internal static void GenerateConfigs()
        {
            foreach (string dataTableName in ProcedurePreload.ConfigName)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateConfigProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }
        
        [MenuItem("JuvenileGemini/Generator/Generate DataTables")]
        internal static void GenerateDataTables()
        {
            foreach (string dataTableName in ProcedurePreload.DataTableNames)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }
        
        [MenuItem("JuvenileGemini/Generator/Generate DataTables Code")]
        internal static void GenerateDataTablesCode()
        {
            foreach (string dataTableName in ProcedurePreload.DataTableNames)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }
    }
}
