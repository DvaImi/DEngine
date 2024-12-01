using DEngine.Editor.ResourceTools;

namespace Game.Editor.BuildPipeline
{
    public interface IBuildPlayerEventHandler
    {
        /// <summary>
        /// 某个平台生成开始前的预处理事件。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="companyName">公司名称。</param>
        /// <param name="gameIdentifier">游戏识别号。</param>
        /// <param name="unityVersion">Unity 版本。</param>
        /// <param name="applicableGameVersion">适用游戏版本。</param>
        /// <param name="platform">生成的目标平台。</param>
        /// <param name="outputDirectory">生成目录。</param>
        void OnPreprocessPlatform(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platform, string outputDirectory);

        /// <summary>
        /// 某个平台生成结束后的后处理事件。
        /// </summary>
        /// <param name="productName">产品名称。</param>
        /// <param name="companyName">公司名称。</param>
        /// <param name="gameIdentifier">游戏识别号。</param>
        /// <param name="unityVersion">Unity 版本。</param>
        /// <param name="applicableGameVersion">适用游戏版本。</param>
        /// <param name="platform">生成的目标平台。</param>
        /// <param name="outputDirectory">生成目录。</param>
        /// <param name="isSuccess">是否生成成功。</param>
        void OnPostprocessPlatform(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platform, string outputDirectory, bool isSuccess);
    }
}