using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Services;
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
            Lines = new ObservableCollection<string>();
            _consoleService = service;
            _consoleService.ExecutionRequested += ConsoleService_ExecutionRequested;
        }

        #endregion

        #region Events

        public event EventHandler<bool> OnExecute;

        #endregion

        #region Properties

        public ObservableCollection<string> Lines { get; set; }

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

        public void Initialize(ProcessService processService)
        {
            if (_processService != null)
            {
                Lines.Clear();
                _processService.NewProcessMessage -= Repository_NewProcessMessage;
            }

            IsLoading = true;
            OnExecute?.Invoke(this, true);
            _processService = processService;
            _processService.NewProcessMessage += Repository_NewProcessMessage;
            _processService.NewExitCode += Repository_NewExitCode;
        }

        public void Dispose()
        {
            _consoleService.ExecutionRequested -= ConsoleService_ExecutionRequested;
        }

        private void Repository_NewExitCode(object sender, int code)
        {
            IsLoading = false;
            OnExecute?.Invoke(this, false);
        }

        private void Repository_NewProcessMessage(object sender, string e) => Execute.OnUIThread(() => Lines.Add(e));

        private void ConsoleService_ExecutionRequested(object sender, ProcessService process)
        {
            if (process == null)
            {
                return;
            }

            Initialize(process);
        }

        #endregion
    }
}
