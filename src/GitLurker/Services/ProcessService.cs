using System;
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

        public Task ExecuteCommandAsync(string command) => ExecuteCommandAsync(command, false);

        public Task ExecuteCommandAsync(string command, string workingDirectory) => ExecuteCommandAsync(command, false, workingDirectory);

        public Task ExecuteCommandAsync(string command, bool listen) => ExecuteCommandAsync(command, listen, _folder);

        public Task ExecuteCommandAsync(string command, bool listen, string workingDirectory)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = workingDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = listen,
                    UseShellExecute = !listen,
                    RedirectStandardOutput = listen,
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                },
            };

            if (listen)
            {
                DataReceivedEventHandler handler = default;
                handler = (s, a) =>
                {
                    if (string.IsNullOrEmpty(a.Data))
                    {
                        process.OutputDataReceived -= handler;
                        return;
                    }

                    NewProcessMessage?.Invoke(this, a.Data);
                };

                process.OutputDataReceived += handler;
            }

            process.Start();

            if (listen)
            {
                process.BeginOutputReadLine();
                return process.WaitForExitAsync();
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
