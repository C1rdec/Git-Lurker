using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CliWrap.EventStream;
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

        public event EventHandler<CLIEvent> NewProcessMessage;

        public event EventHandler<int> NewExitCode;

        #endregion

        #region Methods

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments) => ExecuteCommandAsync(arguments, false);

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, string workingDirectory) => ExecuteCommandAsync(arguments, false, workingDirectory);

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, bool listen) => ExecuteCommandAsync(arguments, listen, _folder);


        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, bool listen, string workingDirectory)
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

            var command = CliWrap.Cli
                .Wrap("cmd.exe")
                .WithWorkingDirectory(workingDirectory)
                .WithArguments($"/C {arguments}");


            _ = Task.Run(async () =>
            {
                await foreach (var cmdEvent in command.ListenAsync())
                {
                    switch (cmdEvent)
                    {
                        case StandardErrorCommandEvent error:
                            HandleProcessMessage(error.Text, false, data, listen);
                            break;
                        case StandardOutputCommandEvent standard:
                            HandleProcessMessage(standard.Text, false, data, listen);
                            break;
                        case ExitedCommandEvent exit:
                            if (listen)
                            {
                                NewExitCode?.Invoke(this, exit.ExitCode);
                            }

                            taskCompletionSource.SetResult(new ExecutionResult()
                            {
                                Output = data,
                                ExitCode = exit.ExitCode,
                            });
                            break;
                    }
                }
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

        private void HandleProcessMessage(string text, bool isError, List<string> data, bool listen)
        {
            data.Add(text);

            if (listen && text != null)
            {
                NewProcessMessage?.Invoke(this, new CLIEvent()
                {
                    Text = text,
                    IsError = isError,
                });
            }
        }

        #endregion
    }
}
