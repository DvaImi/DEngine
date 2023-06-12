// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 00:33:27
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityGameFramework.Runtime;

//自动生成于：2023/4/16 0:33:27
namespace Game.Hotfix
{
    public partial class WelcomeForm : HotfixUGuiForm
    {
        public Text m_Text;
        public Dropdown dropdown;
        public Button webTest;
        public RawImage[] images;
        public Button play;
        public RawImage video;
        VideoPlayer videoPlayer;
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
            play.onClick.AddListener(PlayerVideo);
        }



        private async void WebRequestTest()
        {
            for (int i = 0; i < images.Length; i++)
            {
                WebRequestResult result = await GameEntry.WebRequest.AddWebRequestAsync($"http://192.168.1.102/CDN/{i}.jpg");
                if (result.Success)
                {
                    Texture2D texture = new Texture2D((int)images[i].rectTransform.rect.width, (int)images[i].rectTransform.rect.height);
                    texture.LoadImage(result.Bytes);
                    images[i].texture = texture;
                }
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
            if (videoPlayer == null)
            {
                return;
            }

            if (videoPlayer.isPrepared)
            {
                video.texture = videoPlayer.texture;
            }
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
            m_Text.text += GameEntry.Config.GetString("TestValue");
        }


        private async void PlayerVideo()
        {
            VideoClip videoClip = await GameEntry.Resource.LoadAssetAsync<VideoClip>("Assets/Game/Ori.mp4");
            videoPlayer = video.gameObject.GetOrAddComponent<VideoPlayer>();
            videoPlayer.clip = videoClip;
            videoPlayer.Prepare();
            videoPlayer.Play();

        }
    }
}