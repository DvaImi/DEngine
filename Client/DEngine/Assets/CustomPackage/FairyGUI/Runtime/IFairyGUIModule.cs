using Cysharp.Threading.Tasks;

namespace Game.FairyGUI.Runtime
{
    public interface IFairyGUIModule : IGameUpdateModule
    {
        /// <summary>
        /// 获取包体数量
        /// </summary>
        int PackageCount { get; }

        /// <summary>
        /// 自动释放间隔
        /// </summary>
        float InstanceAutoReleaseInterval { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="fairyDataAssetName">资源信息</param>
        /// <returns></returns>
        UniTask Initialize(string fairyDataAssetName);

        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <returns></returns>
        FairyGUIFormBase GetUIForm(int uiFormId);

        /// <summary>
        /// 异步打开界面
        /// </summary>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<T> OpenUIForm<T>(object userData = null) where T : FairyGUIFormBase;

        /// <summary>
        /// 异步打开界面
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<T> OpenUIForm<T>(int uiFormId, object userData = null) where T : FairyGUIFormBase;

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void CloseUIForm<T>() where T : FairyGUIFormBase;

        /// <summary>
        /// 关闭界面并传入数据
        /// </summary>
        /// <param name="userData"></param>
        /// <typeparam name="T"></typeparam>
        void CloseUIForm<T>(object userData) where T : FairyGUIFormBase;

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="fairyForm"></param>
        void CloseUIForm(FairyGUIFormBase fairyForm);

        /// <summary>
        /// 关闭界面并传入数据
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <param name="userData"></param>
        void CloseUIForm(int uiFormId, object userData = null);

        /// <summary>
        /// 激活界面并传入数据
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <param name="userData"></param>
        void RefocusUIForm(int uiFormId, object userData = null);

        /// <summary>
        /// 激活界面
        /// </summary>
        /// <param name="fairyForm"></param>
        void RefocusUIForm(FairyGUIFormBase fairyForm);

        /// <summary>
        /// 获取包的引用计数
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        int GetPackageReferenceCount(string packageName);
    }
}