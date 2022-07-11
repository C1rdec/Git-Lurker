using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Services;

namespace GitLurker.UI.ViewModels
{
    public class ConsoleViewModel : PropertyChangedBase, IDisposable
    {
        #region Fields

        private Repository _repository;
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

        public void Initialize(Repository repository)
        {
            if (_repository != null)
            {
                Lines.Clear();
                _repository.NewProcessMessage -= Repository_NewProcessMessage;
            }

            IsLoading = true;
            OnExecute?.Invoke(this, true);
            _repository = repository;
            _repository.NewProcessMessage += Repository_NewProcessMessage;
            _repository.NewExitCode += Repository_NewExitCode;
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

        private void ConsoleService_ExecutionRequested(object sender, Repository repo)
        {
            if (repo == null)
            {
                return;
            }

            Initialize(repo);
        }

        #endregion
    }
}
