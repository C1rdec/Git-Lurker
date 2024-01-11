namespace GitLurker.UI.ViewModels;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.UI.Messages;
using GitLurker.UI.Services;
using Lurker.Patreon;

public class PatreonViewModel : FlyoutScreenBase, IHandle<PatronMessage>
{
    #region Fields

    private PatreonService _patronService;
    private IEventAggregator _eventAggregator;
    private PatreonSettingsViewModel _settings;

    #endregion

    public PatreonViewModel(PatreonService service, IEventAggregator eventAggregator, PatreonSettingsViewModel settings, FlyoutService flyoutService)
        : base(flyoutService)
    {
        _patronService = service;
        _eventAggregator = eventAggregator;
        _settings = settings;

        _eventAggregator.SubscribeOnPublishedThread(this);
    }

    #region Properties

    public bool NeedJoin => IsLoggedIn && !IsPledged;

    public string PatreonId => _patronService.PatreonId;

    public bool IsPledged => _patronService.IsPledged;

    public bool IsNotPledged => !IsPledged;

    public bool IsNotLoggedIn => string.IsNullOrEmpty(PatreonId);

    public bool IsLoggedIn => !IsNotLoggedIn;

    public PatreonSettingsViewModel Settings => _settings;

    #endregion

    #region Methods

    public Task HandleAsync(PatronMessage message, CancellationToken cancellationToken)
    {
        Notify();

        return Task.CompletedTask;
    }

    public async void Login()
    {
        await _patronService.LoginAsync();
        await _patronService.CheckPledgeStatusAsync("3779584");

        Notify();

        await _eventAggregator.PublishOnCurrentThreadAsync(new PatronMessage());
    }

    public async void Pledge()
    {
        if (await _patronService.CheckPledgeStatusAsync("3779584"))
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
