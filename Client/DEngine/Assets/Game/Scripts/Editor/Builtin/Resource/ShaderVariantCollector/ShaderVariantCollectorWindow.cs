#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Editor;
using Game.Editor.BuildPipeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ShaderVariantCollectorWindow : EditorWindow
{
    [MenuItem("Game/Resource Tools/ShaderVariantCollector", false, 1)]
    public static void OpenWindow()
    {
        ShaderVariantCollectorWindow window = GetWindow<ShaderVariantCollectorWindow>("着色器变种收集工具", true);
        window.minSize = new Vector2(800, 600);
    }

    private Button m_CollectButton;
    private TextField m_CollectOutputField;
    private Label m_CurrentShaderCountField;
    private Label m_CurrentVariantCountField;
    private SliderInt m_ProcessCapacitySlider;
    private PopupField<string> m_PackageField;

    private List<string> m_PackageNames;
    private string m_CurrentPackageName;

    public void CreateGUI()
    {
        try
        {
            VisualElement root = this.rootVisualElement;

            // 加载布局文件
            var visualAsset = UxmlLoader.LoadWindowUxml<ShaderVariantCollectorWindow>();
            if (visualAsset == null)
                return;

            visualAsset.CloneTree(root);

            // 包裹名称列表
            m_PackageNames = GameBuildPipeline.PackagesNames.ToList();
            m_CurrentPackageName = m_PackageNames[0];

            // 文件输出目录
            m_CollectOutputField = root.Q<TextField>("CollectOutput");
            m_CollectOutputField.SetValueWithoutNotify(ShaderVariantCollectorSetting.GeFileSavePath(m_CurrentPackageName));
            m_CollectOutputField.RegisterValueChangedCallback(_ => { ShaderVariantCollectorSetting.SetFileSavePath(m_CurrentPackageName, m_CollectOutputField.value); });

            // 收集的包裹
            var packageContainer = root.Q("PackageContainer");
            if (m_PackageNames.Count > 0)
            {
                int defaultIndex = GetDefaultPackageIndex(m_CurrentPackageName);
                m_PackageField = new PopupField<string>(m_PackageNames, defaultIndex)
                {
                    label = "Package",
                    style =
                    {
                        width = 350
                    }
                };
                m_PackageField.RegisterValueChangedCallback(_ => { m_CurrentPackageName = m_PackageField.value; });
            }
            else
            {
                m_PackageField = new PopupField<string>
                {
                    label = "Package",
                    style =
                    {
                        width = 350
                    }
                };
            }

            packageContainer.Add(m_PackageField);

            // 容器值
            m_ProcessCapacitySlider = root.Q<SliderInt>("ProcessCapacity");
            m_ProcessCapacitySlider.SetValueWithoutNotify(ShaderVariantCollectorSetting.GeProcessCapacity(m_CurrentPackageName));
#if !UNITY_2020_3_OR_NEWER
            _processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
            _processCapacitySlider.RegisterValueChangedCallback(evt =>
            {
                ShaderVariantCollectorSetting.SetProcessCapacity(_currentPackageName, _processCapacitySlider.value);
                _processCapacitySlider.label = $"Capacity ({_processCapacitySlider.value})";
            });
#else
            m_ProcessCapacitySlider.RegisterValueChangedCallback(_ => { ShaderVariantCollectorSetting.SetProcessCapacity(m_CurrentPackageName, m_ProcessCapacitySlider.value); });
#endif

            m_CurrentShaderCountField = root.Q<Label>("CurrentShaderCount");
            m_CurrentVariantCountField = root.Q<Label>("CurrentVariantCount");

            // 变种收集按钮
            m_CollectButton = root.Q<Button>("CollectButton");
            m_CollectButton.clicked += CollectButton_clicked;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void Update()
    {
        if (m_CurrentShaderCountField != null)
        {
            int currentShaderCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionShaderCount();
            m_CurrentShaderCountField.text = $"Current Shader Count : {currentShaderCount}";
        }

        if (m_CurrentVariantCountField != null)
        {
            int currentVariantCount = ShaderVariantCollectionHelper.GetCurrentShaderVariantCollectionVariantCount();
            m_CurrentVariantCountField.text = $"Current Variant Count : {currentVariantCount}";
        }
    }

    private void CollectButton_clicked()
    {
        string savePath = ShaderVariantCollectorSetting.GeFileSavePath(m_CurrentPackageName);
        int processCapacity = m_ProcessCapacitySlider.value;
        ShaderVariantCollector.Run(savePath, m_CurrentPackageName, processCapacity, null);
    }

    private int GetDefaultPackageIndex(string packageName)
    {
        for (int index = 0; index < m_PackageNames.Count; index++)
        {
            if (m_PackageNames[index] == packageName)
            {
                return index;
            }
        }

        return 0;
    }
}
#endif