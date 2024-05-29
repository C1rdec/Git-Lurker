namespace GitLurker.UI.ViewModels;

using System.Linq;
using Caliburn.Micro;
using GitLurker.Core.Models;
using Lurker.Audio;

public class ModeStatusViewModel : PropertyChangedBase
{
    private SettingsFile _settings;
    private AudioSessionService _audioSessionService;

    public ModeStatusViewModel(SettingsFile settings, AudioSessionService audioService)
    {
        _settings = settings;
        _settings.OnFileSaved += Settings_OnFileSaved;
        _audioSessionService = audioService;
    }

    public bool GitActive => _settings.Entity.Mode == Mode.Git;

    public bool GitVisible => _settings.Entity.AudioEnabled || _settings.Entity.SteamEnabled;

    public bool AudioActive => _settings.Entity.Mode == Mode.Audio;

    public bool AudioVisible => _settings.Entity.AudioEnabled && _audioSessionService.GetSessions().Any();

    public bool GameActive => _settings.Entity.Mode == Mode.Game;

    public bool GameVisible => _settings.Entity.SteamEnabled;

    private void Settings_OnFileSaved(object sender, Settings e)
    {
        NotifyModeChange();
    }

    public void NotifyModeChange()
    {
        NotifyOfPropertyChange(() => GitActive);
        NotifyOfPropertyChange(() => AudioActive);
        NotifyOfPropertyChange(() => GameActive);

        NotifyOfPropertyChange(() => GitVisible);
        NotifyOfPropertyChange(() => AudioVisible);
        NotifyOfPropertyChange(() => GameVisible);
    }
}
