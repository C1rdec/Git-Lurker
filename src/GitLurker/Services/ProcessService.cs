using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.EventStream;
using GitLurker.Models;

namespace GitLurker.Services
{
    public class ProcessService
    {
        #region Fields

        private string _folder;
        private TaskCompletionSource _startedTaskCompletionSource;

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

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments) => ExecuteCommandAsync(arguments, false, _folder, CancellationToken.None);

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, string workingDirectory) => ExecuteCommandAsync(arguments, false, workingDirectory, CancellationToken.None);

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, bool listen) => ExecuteCommandAsync(arguments, listen, _folder, CancellationToken.None);

        public Task<ExecutionResult> ExecuteCommandAsync(string arguments, bool listen, string workingDirectory, CancellationToken token)
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
                try
                {
                    await foreach (var cmdEvent in command.ListenAsync(token))
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
                                return;
                        }
                    }
                }
                catch (Exception)
                {
                    if (listen)
                    {
                        NewExitCode?.Invoke(this, -1);
                    }

                    taskCompletionSource.SetResult(new ExecutionResult()
                    {
                        Output = data,
                        ExitCode = -1,
                    });
                }
            });
            

            return taskCompletionSource.Task;
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

        protected void SetExitCode(int code)
            => NewExitCode?.Invoke(this, code);

        protected Task WaitForStarted()
        {
            if (_startedTaskCompletionSource != null)
            {
                _startedTaskCompletionSource.SetResult();
            }

            _startedTaskCompletionSource = new TaskCompletionSource();
            Task.Delay(6000).ContinueWith(t => _startedTaskCompletionSource.SetResult());

            return _startedTaskCompletionSource.Task;
        }

        private void HandleProcessMessage(string text, bool isError, List<string> data, bool listen)
        {
            if (text.EndsWith("started."))
            {
                _startedTaskCompletionSource?.SetResult();
            }

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
