// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-05-30 20:25:44
// 版 本：1.0
// ========================================================
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public static class ResourceRuleEditorUtility
    {
        public  static void RefreshResourceCollection()
        {
            ResourceRuleEditor ruleEditor = ScriptableObject.CreateInstance<ResourceRuleEditor>();
            ruleEditor.RefreshResourceCollection();
        }
        
        public  static void RefreshResourceCollection(string configPath)
        {
            ResourceRuleEditor ruleEditor = ScriptableObject.CreateInstance<ResourceRuleEditor>();
            ruleEditor.RefreshResourceCollection(configPath);
        }
    }
}