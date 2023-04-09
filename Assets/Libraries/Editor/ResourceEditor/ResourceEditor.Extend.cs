// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-04 20:49:55
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor.ResourceTools
{
    internal sealed partial class ResourceEditor : EditorWindow
    {
        [MenuItem("Game Framework/Resource Tools/Resource Collection Extend", false, 45)]
        public static void ModifyLoadType()
        {
            ResourceEditorExtend window = GetWindow<ResourceEditorExtend>("Resources", true);
            window.minSize = new Vector2(800f, 300f);
        }
        private class ResourceEditorExtend : EditorWindow
        {
            private ResourceFolder m_ResourceRoot = null;
            private ResourceEditorController m_Controller = null;
            private HashSet<Resource> m_SelectedResources = null;
            private HashSet<string> m_ExpandedResourceFolderNames = null;
            private Vector2 m_ResourcesViewScroll = Vector2.zero;
            private Vector2 m_SourceAssetsViewScroll = Vector2.zero;
            private int m_CurrentResourceRowOnDraw = 0;
            private LoadType m_LoadType = LoadType.LoadFromFile;
            private bool m_Packed;
            private MenuState m_MenuState = MenuState.Normal;
            private string m_InputResourceGroups = null;
            private string m_InputResourceFileSystem = null;

            private void OnEnable()
            {
                m_SelectedResources = new HashSet<Resource>();
                m_ExpandedResourceFolderNames = new HashSet<string>();

                m_Controller = new ResourceEditorController();
                m_ResourceRoot = new ResourceFolder("Resources", null);
                m_Controller.OnLoadingResource += OnLoadingResource;
                m_Controller.OnLoadingAsset += OnLoadingAsset;
                m_Controller.OnLoadCompleted += OnLoadCompleted;

                if (m_Controller.Load())
                {
                    Debug.Log("Load configuration success.");
                }
                else
                {
                    Debug.LogWarning("Load configuration failure.");
                }

                EditorUtility.DisplayProgressBar("Prepare Resource Editor", "Processing...", 0f);
                RefreshResourceTree();
                EditorUtility.ClearProgressBar();
            }

            private void OnGUI()
            {
                GUILayout.Space(2f);
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width));
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(Utility.Text.Format("Resource List ({0})", m_Controller.ResourceCount), EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal("box", GUILayout.Height(position.height - 52f));
                    {
                        m_CurrentResourceRowOnDraw = 0;
                        m_ResourcesViewScroll = EditorGUILayout.BeginScrollView(m_ResourcesViewScroll);
                        {
                            DrawSourceAssetsView(m_ResourceRoot);
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5f);
                        DrawResourcesMenu();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            private void DrawResourcesMenu()
            {
                switch (m_MenuState)
                {
                    case MenuState.Normal:
                        DrawResourcesMenu_Normal();
                        break;
                    case MenuState.Groups:
                        DrawResourcesMenu_Groups();
                        break;
                    case MenuState.FileSystem:
                        DrawResourceMenu_FileSystem();
                        break;
                }

            }

            private void DrawSourceAssetsView(ResourceFolder folder)
            {
                m_SourceAssetsViewScroll = EditorGUILayout.BeginScrollView(m_SourceAssetsViewScroll);
                {
                    DrawResourceFolder(folder);
                }
                EditorGUILayout.EndScrollView();
            }

            private void DrawResourceFolder(ResourceFolder folder)
            {
                bool expand = IsExpandedResourceFolder(folder);
                EditorGUILayout.BeginHorizontal();
                {
#if UNITY_2019_3_OR_NEWER
                    bool foldout = EditorGUI.Foldout(new Rect(18f + 14f * folder.Depth, 20f * m_CurrentResourceRowOnDraw + 4f, int.MaxValue, 14f), expand, string.Empty, true);
#else
                bool foldout = EditorGUI.Foldout(new Rect(18f + 14f * folder.Depth, 20f * m_CurrentResourceRowOnDraw + 2f, int.MaxValue, 14f), expand, string.Empty, true);
#endif
                    if (expand != foldout)
                    {
                        expand = !expand;
                        SetExpandedResourceFolder(folder, expand);
                    }

#if UNITY_2019_3_OR_NEWER
                    GUI.DrawTexture(new Rect(32f + 14f * folder.Depth, 20f * m_CurrentResourceRowOnDraw + 3f, 16f, 16f), ResourceFolder.Icon);
                    EditorGUILayout.LabelField(string.Empty, GUILayout.Width(44f + 14f * folder.Depth), GUILayout.Height(18f));
#else
                GUI.DrawTexture(new Rect(32f + 14f * folder.Depth, 20f * m_CurrentResourceRowOnDraw + 1f, 16f, 16f), ResourceFolder.Icon);
                EditorGUILayout.LabelField(string.Empty, GUILayout.Width(40f + 14f * folder.Depth), GUILayout.Height(18f));
#endif
                    EditorGUILayout.LabelField(folder.Name);
                }
                EditorGUILayout.EndHorizontal();

                m_CurrentResourceRowOnDraw++;

                if (expand)
                {
                    foreach (ResourceFolder subFolder in folder.GetFolders())
                    {
                        DrawResourceFolder(subFolder);
                    }

                    foreach (ResourceItem resourceItem in folder.GetItems())
                    {
                        DrawResourceItem(resourceItem);
                    }
                }
            }

            private void DrawResourceItem(ResourceItem resourceItem)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    string title = resourceItem.Name;
                    if (resourceItem.Resource.Packed)
                    {
                        title = "[Packed] " + title;
                    }

                    float emptySpace = position.width;
                    bool select = IsSelectResource(resourceItem.Resource);
                    if (select != EditorGUILayout.Toggle(select, GUILayout.Width(emptySpace - 12f)))
                    {
                        select = !select;
                        SetSelectedResource(resourceItem.Resource, select);
                    }

                    GUILayout.Space(-emptySpace + 24f);
                    GUI.DrawTexture(new Rect(32f + 14f * resourceItem.Depth, 20f * m_CurrentResourceRowOnDraw + 3f, 16f, 16f), resourceItem.Icon);
                    EditorGUILayout.LabelField(string.Empty, GUILayout.Width(30f + 14f * resourceItem.Depth), GUILayout.Height(18f));

                    string[] groups = resourceItem.Resource.GetResourceGroups();
                    string rsourcesGroups = groups.Length <= 0 ? null : " ResourceGroups [" + string.Join(",", groups) + "]";
                    string fileSystem = resourceItem.Resource.FileSystem == null ? null : " FileSystem =>" + resourceItem.Resource.FileSystem;
                    EditorGUILayout.LabelField(title + rsourcesGroups + fileSystem);
                }
                EditorGUILayout.EndHorizontal();
                m_CurrentResourceRowOnDraw++;
            }

            private void DrawResourcesMenu_Normal()
            {
                EditorGUI.BeginDisabledGroup(m_SelectedResources.Count <= 0);
                {
                    m_LoadType = (LoadType)EditorGUILayout.EnumPopup(m_LoadType);
                    m_Packed = EditorGUILayout.ToggleLeft("Packed", m_Packed, GUILayout.Width(65f));
                    m_MenuState = MenuState.Normal;

                    if (GUILayout.Button("Groups", GUILayout.Width(65f)))
                    {
                        m_MenuState = MenuState.Groups;
                    }
                    if (GUILayout.Button("FileSystem", GUILayout.Width(85f)))
                    {
                        m_MenuState = MenuState.FileSystem;
                    }

                    if (GUILayout.Button("OK", GUILayout.Width(65f)))
                    {
                        SetResourcePacked(m_Packed);
                        SetResourceLoadType(m_LoadType);
                        SaveConfiguration();
                    }

                    if (GUILayout.Button("Back", GUILayout.Width(65f)))
                    {
                        m_MenuState = MenuState.Normal;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            private void DrawResourcesMenu_Groups()
            {
                if (m_SelectedResources.Count <= 0)
                {
                    m_MenuState = MenuState.Normal;
                    return;
                }

                m_InputResourceGroups = EditorGUILayout.TextField(m_InputResourceGroups);

                if (GUILayout.Button("OK", GUILayout.Width(50f)))
                {
                    foreach (var selectedResource in m_SelectedResources)
                    {
                        EditorUtility.DisplayProgressBar(Utility.Text.Format("Set Resource Groups:[{0}]", m_InputResourceGroups), "Processing...", 0f);
                        SetResourceGroups(selectedResource, m_InputResourceGroups);
                        EditorUtility.ClearProgressBar();
                    }
                    SaveConfiguration();
                    m_MenuState = MenuState.Normal;
                }

                if (GUILayout.Button("Back", GUILayout.Width(50f)))
                {
                    m_MenuState = MenuState.Normal;
                }
            }

            private void DrawResourceMenu_FileSystem()
            {
                if (m_SelectedResources.Count <= 0)
                {
                    m_MenuState = MenuState.Normal;
                    return;
                }

                m_InputResourceFileSystem = EditorGUILayout.TextField(m_InputResourceFileSystem);

                if (GUILayout.Button("OK", GUILayout.Width(50f)))
                {
                    foreach (var selectedResource in m_SelectedResources)
                    {
                        EditorUtility.DisplayProgressBar(Utility.Text.Format("Set Resource FileSystem:[{0}]", m_InputResourceFileSystem), "Processing...", 0f);
                        SetResourceFileSystem(selectedResource, m_InputResourceFileSystem);
                        EditorUtility.ClearProgressBar();
                    }
                    SaveConfiguration();
                    m_MenuState = MenuState.Normal;
                }

                if (GUILayout.Button("Back", GUILayout.Width(50f)))
                {
                    m_MenuState = MenuState.Normal;
                }
            }

            private void OnLoadingResource(int index, int count)
            {
                EditorUtility.DisplayProgressBar("Loading Resources", Utility.Text.Format("Loading resources, {0}/{1} loaded.", index, count), (float)index / count);
            }

            private void OnLoadingAsset(int index, int count)
            {
                EditorUtility.DisplayProgressBar("Loading Assets", Utility.Text.Format("Loading assets, {0}/{1} loaded.", index, count), (float)index / count);
            }

            private void OnLoadCompleted()
            {
                EditorUtility.ClearProgressBar();
            }

            private void RefreshResourceTree()
            {
                m_ResourceRoot.Clear();
                Resource[] resources = m_Controller.GetResources();
                foreach (Resource resource in resources)
                {
                    string[] splitedPath = resource.Name.Split('/');
                    ResourceFolder folder = m_ResourceRoot;
                    for (int i = 0; i < splitedPath.Length - 1; i++)
                    {
                        ResourceFolder subFolder = folder.GetFolder(splitedPath[i]);
                        folder = subFolder == null ? folder.AddFolder(splitedPath[i]) : subFolder;
                    }

                    string fullName = resource.Variant != null ? Utility.Text.Format("{0}.{1}", splitedPath[splitedPath.Length - 1], resource.Variant) : splitedPath[splitedPath.Length - 1];
                    folder.AddItem(fullName, resource);
                }
            }

            private bool IsExpandedResourceFolder(ResourceFolder folder)
            {
                return m_ExpandedResourceFolderNames.Contains(folder.FromRootPath);
            }

            private void SetExpandedResourceFolder(ResourceFolder folder, bool expand)
            {
                if (expand)
                {
                    m_ExpandedResourceFolderNames.Add(folder.FromRootPath);
                }
                else
                {
                    m_ExpandedResourceFolderNames.Remove(folder.FromRootPath);
                }
            }

            private bool IsSelectResource(Resource resource)
            {
                return m_SelectedResources.Contains(resource);
            }

            private void SetSelectedResource(Resource resource, bool select)
            {
                if (select)
                {
                    m_SelectedResources.Add(resource);
                }
                else
                {
                    m_SelectedResources.Remove(resource);
                }
            }

            private void SetResourceLoadType(LoadType loadType)
            {
                if (m_SelectedResources.Count <= 0)
                {
                    return;
                }

                foreach (var resource in m_SelectedResources)
                {
                    string fullName = resource.FullName;
                    if (m_Controller.SetResourceLoadType(resource.Name, resource.Variant, loadType))
                    {
                        Debug.Log(Utility.Text.Format("Set resource '{0}' load type to '{1}' success.", fullName, loadType));
                    }
                    else
                    {
                        Debug.LogWarning(Utility.Text.Format("Set resource '{0}' load type to '{1}' failure.", fullName, loadType));
                    }
                }
            }

            private void SetResourcePacked(bool packed)
            {
                if (m_SelectedResources.Count <= 0)
                {
                    return;
                }

                foreach (var resource in m_SelectedResources)
                {
                    string fullName = resource.FullName;
                    if (m_Controller.SetResourcePacked(resource.Name, resource.Variant, packed))
                    {
                        Debug.Log(Utility.Text.Format("{1} resource '{0}' success.", fullName, packed ? "Pack" : "Unpack"));
                    }
                    else
                    {
                        Debug.LogWarning(Utility.Text.Format("{1} resource '{0}' failure.", fullName, packed ? "Pack" : "Unpack"));
                    }
                }
            }

            private void SetResourceGroups(Resource resource, string resourceGroups)
            {
                if (resource == null)
                {
                    Debug.LogWarning("Resource is invalid.");
                    return;
                }

                if (m_Controller.SetResourceGroups(resource.Name, resource.Variant, resourceGroups))
                {
                    RefreshResourceTree();
                    Debug.Log(Utility.Text.Format("SetResource '{0}' Groups '{1}' success.", resource.Name, resourceGroups));
                }
                else
                {
                    Debug.LogWarning(Utility.Text.Format("SetResource '{0}' Groups '{1}' failure.", resource.Name, resourceGroups));
                }
            }

            private void SetResourceFileSystem(Resource resource, string fileSystem)
            {
                if (resource == null)
                {
                    Debug.LogWarning("Resource is invalid.");
                    return;
                }

                if (m_Controller.SetResourceFileSystem(resource.Name, resource.Variant, fileSystem))
                {
                    RefreshResourceTree();
                    Debug.Log(Utility.Text.Format("SetResource '{0}' fileSystem '{1}' success.", resource.Name, fileSystem));
                }
                else
                {
                    Debug.LogWarning(Utility.Text.Format("SetResource '{0}' fileSystem '{1}' failure.", resource.Name, fileSystem));
                }
            }

            private void SaveConfiguration()
            {
                if (m_Controller.Save())
                {
                    Debug.Log("Save configuration success.");
                }
                else
                {
                    Debug.LogWarning("Save configuration failure.");
                }
            }
        }
    }
}
