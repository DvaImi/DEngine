namespace Game.Editor.FairyGUI
{
    [GameFilePath("Assets/Game/Configuration/FairyGUIEditorSetting.asset")]
    public class FairyGUIEditorSetting : ScriptableSingleton<FairyGUIEditorSetting>
    {
        /// <summary>
        /// fairyGui 项目路径
        /// </summary>
        public string FairyGUIProject;

        /// <summary>
        /// fairyGui 数据的路径
        /// </summary>
        public string FairyGUIDataPath;

        /// <summary>
        /// 生成代码的路径
        /// </summary>
        public string GeneralCodePath;

        /// <summary>
        /// 生成预制体的路径
        /// </summary>
        public string GeneralObjectAssetName;
    }
}