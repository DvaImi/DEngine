using System;
using System.IO;
using DEngine;
using DEngine.Event;
using DEngine.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Update
{
    public class DemoForm : UpdateUGUIForm
    {
        public GameObject item;

        public string[] taskUri;
        private long[] taskSize;
        private long[] deltaLength;
        private DateTime[] lastMeasurementTime;
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            TestTask();

            GameEntry.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
        }

        private async void TestTask()
        {
            taskSize = new long[taskUri.Length];
            deltaLength = new long[taskUri.Length];
            lastMeasurementTime = new DateTime[taskUri.Length];
            for (int i = 0; i < taskUri.Length; i++)
            {
                int index = i;
                var obj = Instantiate(item, item.transform.parent);
                obj.SetActive(true);
                var size = await GameEntry.WebRequest.AddHeadWebRequest(taskUri[i], "Content-Length");
                taskSize[index] = long.Parse(size);
                lastMeasurementTime[index] = DateTime.Now;
                Slider slider = obj.GetComponentInChildren<Slider>();
                Text text = obj.GetComponentInChildren<Text>();
                text.text = StringUtility.GetByteLengthString(taskSize[index]);
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    StartDownTask(index, slider, text);
                });
            }
        }

        private void StartDownTask(int index, Slider slider, Text text)
        {
            string url = taskUri[index];
            GameEntry.Download.AddDownload(Path.Combine(Application.persistentDataPath, Path.GetFileName(url)), url, userData: new TaskText(slider, text, index));
        }

        private void OnDownloadUpdate(object sender, GameEventArgs e)
        {
            DownloadUpdateEventArgs download = (DownloadUpdateEventArgs)e;

            var currentTime = DateTime.Now;
            if (download.UserData is TaskText taskText)
            {
                TimeSpan timeDifference = currentTime - lastMeasurementTime[taskText.index];
                if (timeDifference.TotalSeconds >= 1)
                {
                    //{0}/{1},{3:P0},{4}/s
                    float progressTotal = (float)download.CurrentLength / taskSize[taskText.index];
                    taskText.slider.value = progressTotal;
                    long speed = download.CurrentLength - deltaLength[taskText.index];
                    taskText.text.text = Utility.Text.Format("{0}/{1}   {2:P0}   {3}/s", StringUtility.GetByteLengthString(download.CurrentLength), StringUtility.GetByteLengthString(taskSize[taskText.index]), progressTotal.ToString("P2"), StringUtility.GetByteLengthString(speed));
                    deltaLength[taskText.index] = download.CurrentLength;
                    lastMeasurementTime[taskText.index] = currentTime;
                }
            }
        }
    }

    internal struct TaskText
    {
        public Slider slider;
        public Text text;
        public int index;

        public TaskText(Slider slider, Text text, int index)
        {
            this.slider = slider;
            this.text = text;
            this.index = index;
        }
    }
}