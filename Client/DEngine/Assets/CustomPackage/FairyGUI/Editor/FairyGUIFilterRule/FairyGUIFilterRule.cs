using System.IO;
using Game.Editor.ResourceTools;

namespace Game.Editor.FairyGUI
{
    public class FairyGUIFilterRule
    {
        /// <summary>
        /// 收集FairyGUI 描述文件规则
        /// </summary>
        public class CollectFairyGUIDesc : IFilterRule
        {
            public bool IsCollectAsset(string assetPath)
            {
                return Path.GetExtension(assetPath) == ".bytes";
            }
        }

        /// <summary>
        /// 收集FairyGUI 依赖资源规则
        /// </summary>
        public class CollectFairyGUIRes : IFilterRule
        {
            public bool IsCollectAsset(string assetPath)
            {
                return !DefaultFilterRule.IsIgnoreFile(Path.GetExtension(assetPath)) && Path.GetExtension(assetPath) != ".bytes";
            }
        }
    }
}