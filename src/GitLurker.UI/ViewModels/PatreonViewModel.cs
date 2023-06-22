using System.Diagnostics;
using Caliburn.Micro;
using GitLurker.Core.Services;
using GitLurker.UI.Messages;

namespace GitLurker.UI.ViewModels;

public class PatreonViewModel : Screen
{
    #region Fields

    private PatronService _patronService;
    private IEventAggregator _eventAggregator;

    #endregion

    public PatreonViewModel(PatronService service, IEventAggregator eventAggregator)
    {
        _patronService = service;
        _eventAggregator = eventAggregator;
    }

    #region Properties

    public bool NeedJoin => IsLoggedIn && !IsPledged;

    public string PatreonId => _patronService.PatreonId;

    public bool IsPledged => _patronService.IsPledged;

    public bool IsNotPledged => !IsPledged;

    public bool IsNotLoggedIn => string.IsNullOrEmpty(PatreonId);

    public bool IsLoggedIn => !IsNotLoggedIn;

    #endregion

    #region Methods

    public async void Login()
    {
        await _patronService.LoginAsync();

        Notify();

        await _eventAggregator.PublishOnCurrentThreadAsync(new PatronMessage());
    }

    public async void Pledge()
    {
        if (await _patronService.CheckPledgeStatusAsync())
        {
            Notify();

            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/poelurker",
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    private void Notify()
    {
        NotifyOfPropertyChange(() => NeedJoin);
        NotifyOfPropertyChange(() => PatreonId);
        NotifyOfPropertyChange(() => IsPledged);
        NotifyOfPropertyChange(() => IsNotPledged);
        NotifyOfPropertyChange(() => IsNotLoggedIn);
        NotifyOfPropertyChange(() => IsLoggedIn);
    }

    #endregion
}
