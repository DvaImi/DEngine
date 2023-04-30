// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:33:27
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using GameFramework;
using GameFramework.Localization;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 0:33:27
namespace GeminiLion.Hotfix
{

    public partial class WelcomeForm : HotfixUGuiForm
    {
        public Text m_Text;
        public Dropdown dropdown;
        protected override void OnInit(object userdata)
        {
            base.OnInit(userdata);
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData()
                {
                    text = "ChineseSimplified"
                },
                new Dropdown.OptionData()
                {
                    text = "ChineseTraditional"
                } ,
                new Dropdown.OptionData()
                {
                    text = "English"
                }
            };
            dropdown.AddOptions(options);
            dropdown.value = GameEntry.Setting.GetInt("Dropdown");
            dropdown.onValueChanged.AddListener(ChangeLanguage);
        }

        private void ChangeLanguage(int arg0)
        {
            Language m_SelectedLanguage = default;
            if (arg0 == 0)
            {
                m_SelectedLanguage = Language.ChineseSimplified;
            }
            if (arg0 == 1)
            {
                m_SelectedLanguage = Language.ChineseTraditional;
            }
            if (arg0 == 2)
            {
                m_SelectedLanguage = Language.English;
            }
            GameEntry.Setting.SetString(Constant.Setting.Language, m_SelectedLanguage.ToString());
            GameEntry.Setting.SetInt("Dropdown", arg0);
            GameEntry.Setting.Save();
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Restart);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_Text.text += "\n";
            DRUIForm[] dataTables = GameEntry.DataTable.GetDataTable<DRUIForm>().GetAllDataRows();
            for (int i = 0; i < dataTables.Length; i++)
            {
                m_Text.text += dataTables[i].Id + "\n" + dataTables[i].AssetName + "\n" + dataTables[i].UIGroupName + "\n";
            }
            m_Text.text += GameEntry.Localization.GetString("CheckVersion.Tips");
            m_Text.text += "\n";
            m_Text.text += "\n";

            AssetInfoMap AssetInfo = GameEntry.BuiltinData.AssetInfo;
            string json = Utility.Json.ToJson(AssetInfo);
            m_Text.text += json;
            Log.Info("序列化出来的的" + json);

        }
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}
