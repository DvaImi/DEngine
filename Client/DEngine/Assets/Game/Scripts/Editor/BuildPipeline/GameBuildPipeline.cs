using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static string[] PackagesNames { get; private set; }
        public static string[] VariantNames { get; private set; }

        static GameBuildPipeline()
        {
            RefreshPackages();
        }

        /// <summary>
        /// 检查 Unity 编辑器当前是否处于忙碌状态
        /// </summary>
        public static bool IsEditorBusy()
        {
            return EditorApplication.isCompiling ||                   // 检查脚本是否正在编译
                   Progress.GetCount() > 0 ||                         // 检查是否有未完成的进度任务
                   Lightmapping.isRunning ||                          // 检查光照贴图是否正在生成
                   AssetDatabase.IsAssetImportWorkerProcess() ||      // 检查资源导入进程是否正在运行
                   EditorApplication.isUpdating ||                    // 检查编辑器是否正在执行更新操作
                   EditorApplication.isPlayingOrWillChangePlaymode || // 检查编辑器是否正在切换至运行模式或退出运行模式
                   ShaderUtil.anythingCompiling;                      // 检查是否有未完成的Shader编译任务
        }
    }
}