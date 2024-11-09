using System.Collections.Generic;
using UnityEngine;

namespace Game.FairyGUI.Runtime
{
    public class FairyGUIFormRuntimeData : ScriptableObject
    {
        [SerializeField] private List<FairyGroup> fairyGroups = new();
        [SerializeField] private List<FairyForm> fairyForms = new();

        public List<FairyGroup> FairyGroups => fairyGroups;

        public List<FairyForm> FairyForms => fairyForms;

        public FairyGroup GetFairyGroup(int uiFormId)
        {
            var form = GetFairyForm(uiFormId);
            return form == null ? null : FairyGroups.Find(o => o.groupName == form.uiGroupName);
        }

        public FairyGroup GetFairyGroup(string packageName)
        {
            var form = GetFairyForm(packageName);
            return form == null ? null : FairyGroups.Find(o => o.groupName == form.uiGroupName);
        }

        public FairyForm GetFairyForm(int uiFormId)
        {
            return FairyForms.Find(o => o.id == uiFormId);
        }

        public FairyForm GetFairyForm(string packageName)
        {
            return FairyForms.Find(o => o.packageName == packageName);
        }
    }
}