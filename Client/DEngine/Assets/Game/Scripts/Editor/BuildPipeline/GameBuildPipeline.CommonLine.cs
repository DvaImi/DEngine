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
                BuildResource(GameSetting.Instance.BundlesOutput, false);
                Debug.Log("====================打包资源结束========================");

                Debug.Log("====================打包工程========================");
                BuildPlayer();
                Debug.Log("====================打包工程结束========================");
            }
        }
    }
}