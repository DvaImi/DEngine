using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.CommandLine
{

    /// <summary>use to record command input history</summary>
    class InputHistory
    {

        readonly List<string> history;
        readonly int capacity;
        int lastIndex;

        public InputHistory(int capacity)
        {

            this.lastIndex = 0;
            this.capacity = capacity;
            this.history = new List<string>();
        }
        /// <summary>record input string</summary>
        public void Record(string input)
        {

            if (history.Count == capacity)
            {
                history.RemoveAt(0);
            }
            history.Add(input);
            lastIndex = history.Count - 1;
        }

        /// <summary>get last input string</summary>
        public string Last
        {
            get
            {
                if (history.Count == 0) return string.Empty;
                if (lastIndex < 0 || lastIndex >= history.Count) lastIndex = history.Count - 1;
                return history[lastIndex--];
            }
        }

        /// <summary>get next input string</summary>
        public string Next
        {
            get
            {
                if (history.Count == 0) return string.Empty;
                if (lastIndex >= history.Count || lastIndex < 0) lastIndex = 0;
                return history[lastIndex++];
            }
        }
    }


    /// <summary>command selector</summary>
    class LinearSelector
    {

        public event Action<int> OnSelectionChanged;
        readonly List<string> optionsBuffer;
        int currentIndex;

        int TotalCount
        {
            get
            {
                if (optionsBuffer == null) return 0;
                return optionsBuffer.Count;
            }
        }

        /// <summary>
        /// the current selection of the alternative options
        /// if value is -1, it means there's no alternative options choosed
        /// </summary>
        public int SelectionIndex => currentIndex;

        public LinearSelector()
        {

            this.optionsBuffer = new();
            this.currentIndex = -1;
        }

        public void LoadOptions(List<string> options)
        {

            this.optionsBuffer.Clear();
            this.optionsBuffer.AddRange(options);
            this.currentIndex = -1;
        }

        public bool GetCurrentSelection(out string selection)
        {

            selection = string.Empty;
            if (currentIndex == -1) return false;
            selection = optionsBuffer[currentIndex];
            return true;
        }

        /// <summary>move to next alternative option</summary>
        public void MoveNext()
        {

            if (TotalCount == 0) return;
            if (currentIndex == -1)
            {
                currentIndex = 0;
                OnSelectionChanged?.Invoke(currentIndex);
                return;
            }
            currentIndex = currentIndex < TotalCount - 1 ? currentIndex + 1 : 0;
            OnSelectionChanged?.Invoke(currentIndex);
        }

        /// <summary>move to last alternative option</summary>
        public void MoveLast()
        {

            if (TotalCount == 0) return;
            if (currentIndex == -1)
            {
                currentIndex = TotalCount - 1;
                OnSelectionChanged?.Invoke(currentIndex);
                return;
            }
            currentIndex = currentIndex > 0 ? currentIndex - 1 : TotalCount - 1;
            OnSelectionChanged?.Invoke(currentIndex);
        }
    }
}