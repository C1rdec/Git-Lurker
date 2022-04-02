using System;
using System.Windows.Input;

namespace GitLurker.UI.ViewModels
{
    public class DoubleClickCommand : ICommand
    {
        #region Fields

        private Action _action;

        #endregion

        #region Constructors

        public DoubleClickCommand(Action action)
        {
            _action = action;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        #endregion
    }
}