using UnityEditor;
using UnityEngine;
using System.IO;

namespace Game.Editor.Package
{
    public class PackageCreatorEditor : EditorWindow
    {
        private const string CustomPackage = "Assets/CustomPackage/"; // 默认的包路径
        private string packageName = "";
        private PackageInfo packageInfo = new PackageInfo(); // 包信息

        [MenuItem("Package Tools/Create Custom Package")]
        public static void ShowWindow()
        {
            GetWindow<PackageCreatorEditor>("Create Custom Package");
        }

        // 绘制编辑器窗口的UI
        private void OnGUI()
        {
            GUILayout.Label("Create or Edit Unity Package", EditorStyles.boldLabel);

            // 输入包路径
            packageName = EditorGUILayout.TextField("Name", packageName);

            GUI.enabled = false;
            packageInfo.name = EditorGUILayout.TextField("Package Name", $"com.dvalmi.{packageName.ToLowerInvariant()}");
            GUI.enabled = true;
            packageInfo.version = EditorGUILayout.TextField("Version", packageInfo.version);
            packageInfo.displayName = EditorGUILayout.TextField("Display Name", packageInfo.displayName);
            packageInfo.description = EditorGUILayout.TextField("Description", packageInfo.description);
            packageInfo.unity = EditorGUILayout.TextField("Unity Version", packageInfo.unity);
            packageInfo.authorName = EditorGUILayout.TextField("Author Name", packageInfo.authorName);

            // 按钮生成目录和package.json
            if (GUILayout.Button("Create or Update Package"))
            {
                CreatePackageStructure();
            }

            // 按钮读取package.json
            if (GUILayout.Button("Load Existing package.json"))
            {
                LoadPackageJson();
            }
        }

        // 创建或更新Unity Package结构
        private void CreatePackageStructure()
        {
            string packageFullPath = Path.Combine(CustomPackage, packageName);
            if (!Directory.Exists(packageFullPath))
            {
                // 创建必要的目录结构
                Directory.CreateDirectory(packageFullPath);
                Directory.CreateDirectory(Path.Combine(packageFullPath, "Runtime"));
                Directory.CreateDirectory(Path.Combine(packageFullPath, "Editor"));
                Directory.CreateDirectory(Path.Combine(packageFullPath, "Tests"));
            }

            // 写入package.json文件
            string packageJsonPath = Path.Combine(packageFullPath, "package.json");
            string packageJsonContent = GeneratePackageJson();
            File.WriteAllText(packageJsonPath, packageJsonContent);


            // 写入README.md文件
            string readmePath = Path.Combine(packageFullPath, "README.md");
            if (!File.Exists(readmePath)) // 如果文件不存在，才创建它
            {
                string readmeContent = GenerateReadme();
                File.WriteAllText(readmePath, readmeContent);
            }

            // 写入CHANGELOG.md文件
            string changelogPath = Path.Combine(packageFullPath, "CHANGELOG.md");
            if (!File.Exists(changelogPath)) // 如果文件不存在，才创建它
            {
                string changelogContent = GenerateChangelog();
                File.WriteAllText(changelogPath, changelogContent);
            }


            // 刷新资产数据库
            AssetDatabase.Refresh();

            Debug.Log($"Package '{packageInfo.name}' created/updated successfully at {packageFullPath}");
        }

        // 生成package.json文件内容
        private string GeneratePackageJson()
        {
            return $@"
{{
    ""name"": ""{packageInfo.name.ToLower()}"",
    ""version"": ""{packageInfo.version}"",
    ""displayName"": ""{packageInfo.displayName}"",
    ""description"": ""{packageInfo.description}"",
    ""unity"": ""{packageInfo.unity}"",
    ""author"": {{
        ""name"": ""{packageInfo.authorName}""
    }}
}}
";
        }

        // 读取现有的package.json文件
        private void LoadPackageJson()
        {
            string packageJsonPath = Path.Combine(CustomPackage, packageName, "package.json");

            if (File.Exists(packageJsonPath))
            {
                string jsonContent = File.ReadAllText(packageJsonPath);
                packageInfo = JsonUtility.FromJson<PackageInfo>(jsonContent);
                Debug.Log("Loaded package.json successfully");
            }
            else
            {
                Debug.LogError("package.json not found in the specified path.");
            }
        }

        // 生成README.md文件内容
        private string GenerateReadme()
        {
            return $@"# {packageInfo.displayName}
{packageInfo.description}
## Installation
1. Open your Unity project.
2. Open the Package Manager (Window -> Package Manager).
3. Add the package by providing the following path:

## Usage

Describe how to use this package.

## Author
- **Name**: {{packageInfo.authorName}}
";
        }


        private string GenerateChangelog()
        {
            return $"# ChangelogAll notable changes to this package will be documented in this file.# [1.0.0] - Initial release-Added initial features for {packageInfo.displayName}";
        }
    }
}