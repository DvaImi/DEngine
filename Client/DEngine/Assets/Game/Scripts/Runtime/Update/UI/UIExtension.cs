using System.Collections;
using Cysharp.Threading.Tasks;
using DEngine.DataTable;
using DEngine.Runtime;
using DEngine.UI;
using Game.Update.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Update
{
    public static class UIExtension
    {
        public static IEnumerator FadeToAlpha(this CanvasGroup self, float alpha, float duration)
        {
            float time = 0f;
            float originalAlpha = self.alpha;
            while (time < duration)
            {
                time += Time.deltaTime;
                self.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
                yield return new WaitForEndOfFrame();
            }

            self.alpha = alpha;
        }

        public static IEnumerator SmoothValue(this Slider self, float value, float duration)
        {
            float time = 0f;
            float originalValue = self.value;
            while (time < duration)
            {
                time += Time.deltaTime;
                self.value = Mathf.Lerp(originalValue, value, time / duration);
                yield return new WaitForEndOfFrame();
            }

            self.value = value;
        }

        public static async UniTask FadeToAlphaByUniTask(this CanvasGroup self, float alpha, float duration)
        {
            float time = 0f;
            float originalAlpha = self.alpha;
            while (time < duration)
            {
                time += Time.deltaTime;
                self.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
                await UniTask.Yield();
            }

            self.alpha = alpha;
        }

        public static void SetGlobalUIClickSound(this UIComponent self, int soundId)
        {
            SoundButton.GlobalUIClickSoundHandler -= OnSoundButtonOnGlobalUIClickSoundHandler;
            SoundButton.GlobalUIClickSoundHandler += OnSoundButtonOnGlobalUIClickSoundHandler;
            return;

            void OnSoundButtonOnGlobalUIClickSoundHandler()
            {
                GameEntry.Sound.PlayUISound(soundId);
            }
        }

        public static UIFormLogic GetUIForm(this UIComponent self, UIFormId uiFormId, string uiGroupName = null)
        {
            return self.GetUIForm((int)uiFormId, uiGroupName);
        }

        public static UIFormLogic GetUIForm(this UIComponent self, int uiFormId, string uiGroupName = null)
        {
            IDataTable<DRUIForm> dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                return null;
            }

            string assetName = UpdateAssetUtility.GetUIFormAsset(drUIForm.AssetName);
            UIForm uiForm;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = self.GetUIForm(assetName);
                return uiForm == null ? null : uiForm.Logic;
            }

            IUIGroup uiGroup = self.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return null;
            }

            uiForm = (UIForm)uiGroup.GetUIForm(assetName);
            return uiForm == null ? null : uiForm.Logic;
        }

        public static int? OpenUIForm(this UIComponent self, UIFormId uiFormId, object userData = null)
        {
            return self.OpenUIForm((int)uiFormId, userData);
        }

        public static int? OpenUIForm(this UIComponent self, int uiFormId, object userData = null)
        {
            IDataTable<DRUIForm> dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                Log.Warning("Can not load UI form '{0}' from data table.", uiFormId.ToString());
                return null;
            }

            string assetName = UpdateAssetUtility.GetUIFormAsset(drUIForm.AssetName);
            if (!drUIForm.AllowMultiInstance)
            {
                if (self.IsLoadingUIForm(assetName))
                {
                    return null;
                }

                if (self.HasUIForm(assetName))
                {
                    return null;
                }
            }

            return self.OpenUIForm(assetName, drUIForm.UIGroupName, Constant.AssetPriority.UIFormAsset, drUIForm.PauseCoveredUIForm, userData);
        }

        public static void CloseUIForm(this UIComponent self, UIFormLogic uiForm)
        {
            self.CloseUIForm(uiForm.UIForm);
        }

        public static void CloseUIForm(this UIComponent self, UIFormId uiFormId, object userData = null)
        {
            var uiform = self.GetUIForm(uiFormId);
            if (uiform == null)
            {
                return;
            }

            self.CloseUIForm(uiform.UIForm);
        }
    }
}