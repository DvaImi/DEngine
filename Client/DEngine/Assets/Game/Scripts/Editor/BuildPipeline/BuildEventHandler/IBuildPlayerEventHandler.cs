using DEngine.Editor.ResourceTools;

namespace Game.Editor.BuildPipeline
{
    public interface IBuildPlayerEventHandler
    {
        /// <summary>
        /// 获取当某个平台生成失败时，是否继续生成下一个平台。
        /// </summary>
        bool ContinueOnFailure { get; }

        /// <summary>
        /// 所有平台生成开始前的预处理事件。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="companyName">公司名称。</param>
        /// <param name="gameIdentifier">游戏识别号。</param>
        /// <param name="unityVersion">Unity 版本。</param>
        /// <param name="applicableGameVersion">适用游戏版本。</param>
        /// <param name="platforms">生成的目标平台。</param>
        /// <param name="outputDirectory">生成目录。</param>
        void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platforms, string outputDirectory);

        /// <summary>
        /// 某个平台生成开始前的预处理事件。
        /// </summary>
        /// <param name="platform">生成平台。</param>
        void OnPreprocessPlatform(Platform platform);

        /// <summary>
        /// 某个平台生成结束后的后处理事件。
        /// </summary>
        /// <param name="platform">生成平台。</param>
        /// <param name="isSuccess">是否生成成功。</param>
        void OnPostprocessPlatform(Platform platform, bool isSuccess);

        /// <summary>
        /// 所有平台生成结束后的后处理事件。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="companyName">公司名称。</param>
        /// <param name="gameIdentifier">游戏识别号。</param>
        /// <param name="unityVersion">Unity 版本。</param>
        /// <param name="applicableGameVersion">适用游戏版本。</param>
        /// <param name="platforms">生成的目标平台。</param>
        /// <param name="outputDirectory">生成目录。</param>
        void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platforms, string outputDirectory);
    }
}