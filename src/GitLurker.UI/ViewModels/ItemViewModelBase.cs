namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;

public abstract class ItemViewModelBase : ViewAware
{
    #region Fields

    private object _view;

    #endregion

    #region Properties

    public abstract string Id { get; }

    public bool IsSelected
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    protected object View => _view;

    #endregion

    #region Methods

    protected override void OnViewLoaded(object view)
    {
        _view = view;
        base.OnViewLoaded(view);
    }

    #endregion
}
