using Cysharp.Threading.Tasks;

namespace Game.Editor.ResourceTools
{
    public interface IHostingService
    {
        /// <summary>
        /// 是否正在监听
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// 启动服务并监听指定端口
        /// </summary>
        /// <param name="port"></param>
        void StartService(int port); // 

        /// <summary>
        /// 停止服务
        /// </summary>
        void StopService();

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="relativePath"></param>
        UniTask UploadFile(string filePath, string relativePath);
    }
}