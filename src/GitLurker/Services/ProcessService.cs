using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public Task<IEnumerable<string>> ExecuteCommandAsync(string command) => ExecuteCommandAsync(command, false);

        public Task<IEnumerable<string>> ExecuteCommandAsync(string command, string workingDirectory) => ExecuteCommandAsync(command, false, workingDirectory);

        public Task<IEnumerable<string>> ExecuteCommandAsync(string command, bool listen) => ExecuteCommandAsync(command, listen, _folder);

        public Task<IEnumerable<string>> ExecuteCommandAsync(string command, bool listen, string workingDirectory)
        {
            var taskCompletionSource = new TaskCompletionSource<IEnumerable<string>>();
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
                if (string.IsNullOrEmpty(a.Data))
                {
                    process.OutputDataReceived -= handler;
                    return;
                }

                data.Add(a.Data);

                if (listen)
                {
                    NewProcessMessage?.Invoke(this, a.Data);
                }
            };

            process.OutputDataReceived += handler;

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExitAsync().ContinueWith(t => 
            { 
                taskCompletionSource.SetResult(data);
            });

            return taskCompletionSource.Task;
        }

        #endregion
    }
}
