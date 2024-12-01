using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureCheckVersion : GameProcedureBase
    {
        private bool m_CheckVersionComplete;
        private bool m_NeedUpdateVersion;
        private VersionInfo m_VersionInfo;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            //检测连网状态
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (GameEntry.BuiltinData.ForceCheckVersion)
                {
                    Log.Warning("The device is not connected to the network");
                    GameEntry.BuiltinData.OpenDialog(new DialogParams
                    {
                        Mode = 1,
                        Message = "The device is not connected to the network",
                        ConfirmText = "Quit",
                        OnClickConfirm = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                    });
                    return;
                }

                TryUseLastLocalVersionResource();
                return;
            }

            string checkVersionUrl = GameEntry.BuiltinData.Builtin.BuildInfo.CheckVersionUrl;
            Log.Debug(checkVersionUrl);
            GameEntry.WebRequest.Get(checkVersionUrl, OnRequestFinished);
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
                procedureOwner.SetData<VarBoolean>(Constant.Resource.IsResourcePackMode, m_VersionInfo.IsCompressedMode);
                procedureOwner.SetData<VarInt32>(Constant.Resource.VersionListLength, m_VersionInfo.ResourceVersionInfo.VersionListLength);
                procedureOwner.SetData<VarInt32>(Constant.Resource.VersionListHashCode, m_VersionInfo.ResourceVersionInfo.VersionListHashCode);
                procedureOwner.SetData<VarInt32>(Constant.Resource.VersionListCompressedLength, m_VersionInfo.ResourceVersionInfo.VersionListCompressedLength);
                procedureOwner.SetData<VarInt32>(Constant.Resource.VersionListCompressedHashCode, m_VersionInfo.ResourceVersionInfo.VersionListCompressedHashCode);
                if (m_VersionInfo.IsCompressedMode)
                {
                    procedureOwner.SetData<VarString>(Constant.Resource.ResourcePackName, m_VersionInfo.ResourcePackInfo.ResourcePackName);
                    procedureOwner.SetData<VarInt64>(Constant.Resource.ResourcePackLength, m_VersionInfo.ResourcePackInfo.ResourcePackLength);
                }

                ChangeState<ProcedureUpdateVersionList>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }

        private void OnRequestFinished(WebRequestResult result)
        {
            if (result.Success)
            {
                // 解析版本信息
                m_VersionInfo = result.ToObject<VersionInfo>();
                if (m_VersionInfo == null)
                {
                    Log.Error("Parse VersionInfo failure.");
                    return;
                }

                Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.", m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalGameVersion.ToString(), Version.GameVersion, Version.InternalGameVersion.ToString());
                GameEntry.Setting.SetInt(Constant.Resource.InternalResourceVersion, m_VersionInfo.InternalResourceVersion);
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
                        OnClickCancel = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
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
                if (GameEntry.BuiltinData.ForceCheckVersion)
                {
                    return;
                }

                TryUseLastLocalVersionResource();
            }
        }

        private void TryUseLastLocalVersionResource()
        {
            Log.Info("Try to use the latest local resource version.");
            if (GameEntry.Setting.HasSetting(Constant.Resource.InternalResourceVersion))
            {
                int internalResourceVersion = GameEntry.Setting.GetInt(Constant.Resource.InternalResourceVersion);
                if (internalResourceVersion > 0)
                {
                    GameEntry.BuiltinData.OpenDialog(new DialogParams
                    {
                        Mode = 2,
                        Message = "Try to use the latest local resource version.",
                        ConfirmText = "Confirm",
                        CancelText = "Quit",
                        OnClickConfirm = _ =>
                        {
                            m_NeedUpdateVersion = GameEntry.Resource.CheckVersionList(internalResourceVersion) == CheckVersionListResult.NeedUpdate;
                            m_CheckVersionComplete = true;
                        },
                        OnClickCancel = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                    });
                }
            }

            GameEntry.BuiltinData.OpenDialog(new DialogParams
            {
                Mode = 1,
                Message = "Try to use the latest local resource version failure.",
                ConfirmText = "Quit",
                OnClickConfirm = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            });
        }

        private static void GotoUpdateApp(object userData)
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
    }
}