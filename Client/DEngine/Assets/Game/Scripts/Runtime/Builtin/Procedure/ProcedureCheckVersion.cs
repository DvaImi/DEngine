﻿using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool m_CheckVersionComplete = false;
        private bool m_NeedUpdateVersion = false;
        private VersionInfo m_VersionInfo = null;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            //检测连网状态
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("The device is not connected to the network");
                GameEntry.BuiltinData.OpenDialog(new DialogParams
                {
                    Mode = 1,
                    Message = "The device is not connected to the network",
                    ConfirmText = "Quit",
                    OnClickConfirm = delegate(object userData) { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                });
                return;
            }

            CheckVersionList();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckVersionComplete)
            {
                return;
            }

            if (m_NeedUpdateVersion)
            {
                procedureOwner.SetData<VarInt32>("VersionListLength", m_VersionInfo.VersionListLength);
                procedureOwner.SetData<VarInt32>("VersionListHashCode", m_VersionInfo.VersionListHashCode);
                procedureOwner.SetData<VarInt32>("VersionListCompressedLength", m_VersionInfo.VersionListCompressedLength);
                procedureOwner.SetData<VarInt32>("VersionListCompressedHashCode", m_VersionInfo.VersionListCompressedHashCode);
                ChangeState<ProcedureUpdateVersionList>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }

        private async void CheckVersionList()
        {
            string checkVersionUrl = GameEntry.BuiltinData.Builtin.BuildInfo.CheckVersionUrl;
            // string checkVersionUrl = Utility.Text.Format(GameEntry.BuiltinData.Builtin.BuildInfo.CheckVersionUrl, GameEntry.BuiltinData.Builtin.BuildInfo.LatestGameVersion, GetPlatformPath());
            Log.Debug(checkVersionUrl);
            // 向服务器请求版本信息
            WebRequestResult result = await GameEntry.WebRequest.Get(checkVersionUrl);
            if (result.Success)
            {
                // 解析版本信息
                byte[] versionInfoBytes = result.Bytes;
                string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
                Log.Debug(versionInfoString);
                m_VersionInfo = Utility.Json.ToObject<VersionInfo>(versionInfoString);
                if (m_VersionInfo == null)
                {
                    Log.Error("Parse VersionInfo failure.");
                    return;
                }

                Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.", m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalGameVersion.ToString(), Version.GameVersion, Version.InternalGameVersion.ToString());
                GameEntry.Setting.SetInt("InternalResourceVersion", m_VersionInfo.InternalResourceVersion);
                GameEntry.Setting.Save();
                m_NeedUpdateVersion = GameEntry.Resource.CheckVersionList(m_VersionInfo.InternalResourceVersion) == CheckVersionListResult.NeedUpdate;
                if (m_VersionInfo.ForceUpdateGame)
                {
                    //需要强制更新游戏应用
                    GameEntry.BuiltinData.OpenDialog(new DialogParams
                    {
                        Mode = 1,
                        Title = GameEntry.Localization.GetString("ForceUpdate.Title"),
                        Message = GameEntry.Localization.GetString("ForceUpdate.Message"),
                        ConfirmText = GameEntry.Localization.GetString("ForceUpdate.UpdateButton"),
                        OnClickConfirm = GotoUpdateApp,
                        CancelText = GameEntry.Localization.GetString("ForceUpdate.QuitButton"),
                        OnClickCancel = delegate(object userData) { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                    });
                    return;
                }

                // 设置资源更新下载地址
                GameEntry.Resource.UpdatePrefixUri = Utility.Path.GetRegularPath(m_VersionInfo.UpdatePrefixUri);
                m_CheckVersionComplete = true;
            }
            else
            {
                Log.Warning("Check version failure, error message is '{0}'.", result.ErrorMessage);
            }
        }

        private void GotoUpdateApp(object userData)
        {
            string url = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = GameEntry.BuiltinData.Builtin.BuildInfo.WindowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = GameEntry.BuiltinData.Builtin.BuildInfo.MacOSAppUrl;
#elif UNITY_IOS
            url = GameEntry.BuiltinData.Builtin.BuildInfo.IOSAppUrl;
#elif UNITY_ANDROID
            url = GameEntry.BuiltinData.Builtin.BuildInfo.AndroidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit);
            }
        }

        private string GetPlatformPath()
        {
            // 这里和 GameBuildEventHandler.GetPlatformPath() 对应。
            // 使用 平台标识符 关联 UnityEngine.RuntimePlatform 和 DEngine.Editor.ResourceTools.Platform
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerARM:
                    return "WSA";

                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";

                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";

                default:
                    throw new System.NotSupportedException(Utility.Text.Format("Platform '{0}' is not supported.", Application.platform));
            }
        }
    }
}