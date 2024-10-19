using System;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Editor.ResourceTools
{
    public class HttpHostingService : IHostingService
    {
        private HttpListener m_Listener;
        private string m_UploadUrl; // 固定的上传URL

        public bool IsListening => m_Listener?.IsListening ?? false;

        public void StartService(int port)
        {
            try
            {
                GameUtility.IO.CreateDirectoryIfNotExists(HostingServiceManager.HostingServicePath);

                m_Listener = new HttpListener();
                string url = $"{DEngineSetting.Instance.HostURL}:{port}/";
                m_Listener.Prefixes.Add(url);
                m_Listener.Start();
                m_Listener.BeginGetContext(OnRequestReceived, m_Listener);

                m_UploadUrl = $"{url}/upload"; // 固定上传路径
                Debug.Log($"Hosting service started on port {port}. Upload URL: {m_UploadUrl}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start hosting service: {ex.Message}");
            }
        }

        public void StopService()
        {
            if (m_Listener is { IsListening: true })
            {
                try
                {
                    m_Listener.Stop(); // 停止监听新的请求
                    m_Listener.Close(); // 释放资源
                    m_Listener = null;
                    Debug.Log("Hosting service stopped.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to stop hosting service: {ex.Message}");
                }
            }
        }


        public async UniTask UploadFile(string filePath, string relativePath)
        {
            try
            {
                // URL 编码文件相对路径
                string encodedRelativePath = UnityWebRequest.EscapeURL(relativePath);

                byte[] fileData = await File.ReadAllBytesAsync(filePath);
                using (UnityWebRequest request = new UnityWebRequest(m_UploadUrl, "POST"))
                {
                    // 设置上传的内容
                    request.uploadHandler = new UploadHandlerRaw(fileData);
                    request.uploadHandler.contentType = "application/octet-stream";
                    request.downloadHandler = new DownloadHandlerBuffer(); // 接收服务器的响应

                    // 将编码后的相对路径通过Header发送到服务器
                    request.SetRequestHeader("FileName", encodedRelativePath);

                    // 发送请求并等待响应
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("File uploaded successfully!");
                        Debug.Log($"Response: {request.downloadHandler.text}");
                    }
                    else
                    {
                        Debug.LogError($"File upload failed: {request.error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to upload file: {ex.Message}");
            }
        }


        private void OnRequestReceived(IAsyncResult result)
        {
            if (m_Listener is not { IsListening: true })
            {
                return;
            }

            HttpListenerContext context = null;
            try
            {
                context = m_Listener.EndGetContext(result);
                m_Listener.BeginGetContext(OnRequestReceived, m_Listener); // 继续监听

                if (context.Request.HttpMethod == "POST")
                {
                    HandleFileUpload(context); // 处理文件上传
                }
                else if (context.Request.HttpMethod == "GET")
                {
                    HandleFileDownload(context); // 处理文件下载
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling request: {ex.Message}");
                if (context != null)
                {
                    context.Response.StatusCode = 500;
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write("Internal server error.");
                    }

                    context.Response.Close();
                }
            }
        }

        private static void HandleFileUpload(HttpListenerContext context)
        {
            try
            {
                var fileStream = context.Request.InputStream;
                var fileName = context.Request.Headers["FileName"];

                // 解码文件名，处理 URL 编码中的斜杠问题
                fileName = WebUtility.UrlDecode(fileName);

                string uploadPath = Path.Combine(HostingServiceManager.HostingServicePath, fileName);

                FileInfo fileInfo = new(uploadPath);
                if (fileInfo.Directory is { Exists: false })
                {
                    fileInfo.Directory.Create();
                }

                using (var file = File.Create(uploadPath))
                {
                    fileStream.CopyTo(file);
                }

                context.Response.StatusCode = 200;
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("File uploaded successfully.");
                }

                Debug.Log($"File uploaded to: {uploadPath}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("Error uploading file: " + ex.Message);
                }

                Debug.LogError($"File upload error: {ex.Message}");
            }

            context.Response.Close();
        }


        private static void HandleFileDownload(HttpListenerContext context)
        {
            try
            {
                string requestedFile = context.Request.Url.LocalPath.TrimStart('/'); // 从URL获取请求的文件路径
                string filePath = Path.Combine(HostingServiceManager.HostingServicePath, requestedFile);

                if (File.Exists(filePath))
                {
                    byte[] fileData = File.ReadAllBytes(filePath);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/octet-stream"; // 二进制流
                    context.Response.ContentLength64 = fileData.Length;
                    context.Response.OutputStream.Write(fileData, 0, fileData.Length);
                    context.Response.OutputStream.Close();

                    Debug.Log($"File served: {filePath}");
                }
                else
                {
                    context.Response.StatusCode = 404;
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write("File not found.");
                    }

                    Debug.LogWarning($"File not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("Error downloading file: " + ex.Message);
                }

                Debug.LogError($"File download error: {ex.Message}");
            }

            context.Response.Close();
        }
    }
}