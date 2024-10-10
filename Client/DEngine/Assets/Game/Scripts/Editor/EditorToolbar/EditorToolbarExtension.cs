using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Editor.Toolbar
{
    [InitializeOnLoad]
    public static class EditorToolbarExtension
    {
        private static ScriptableObject s_CurrentToolbar;
        private static readonly Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly List<EditorToolbarMenuAttribute> LeftMenu = new();
        private static readonly List<EditorToolbarMenuAttribute> RightMenu = new();
        private static readonly Dictionary<string, MethodInfo> LeftCachedMethods = new();
        private static readonly Dictionary<string, MethodInfo> RightCachedMethods = new();
        private static readonly Dictionary<string, MethodInfo> LeftToolbarCustomGUI = new();
        private static readonly Dictionary<string, MethodInfo> RightToolbarCustomGUI = new();

        static EditorToolbarExtension()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            CacheMethods();
        }

        private static void CacheMethods()
        {
            LeftMenu.Clear();
            RightMenu.Clear();
            LeftCachedMethods.Clear();
            RightCachedMethods.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    foreach (MethodInfo method in methods)
                    {
                        var attribute = method.GetCustomAttribute<EditorToolbarMenuAttribute>();
                        if (attribute != null)
                        {
                            if (attribute.Align == 0)
                            {
                                if (attribute.UseCustomGUI)
                                {
                                    LeftToolbarCustomGUI[attribute.MenuName!] = method;
                                }
                                else
                                {
                                    LeftCachedMethods[attribute.MenuName!] = method;
                                }

                                LeftMenu.Add(attribute);
                            }
                            else
                            {
                                if (attribute.UseCustomGUI)
                                {
                                    RightToolbarCustomGUI[attribute.MenuName!] = method;
                                }
                                else
                                {
                                    RightCachedMethods[attribute.MenuName!] = method;
                                }

                                RightMenu.Add(attribute);
                            }
                        }
                    }
                }
            }

            CheckDuplicateOrders(LeftMenu, LeftCachedMethods);
            CheckDuplicateOrders(RightMenu, RightCachedMethods);
            LeftMenu.Sort(Compare);
            RightMenu.Sort(Compare);
        }

        private static void CheckDuplicateOrders(List<EditorToolbarMenuAttribute> menuAttributes, Dictionary<string, MethodInfo> methodInfos)
        {
            Dictionary<int, List<string>> orderDictionary = new();

            foreach (var menu in menuAttributes)
            {
                if (orderDictionary.TryGetValue(menu.Order, out var methodNames))
                {
                    methodNames.Add(menu.MenuName);
                }
                else
                {
                    orderDictionary[menu.Order] = new List<string> { menu.MenuName };
                }
            }

            foreach (var kvp in orderDictionary)
            {
                if (kvp.Value.Count > 1)
                {
                    List<string> methodPaths = new();
                    foreach (var (menuName, method) in methodInfos)
                    {
                        if (method.DeclaringType != null)
                        {
                            var fullPath = $"{method.DeclaringType.FullName}.{method.Name}";
                            methodPaths.Add($"{menuName} -> {fullPath}() ");
                        }
                    }

                    Debug.LogWarning($"Order '{kvp.Key}' is duplicated by the following methods: \n{string.Join("\n", methodPaths)}");
                }
            }
        }

        private static int Compare(EditorToolbarMenuAttribute x, EditorToolbarMenuAttribute y)
        {
            return x.Order.CompareTo(y.Order);
        }

        private static void OnUpdate()
        {
            if (!s_CurrentToolbar)
            {
                Object[] toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
                s_CurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (s_CurrentToolbar != null)
                {
                    FieldInfo root = s_CurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (root != null)
                    {
                        VisualElement concreteRoot = root.GetValue(s_CurrentToolbar) as VisualElement;


                        VisualElement leftParent = new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };

                        VisualElement toolbarLeftZone = concreteRoot.Q("ToolbarZoneLeftAlign");
                        IMGUIContainer leftContainer = new IMGUIContainer();
                        leftContainer.onGUIHandler += OnGUILeftHandler;
                        leftParent.Add(leftContainer);
                        toolbarLeftZone.Add(leftParent);

                        VisualElement rightParent = new VisualElement()
                        {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };

                        VisualElement toolbarRightZone = concreteRoot.Q("ToolbarZoneRightAlign");
                        IMGUIContainer rightContainer = new IMGUIContainer();
                        rightContainer.onGUIHandler += OnGUIRightHandler;
                        rightParent.Add(rightContainer);
                        toolbarRightZone.Add(rightParent);
                    }
                }
            }
        }

        private static void OnGUILeftHandler()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(15F);
                for (var i = 0; i < LeftMenu.Count; i++)
                {
                    var menu = LeftMenu[i];
                    if (menu.UseCustomGUI)
                    {
                        CallCustomGUIMethod(0, menu.MenuName);
                    }
                    else
                    {
                        if (GUILayout.Button(menu.MenuName, EditorStyles.toolbarButton))
                        {
                            CallMethod(0, menu.MenuName);
                        }
                    }

                    GUILayout.Space(5F);
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void OnGUIRightHandler()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(15F);
                for (var i = 0; i < RightMenu.Count; i++)
                {
                    var menu = RightMenu[i];
                    if (menu.UseCustomGUI)
                    {
                        CallCustomGUIMethod(1, menu.MenuName);
                    }
                    else
                    {
                        if (GUILayout.Button(menu.MenuName, EditorStyles.toolbarButton))
                        {
                            CallMethod(1, menu.MenuName);
                        }
                    }

                    GUILayout.Space(5F);
                }

                GUILayout.Space(10F);
            }
            GUILayout.EndHorizontal();
        }

        private static void CallMethod(int align, string menuName)
        {
            if (align == 0)
            {
                if (LeftCachedMethods.TryGetValue(menuName, out var methodInfo))
                {
                    try
                    {
                        methodInfo.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError($"Error calling method: {menuName}. Exception: {e.Message}");
                    }

                    return;
                }
            }
            else
            {
                if (RightCachedMethods.TryGetValue(menuName, out var methodInfo))
                {
                    try
                    {
                        methodInfo.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError($"Error calling method: {menuName}. Exception: {e.Message}");
                    }

                    return;
                }
            }


            Debug.LogWarning($"Method not found for menu name: {menuName}");
        }

        private static void CallCustomGUIMethod(int align, string menuName)
        {
            if (align == 0)
            {
                if (LeftToolbarCustomGUI.TryGetValue(menuName, out var methodInfo))
                {
                    try
                    {
                        methodInfo.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            else
            {
                if (RightToolbarCustomGUI.TryGetValue(menuName, out var methodInfo))
                {
                    try
                    {
                        methodInfo.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}