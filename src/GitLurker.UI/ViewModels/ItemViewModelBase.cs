namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;

public abstract class ItemViewModelBase : ViewAware
{
    #region Fields

    private object _view;
    private bool _isSelected;

    #endregion

    #region Properties

    public abstract string Id { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
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
