// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-05-14 23:47:04
// 版 本：1.0
// ========================================================

using System.Text;
using GameFramework;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GeminiLion.Editor
{
    public class AssetsTool : OdinEditor
    {
        private static StringBuilder m_RelatePath = new StringBuilder();

        [MenuItem("Assets/Copy Selects AssetsPath", isValidateFunction: false, priority: -1)]
        public static void GetAssetsPath()
        {
            Object[] objects = Selection.objects;

            if (objects == null)
            {
                throw new GameFrameworkException("No game object selected.");
            }

            for (int i = 0; i < objects.Length; i++)
            {
                m_RelatePath.Append(AssetDatabase.GetAssetPath(objects[i]));

                if (i < objects.Length - 1)
                {
                    m_RelatePath.Append("\n");
                }
            }

            Clipboard.Copy(m_RelatePath);
            Debug.Log(m_RelatePath);
            AssetDatabase.Refresh();
        }
    }
}