using UnityEngine;

namespace Game
{
    [CreateAssetMenu()]
    public class BuiltinData : ScriptableObject
    {
        /// <summary>
        /// 内置资源更新界面
        /// </summary>
        public UpdateResourceForm UpdateResourceFormTemplate = null;
        /// <summary>
        /// 内置原生对话框
        /// </summary>
        public NativeDialogForm NativeDialogFormTemplate = null;
        /// <summary>
        /// 构建信息
        /// </summary>
        public BuildInfo BuildInfo;
    }
}
