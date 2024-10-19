using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [InitializeOnLoad]
    public static class HostingServiceManager
    {
        private static IHostingService s_HostingService;
        public static bool IsListening => s_HostingService?.IsListening ?? false;

        public static string HostingServicePath { get; } = DEngine.Utility.Text.Format("{0}/{1}", DEngineSetting.BundlesOutput, "HostingService");

        static HostingServiceManager()
        {
            Init();
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuit;
        }

        public static void StartService()
        {
            if (!DEngineSetting.Instance.EnableHostingService || s_HostingService?.IsListening == true)
            {
                return;
            }

            try
            {
                var buildEventHandlerType = DEngine.Utility.Assembly.GetType(EditorPrefs.GetString("File Hosting Service Type Name"));
                if (buildEventHandlerType == null)
                {
                    Debug.LogError("Failed to get file hosting service type.");
                    return;
                }

                s_HostingService = (IHostingService)Activator.CreateInstance(buildEventHandlerType);
                if (s_HostingService == null)
                {
                    Debug.LogError("Failed to create hosting service instance.");
                    return;
                }

                int port = DEngineSetting.Instance.HostingServicePort;
                s_HostingService.StartService(port);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start hosting service: {ex.Message}");
            }
        }

        public static void StopService()
        {
            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            if (s_HostingService is { IsListening: true })
            {
                s_HostingService.StopService();
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.quitting -= OnEditorQuit;
        }

        public static async UniTask UploadFile(string filePath, string relativePath)
        {
            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            if (s_HostingService == null || !s_HostingService.IsListening)
            {
                return;
            }

            try
            {
                await s_HostingService.UploadFile(filePath, relativePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to upload file: {ex.Message}");
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    StopService();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    StartService();
                    break;
                default:
                    return;
            }
        }

        private static void OnEditorQuit()
        {
            StopService();
        }
    }
}