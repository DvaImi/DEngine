using System.Collections.Generic;
using Game.Editor.DataTableTools;
using Game.Editor.Toolbar;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        [MenuItem("Game/Build Pipeline/Automated Build", false, 100)]
        [EditorToolMenu("AutomatedBuild", 1, 100)]
        private static void AutomatedBuild()
        {
            EditorTools.CloseAllCustomEditorWindows();
            Debug.Log("开始一键打包任务");
            CheckPlatform();
            {
                Debug.Log("====================目标平台切换成功========================");
                Debug.Log("====================生成游戏数据表========================");
                GeneratorDataTableCommonLine.GenerateAll();
                Debug.Log("====================数据表生成结束========================");
                Debug.Log("====================保存配置文件========================");
                Debug.Log("====================保存配置文件========================");
                SaveHybridCLR();
                SaveResource();
                SaveBuildInfo();
                SaveBuildSetting();
                GameSetting.Save();
                Debug.Log("====================保存配置文件结束========================");

                Debug.Log("====================编译代码========================");
                PrebuildCommand.GenerateAll();
                CompileHotfixDll();
                Debug.Log("====================编译代码结束========================");

                Debug.Log("====================打包资源========================");
                BuildResource();
                Debug.Log("====================打包资源结束========================");

                Debug.Log("====================打包工程========================");
                BuildPlayer();
                Debug.Log("====================打包工程结束========================");
            }
        }

        /// <summary>
        /// 多渠道自动打包
        /// </summary>
        private static void MultiChannelAutomatedBuild()
        {
            EditorTools.CloseAllCustomEditorWindows();
            Debug.Log("开始一键打包任务");

            Debug.Log("====================保存配置文件========================");
            SaveHybridCLR();
            SaveResource();
            SaveBuildInfo();
            SaveBuildSetting();
            GameSetting.Save();
            Debug.Log("====================保存配置文件结束========================");

            Debug.Log("====================编译代码========================");
            PrebuildCommand.GenerateAll();
            CompileHotfixDll();
            Debug.Log("====================编译代码结束========================");

            Debug.Log("====================打包资源========================");
            BuildResource();
            Debug.Log("====================打包资源结束========================");

            Debug.Log("====================读取渠道配置========================");
            List<DRPackingParameter> packingParametersa = GetPackingParameterset();

            if (packingParametersa != null)
            {
                foreach (DRPackingParameter parameter in packingParametersa)
                {
                    Debug.Log("====================应用渠道配置========================");
                    ApplyPackingParameter(parameter);
                    Debug.Log($"====================打包{parameter.AppName}工程========================");
                    BuildPlayerV2(parameter.ChannelName, parameter.ChannelPlatform);
                    Debug.Log($"====================打包{parameter.AppName}工程结束========================");
                }
            }
        }
    }
}