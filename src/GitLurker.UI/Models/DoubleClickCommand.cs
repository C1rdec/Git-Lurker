namespace GitLurker.UI.ViewModels;

using System;
using System.Windows.Input;

public class DoubleClickCommand : ICommand
{
    #region Fields

    private Action<object> _action;

    #endregion

    #region Constructors

    public DoubleClickCommand(Action<object> action)
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
        return true;
    }

    public void Execute(object parameter)
    {
        _action?.Invoke(parameter);
    }

    #endregion
}