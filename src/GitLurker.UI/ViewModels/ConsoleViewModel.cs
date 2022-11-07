using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Services;
using GitLurker.UI.Models;
using GitLurker.UI.Services;

namespace GitLurker.UI.ViewModels
{
    public class ConsoleViewModel : PropertyChangedBase, IDisposable
    {
        #region Fields

        private ProcessService _processService;
        private ConsoleService _consoleService;
        private int _exitCode;
        private bool _isLoading;

        #endregion

        #region Constructors

        public ConsoleViewModel(ConsoleService service)
        {
            Lines = new ObservableCollection<ConsoleLine>();
            _consoleService = service;
            _consoleService.ExecutionRequested += ConsoleService_ExecutionRequested;
        }

        #endregion

        #region Events

        public event EventHandler<bool> OnExecute;

        #endregion

        #region Properties

        public ObservableCollection<ConsoleLine> Lines { get; set; }

        public int ExitCode
        {
            get => _exitCode;
            set
            {
                _exitCode = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            _consoleService.ExecutionRequested -= ConsoleService_ExecutionRequested;
        }

        private static bool IsLineInError(string line)
        {
            if (line.StartsWith("conflict", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (line.EndsWith("needs merge", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void Repository_NewExitCode(object sender, int code)
        {
            IsLoading = false;
            OnExecute?.Invoke(this, false);
        }

        private void Repository_NewProcessMessage(object sender, string e)
        {
            if (string.IsNullOrEmpty(e))
            {
                return;
            }

            var consoleLine = new ConsoleLine
            {
                Line = e,
            };

            if (IsLineInError(e))
            {
                consoleLine.IsError = true;
            }

            Execute.OnUIThread(() => Lines.Add(consoleLine));
        }

        private void ConsoleService_ExecutionRequested(object sender, ProcessService process)
        {
            if (process == null)
            {
                return;
            }

            if (_processService != null)
            {
                Execute.OnUIThread(() => 
                { 
                    Lines.Clear();
                });
                _processService.NewProcessMessage -= Repository_NewProcessMessage;
            }

            IsLoading = true;
            OnExecute?.Invoke(this, true);
            _processService = process;
            _processService.NewProcessMessage += Repository_NewProcessMessage;
            _processService.NewExitCode += Repository_NewExitCode;
        }

        #endregion
    }
}
