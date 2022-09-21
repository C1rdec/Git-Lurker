using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GitLurker.Models;

namespace GitLurker.Services
{
    public class ProcessService
    {
        #region Fields

        private string _folder;

        #endregion

        #region Constructors

        public ProcessService(string folder)
        {
            _folder = folder;
        }

        #endregion

        #region Events

        public event EventHandler<string> NewProcessMessage;

        public event EventHandler<int> NewExitCode;

        #endregion

        #region Methods

        public Task<ExecutionResult> ExecuteCommandAsync(string command) => ExecuteCommandAsync(command, false);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, string workingDirectory) => ExecuteCommandAsync(command, false, workingDirectory);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, bool listen) => ExecuteCommandAsync(command, listen, _folder);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, bool listen, string workingDirectory)
        {
            if (!string.IsNullOrEmpty(_folder) && !Directory.Exists(_folder))
            {
                if (listen)
                {
                    NewExitCode?.Invoke(this, -1);
                }

                return Task.FromResult(new ExecutionResult());
            }

            var taskCompletionSource = new TaskCompletionSource<ExecutionResult>();
            var data = new List<string>();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = workingDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                },
            };

            DataReceivedEventHandler handler = default;
            handler = (s, a) =>
            {
                data.Add(a.Data);

                if (listen && a.Data != null)
                {
                    NewProcessMessage?.Invoke(this, a.Data);
                }
            };

            process.OutputDataReceived += handler;

            process.Start();
            process.BeginOutputReadLine();
            Task.Run(() => process.WaitForExit()).ContinueWith(t => 
            {
                process.OutputDataReceived -= handler;
                if (listen)
                {
                    NewExitCode?.Invoke(this, process.ExitCode);
                }

                taskCompletionSource.SetResult(new ExecutionResult()
                {
                    Output = data,
                    ExitCode = process.ExitCode,
                });
            });

            return taskCompletionSource.Task;
        }

        public void OpenUserSecret(string secretId)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folderPath = Path.Combine(appdata, "Microsoft", "UserSecrets", secretId);

            if (!Directory.Exists(folderPath))
            {
                return;
            }

            var filePath = Path.Combine(folderPath, "secrets.json");
            if (!File.Exists(filePath))
            {
                return;
            }

            OpenFile(filePath);
            NewExitCode?.Invoke(this, -1);
        }

        protected void OpenFile(string filePath)
        {
            new Process()
            {
                StartInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true,
                }
            }.Start();
        }

        #endregion
    }
}
