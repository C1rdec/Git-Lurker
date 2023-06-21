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

    public string PatreonId => _patronService.PatreonId;

    public bool IsPledged => _patronService.IsPledged;

    public bool IsNotPledged => !IsPledged;

    public bool IsNotLoggedIn => string.IsNullOrEmpty(PatreonId);

    #endregion

    #region Methods

    public async void Login()
    {
        await _patronService.LoginAsync();

        NotifyOfPropertyChange(() => PatreonId);
        NotifyOfPropertyChange(() => IsPledged);
        NotifyOfPropertyChange(() => IsNotPledged);
        NotifyOfPropertyChange(() => IsNotLoggedIn);

        await _eventAggregator.PublishOnCurrentThreadAsync(new PatronMessage());
    }

    public void Pledge()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/poelurker",
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    #endregion
}
