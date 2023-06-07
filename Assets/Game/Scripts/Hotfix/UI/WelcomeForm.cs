// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:33:27
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 0:33:27
namespace Game.Hotfix
{
    public partial class WelcomeForm : HotfixUGuiForm
    {
        public Text m_Text;
        public Dropdown dropdown;
        public Button webTest;
        public Button dowTest;

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
                },
                new Dropdown.OptionData()
                {
                    text = "English"
                }
            };
            dropdown.AddOptions(options);
            dropdown.value = GameEntry.Setting.GetInt("Dropdown");
            dropdown.onValueChanged.AddListener(ChangeLanguage);
            webTest.onClick.AddListener(WebRequestTest);
            dowTest.onClick.AddListener(DownLoadTest);
        }
        int number = 0;
        private async void DownLoadTest()
        {
            DownLoadResult downLoadResult = await GameEntry.Download.AddDownloadAsync(@$"E:\\Desktop\\Dow\PhotoWall{number}.zip", "https://codeload.github.com/DvaImi/PhotoWall/zip/refs/heads/master");
            if (downLoadResult.Success)
            {
                Log.Debug(downLoadResult);
            }
            number += 1;
        }

        private async void WebRequestTest()
        {
            WebRequestResult result = await GameEntry.WebRequest.AddWebRequestWithHeaderAsync(@"E:\\Desktop\\BuildInfo.txt", UnityWebRequestHeader.Creat(null));
            if (result.Success)
            {
                Log.Debug(Utility.Converter.GetString(result.Bytes));
            }
        }

        private async void ChangeLanguage(int arg0)
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

            await GameEntry.Localization.LoadDictionaryAsync(m_SelectedLanguage);
            GameEntry.BuiltinData.InitLanguageBuiltin();
            GameEntry.Setting.SetString(Constant.Setting.Language, m_SelectedLanguage.ToString());
            GameEntry.Setting.SetInt("Dropdown", arg0);
            GameEntry.Setting.Save();
            ReadLanguage();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            ReadLanguage();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void ReadLanguage()
        {
            m_Text.text = string.Empty;
            m_Text.text += "\n";
            DRUIForm[] dataTables = GameEntry.DataTable.GetDataTable<DRUIForm>().GetAllDataRows();
            for (int i = 0; i < dataTables.Length; i++)
            {
                m_Text.text += dataTables[i].Id + "\n" + dataTables[i].AssetName + "\n" + dataTables[i].UIGroupName +
                               "\n";
            }
            m_Text.text += GameEntry.Localization.GetString("Game.Name");
            m_Text.text += "\n";
            m_Text.text += "\n";
            m_Text.text += "\n";
        }
    }
}