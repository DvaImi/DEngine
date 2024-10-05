using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Runtime;
using FairyGUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.FairyGUI.Runtime
{
    public class FairyGUIModule : IFairyGUIModule, IGameModule
    {
        public int Priority => 1;
        private UIComponent m_UIComponent;
        private FairyGUIFormRuntimeData m_FormRuntimeData;
        private IFairyGUIModule m_FairyGUIModuleImplementation;
        private readonly Dictionary<string, int> m_FairyGUIReference = new();
        private readonly Queue<string> m_PreUnLoad = new();
        private float m_InstanceAutoReleaseInterval;
        private float m_Time;
        public int PackageCount => UIPackage.GetPackages().Count;

        public float InstanceAutoReleaseInterval
        {
            get { return m_InstanceAutoReleaseInterval; }
            set { m_InstanceAutoReleaseInterval = value; }
        }

        public async UniTask Initialize(string fairyDataAssetName)
        {
            m_UIComponent = GameEntry.UI;
            m_FormRuntimeData = await GameEntry.Resource.LoadAssetAsync<FairyGUIFormRuntimeData>(fairyDataAssetName);
            UIPackage.unloadBundleByFGUI = false;

            foreach (var fairyGroup in m_FormRuntimeData.FairyGroups)
            {
                m_UIComponent.AddUIGroup(fairyGroup.Name, fairyGroup.Depth);
            }

            if (m_InstanceAutoReleaseInterval < 60)
            {
                m_InstanceAutoReleaseInterval = 60;
            }
        }

        public FairyGUIFormBase GetUIForm(int uiFormId)
        {
            var groupName = GetGroupName(uiFormId);
            var assetName = GetFairyGUIFormAssetName(uiFormId);
            var form = m_UIComponent.GetUIForm(assetName);
            if (form == null)
            {
                return null;
            }

            var uiGroup = m_UIComponent.GetUIGroup(groupName);
            return ((UIForm)uiGroup?.GetUIForm(assetName))?.Logic as FairyGUIFormBase;
        }

        public async UniTask<T> OpenUIForm<T>(object userData = null) where T : FairyGUIFormBase
        {
            var fairyForm = m_FormRuntimeData.GetFairyForm(typeof(T).Name);
            if (fairyForm == null)
            {
                return null;
            }

            await AddPackage(fairyForm);

            if (!fairyForm.AllowMultiInstance)
            {
                if (m_UIComponent.IsLoadingUIForm(fairyForm.ObjectAssetName))
                {
                    return null;
                }

                if (m_UIComponent.HasUIForm(fairyForm.ObjectAssetName))
                {
                    return (m_UIComponent.GetUIForm(fairyForm.ObjectAssetName)).Logic as T;
                }
            }

            return (await m_UIComponent.OpenUIFormAsync(fairyForm.ObjectAssetName, fairyForm.UIGroupName, 0, fairyForm.PauseCoveredUIForm, userData)).Logic as T;
        }


        public async UniTask<T> OpenUIForm<T>(int uiFormId, object userData = null) where T : FairyGUIFormBase
        {
            var fairyForm = m_FormRuntimeData.GetFairyForm(uiFormId);

            if (fairyForm == null)
            {
                return null;
            }

            await AddPackage(fairyForm);

            if (!fairyForm.AllowMultiInstance)
            {
                if (m_UIComponent.IsLoadingUIForm(fairyForm.ObjectAssetName))
                {
                    return null;
                }

                if (m_UIComponent.HasUIForm(fairyForm.ObjectAssetName))
                {
                    return (m_UIComponent.GetUIForm(fairyForm.ObjectAssetName)).Logic as T;
                }
            }

            return (await m_UIComponent.OpenUIFormAsync(fairyForm.ObjectAssetName, fairyForm.UIGroupName, 0, fairyForm.PauseCoveredUIForm, userData)).Logic as T;
        }

        public void CloseUIForm<T>() where T : FairyGUIFormBase
        {
            var fairyForm = m_FormRuntimeData.GetFairyForm(typeof(T).Name);
            if (fairyForm == null)
            {
                return;
            }

            CloseUIForm(fairyForm.Id);
        }


        public void CloseUIForm<T>(object userData) where T : FairyGUIFormBase
        {
            var fairyForm = m_FormRuntimeData.GetFairyForm(typeof(T).Name);
            if (fairyForm == null)
            {
                return;
            }

            CloseUIForm(fairyForm.Id, userData);
        }


        public void CloseUIForm(FairyGUIFormBase fairyForm)
        {
            if (!fairyForm)
            {
                return;
            }

            m_UIComponent.CloseUIForm(fairyForm.UIForm);
            RemoveReference(fairyForm.PackageName);
        }

        public void CloseUIForm(int uiFormId, object userData = null)
        {
            var fairyForm = GetUIForm(uiFormId);

            if (!fairyForm)
            {
                return;
            }

            m_UIComponent.CloseUIForm(fairyForm.UIForm, userData);
            RemoveReference(fairyForm.PackageName);
        }


        public void RefocusUIForm(int uiFormId, object userData = null)
        {
            var fairyForm = GetUIForm(uiFormId);

            if (!fairyForm)
            {
                return;
            }

            m_UIComponent.RefocusUIForm(fairyForm.UIForm, userData);
        }


        public void RefocusUIForm(FairyGUIFormBase fairyForm)
        {
            if (!fairyForm)
            {
                return;
            }

            m_UIComponent.RefocusUIForm(fairyForm.UIForm);
        }


        public int GetPackageReferenceCount(string packageName)
        {
            m_FairyGUIReference.TryGetValue(packageName, out var refCount);
            return refCount;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_Time += elapseSeconds;
            if (m_Time > m_InstanceAutoReleaseInterval)
            {
                RemovePackage();
                m_Time = 0;
            }
        }

        public void Shutdown()
        {
            UIPackage.RemoveAllPackages();
            UIPackage.branch = null;
            FontManager.Clear();
            NTexture.DisposeEmpty();
            UIObjectFactory.Clear();
            m_FairyGUIReference.Clear();
            m_PreUnLoad.Clear();
        }

        private string GetGroupName(int uiFormId)
        {
            return m_FormRuntimeData.GetFairyGroup(uiFormId)?.Name;
        }

        private string GetFairyGUIFormAssetName(int uiFormId)
        {
            return m_FormRuntimeData.GetFairyForm(uiFormId)?.ObjectAssetName;
        }

        private async UniTask AddPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                throw new DEngineException("packageName is null");
            }

            UIPackage uiPackage = UIPackage.GetByName(packageName);
            if (uiPackage == null)
            {
                var fairyForm = m_FormRuntimeData.GetFairyForm(packageName);
                if (fairyForm == null)
                {
                    Log.Error($"FairyForm is not exist. packageName is '{packageName}'");
                    return;
                }

                //load dependencies package
                foreach (var package in fairyForm.DependencyPackages)
                {
                    await AddPackage(package);
                }

                await InternalLoadFairyGUIAsset(fairyForm);
            }
            else
            {
                AddReference(packageName);
            }
        }

        private async UniTask AddPackage(FairyForm fairyForm)
        {
            if (fairyForm == null)
            {
                throw new DEngineException("fairyForm is null");
            }

            UIPackage uiPackage = UIPackage.GetByName(fairyForm.PackageName);
            if (uiPackage == null)
            {
                //load dependencies package
                foreach (var package in fairyForm.DependencyPackages)
                {
                    await AddPackage(package);
                }

                await InternalLoadFairyGUIAsset(fairyForm);
            }
            else
            {
                AddReference(fairyForm.PackageName);
            }
        }

        private async UniTask InternalLoadFairyGUIAsset(FairyForm fairyForm)
        {
            //load dependencies
            Object[] dependencies = await GameEntry.Resource.LoadAssetsAsync<Object>(fairyForm.DependencyAssets.ToArray());

            //load desc
            TextAsset desc = await GameEntry.Resource.LoadAssetAsync<TextAsset>(fairyForm.AssetName);

            UIPackage.AddPackage(desc.bytes, "", LoadFunc);

            AddReference(fairyForm.PackageName);

            object LoadFunc(string name, string extension, Type type, out DestroyMethod destroyMethod)
            {
                destroyMethod = DestroyMethod.None;
                name = Utility.Text.Format("{0}_{1}", fairyForm.PackageName, name);
                return dependencies.FirstOrDefault(res => string.Equals(res.name, name));
            }
        }

        private void AddReference(string packageName)
        {
            if (!m_FairyGUIReference.TryGetValue(packageName, out int refCout))
            {
                refCout = 0;
                m_FairyGUIReference.Add(packageName, refCout);
            }

            refCout++;
            m_FairyGUIReference[packageName] = refCout;
            Log.Info($"AddReference {packageName}");
        }

        private void RemoveReference(string packageName)
        {
            if (!m_FairyGUIReference.TryGetValue(packageName, out _))
            {
                throw new DEngineException(Utility.Text.Format("{0} is not exit", packageName));
            }

            m_FairyGUIReference[packageName]--;

            if (m_FairyGUIReference[packageName] <= 0 && UIPackage.GetPackages().Count != 0)
            {
                m_FairyGUIReference.Remove(packageName);
                if (!m_PreUnLoad.Contains(packageName))
                {
                    m_PreUnLoad.Enqueue(packageName);
                }
            }

            Log.Info($"RemoveReference {packageName}...");
        }

        private void RemovePackage()
        {
            while (m_PreUnLoad.Count > 0)
            {
                UIPackage.RemovePackage(m_PreUnLoad.Dequeue());
            }

            Log.Info("Remove package...");
        }
    }
}