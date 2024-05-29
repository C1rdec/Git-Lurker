namespace GitLurker.UI.Services;

using System.Linq;
using GitLurker.Core.Models;
using Lurker.Audio;

public class ModeService
{
    private SettingsFile _settingsFile;
    private AudioSessionService _audioSessionService;

    public ModeService(SettingsFile file, AudioSessionService audioService)
    {
        _settingsFile = file;
        _audioSessionService = audioService;
    }

    public Mode GetNextMode()
    {
        var mode = _settingsFile.GetNextMode();
        if (mode == Mode.Audio && !_audioSessionService.GetSessions().Any())
        {
            // We need to set the Mode to Audio to get the next one
            _settingsFile.Entity.Mode = Mode.Audio;
            mode = _settingsFile.GetNextMode();
        }

        return mode;
    }
}
