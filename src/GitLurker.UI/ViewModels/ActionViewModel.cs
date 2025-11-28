namespace GitLurker.UI.ViewModels;

using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;

public class ActionViewModel : PropertyChangedBase
{
    #region Fields

    private Guid _id;
    private Func<Task> _action;
    private Func<Task> _holdAction;
    private PackIconControlBase _icon;
    private bool _permanent;
    private DebounceService _debounceService;
    private CancellationTokenSource _tokenSource;

    #endregion

    #region Constructors

    public ActionViewModel(Func<Task> action, Func<Task> holdAction, PackIconControlBase icon, bool permanent, Guid id)
    {
        _action = action;
        _holdAction = holdAction;

        icon.Height = 30;
        icon.Width = 30;
        _icon = icon;
        _permanent = permanent;
        _id = id;
        _debounceService = new DebounceService();
    }

    #endregion

    #region Properties

    public Guid Id => _id;

    public PackIconControlBase Icon => _icon;

    public bool Permanent => _permanent;

    public bool IsEnable => !IsDisable;

    public bool IsDisable
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(() => IsEnable);
        }
    }

    public bool IsActive
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    public int HoldProgress
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region Methods

    public async void OnMouseUp()
    {
        CancelHold();

        var progress = HoldProgress;
        HoldProgress = 0;

        var action = progress == 100 ? _holdAction : _action;
        await action();
    }

    public void OnMouseDown()
    {
        if (_holdAction == null)
        {
            return;
        }

        _tokenSource = new CancellationTokenSource();
        _debounceService.Debounce(300, () => Execute.OnUIThread(async () =>
        {
            while (HoldProgress < 100 && _tokenSource != null)
            {
                HoldProgress+=2;
                await Task.Delay(20);
            }
        }));
    }

    public void OnMouseLeave()
    {
        CancelHold();
        HoldProgress = 0;
    }

    private void CancelHold()
    {
        if (_tokenSource != null)
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }
    }

    #endregion
}
