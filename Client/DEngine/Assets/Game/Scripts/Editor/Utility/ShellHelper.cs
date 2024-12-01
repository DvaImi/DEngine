using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public static class ShellHelper
    {
        public static void Run(string cmd, string workDirectory)
        {
            Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                string app       = "cmd.exe";
                string arguments = "/c";
#endif
                ProcessStartInfo start = new ProcessStartInfo(app);
                process.StartInfo      = start;
                start.Arguments        = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow   = false;
                start.ErrorDialog      = true;
                start.UseShellExecute  = true;
                start.WorkingDirectory = workDirectory;

                process.Start();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void RunV2(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            Process process = new();
            try
            {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                string app = "bash";
                string splitChar = ":";
                string arguments = "-c";
#elif UNITY_EDITOR_WIN
                string app       = "cmd.exe";
                string splitChar = ";";
                string arguments = "/c";
#endif
                ProcessStartInfo start = new ProcessStartInfo(app);

                if (environmentVars != null)
                {
                    foreach (string var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += (splitChar + var);
                    }
                }

                process.StartInfo      = start;
                start.Arguments        = arguments + " \"" + cmd + "\"";
                start.CreateNoWindow   = true;
                start.ErrorDialog      = true;
                start.UseShellExecute  = false;
                start.WorkingDirectory = workDirectory;

                if (start.UseShellExecute)
                {
                    start.RedirectStandardOutput = false;
                    start.RedirectStandardError  = false;
                    start.RedirectStandardInput  = false;
                }
                else
                {
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError  = true;
                    start.RedirectStandardInput  = true;
                    start.StandardOutputEncoding = Encoding.UTF8;
                    start.StandardErrorEncoding  = Encoding.UTF8;
                }

                bool endOutput = false;
                bool endError  = false;

                process.OutputDataReceived += (_, args) =>
                {
                    if (string.IsNullOrWhiteSpace(args.Data))
                    {
                        endOutput = true;
                    }
                    else
                    {
                        Debug.Log(args.Data);
                    }
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (string.IsNullOrWhiteSpace(args.Data))
                    {
                        endError = true;
                    }
                    else
                    {
                        Debug.Log(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!endOutput || !endError)
                {
                }

                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                process.Close();
            }
        }
    }
}