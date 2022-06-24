using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        #endregion

        #region Methods

        public Task<ExecutionResult> ExecuteCommandAsync(string command) => ExecuteCommandAsync(command, false);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, string workingDirectory) => ExecuteCommandAsync(command, false, workingDirectory);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, bool listen) => ExecuteCommandAsync(command, listen, _folder);

        public Task<ExecutionResult> ExecuteCommandAsync(string command, bool listen, string workingDirectory)
        {
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
                taskCompletionSource.SetResult(new ExecutionResult()
                {
                    Output = data,
                    ExitCode = process.ExitCode,
                });
            });

            return taskCompletionSource.Task;
        }

        #endregion
    }
}
