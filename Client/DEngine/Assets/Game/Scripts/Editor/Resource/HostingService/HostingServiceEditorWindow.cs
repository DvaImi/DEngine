using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class HostingServiceEditorWindow : EditorWindow
    {
        private HttpHostingService hostingService;
        private int port = 8080; // 默认端口
        private bool isServiceRunning;
        private const string ServiceRunningKey = "HostingService_IsRunning"; // 存储服务状态的Key

        [MenuItem("Tools/Hosting Service")]
        public static void ShowWindow()
        {
            GetWindow<HostingServiceEditorWindow>("Hosting Service");
        }

        private void OnEnable()
        {
            hostingService = new HttpHostingService();
            isServiceRunning = EditorPrefs.GetBool(ServiceRunningKey, false); // 从EditorPrefs读取状态

            if (isServiceRunning)
            {
                StartService(); // 确保窗口启动时，服务根据状态自动恢复
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("File Hosting Service", EditorStyles.boldLabel);

            // 配置端口
            port = EditorGUILayout.IntField("Port:", port);

            GUILayout.Space(10);

            // 显示服务状态并提供操作按钮
            if (!isServiceRunning)
            {
                if (GUILayout.Button("Start Service"))
                {
                    StartService();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Service"))
                {
                    StopService();
                }
            }

            GUILayout.Space(10);

            // 状态显示
            if (isServiceRunning)
            {
                EditorGUILayout.HelpBox($"Service running on port {port}.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Service is stopped.", MessageType.Warning);
            }
        }

        private void StartService()
        {
            if (!isServiceRunning)
            {
                hostingService.StartService(port);
                isServiceRunning = true;
                EditorPrefs.SetBool(ServiceRunningKey, true); // 保存服务运行状态
            }
        }

        private void StopService()
        {
            if (isServiceRunning)
            {
                hostingService.StopService();
                isServiceRunning = false;
                EditorPrefs.SetBool(ServiceRunningKey, false); // 保存服务停止状态
            }
        }

        private void OnDisable()
        {
            // 不在OnDisable中停止服务，保持服务状态持久化
            EditorPrefs.SetBool(ServiceRunningKey, isServiceRunning); // 确保状态被持久保存
        }
    }
}