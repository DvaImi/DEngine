using System;
using Cysharp.Threading.Tasks;
using DEngine.Resource;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public static class HostingServiceManager
    {
        private static IHostingService s_HostingService;
        public static bool IsListening => s_HostingService?.IsListening ?? false;

        public static string HostingServicePath { get; } = DEngine.Utility.Text.Format("{0}/{1}", DEngineSetting.BundlesOutput, "HostingService");


        static HostingServiceManager()
        {
            EditorApplication.quitting -= OnEditorQuit;
            EditorApplication.quitting += OnEditorQuit;
        }

        [InitializeOnEnterPlayMode]
        public static void StartService()
        {
            if (!DEngineSetting.Instance.EnableHostingService || IsListening)
            {
                return;
            }
            
            if (DEngineSetting.Instance.ResourceMode < ResourceMode.Updatable)
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
        }

        public static async UniTask UploadFile(string filePath, string relativePath)
        {
            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            if (s_HostingService is not { IsListening: true })
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

        private static void OnEditorQuit()
        {
            StopService();
        }
    }
}