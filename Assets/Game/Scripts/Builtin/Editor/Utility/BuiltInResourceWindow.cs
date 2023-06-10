using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class BuiltInResourceWindow : EditorWindow
    {
        //[MenuItem("Game/BuiltInResourceWindow")]
        static void Init()
        {
            EditorWindow window = EditorWindow.CreateInstance<BuiltInResourceWindow>();
            window.Show();
        }

        private List<Texture2D> builtInTexs = new List<Texture2D>();
        private void GetBultinAsset()
        {
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
            var bundle = info.Invoke(null, new object[0]) as AssetBundle;
            Object[] objs = bundle.LoadAllAssets();
            if (null != objs)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] is Texture2D)
                    {
                        builtInTexs.Add(objs[i] as Texture2D);
                    }
                }
            }
            builtInTexs.Sort((x, y) => x.name.CompareTo(y.name));
        }

        private void OnEnable()
        {
            GetBultinAsset();
        }

        Vector2 scrollPos = Vector2.zero;
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < builtInTexs.Count; i++)
            {
                EditorGUILayout.ObjectField(builtInTexs[i], typeof(Texture2D), false);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
