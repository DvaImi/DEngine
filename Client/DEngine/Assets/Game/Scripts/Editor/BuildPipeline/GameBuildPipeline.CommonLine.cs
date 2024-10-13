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
        [EditorToolbarMenu("一键打包", 1, 100)]
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
                DEngineSetting.Save();
                Debug.Log("====================保存配置文件结束========================");

#if ENABLE_HYBRIDCLR
                Debug.Log("====================编译代码========================");
                PrebuildCommand.GenerateAll();
                CopyDllAssets();
                Debug.Log("====================编译代码结束========================");
#endif

                Debug.Log("====================处理文件系统========================");
                ProcessFileSystem();
                Debug.Log("====================处理文件系统结束========================");

                Debug.Log("====================打包资源========================");
                BuildResource();
                Debug.Log("====================打包资源结束========================");

                Debug.Log("====================打包工程========================");
                BuildPlayer();
                Debug.Log("====================打包工程结束========================");
            }
        }
    }
}