namespace GitLurker.UI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.UI.Services;
using Lurker.Audio;

public class AudioLibraryViewModel : ItemListViewModelBase<AudioSessionViewModel>, IItemListViewModel
{
    #region Fields

    private MouseService _mouseService;
    private AudioSessionService _audioSessionService;
    private ObservableCollection<AudioSessionViewModel> _audioSessionViewModels;

    #endregion

    #region Constructors

    public AudioLibraryViewModel(MouseService mouseService)
    {
        _mouseService = mouseService;
        _audioSessionService = new AudioSessionService();
        _audioSessionViewModels = [];
    }


    #endregion

    #region Properties

    public override ObservableCollection<AudioSessionViewModel> ItemViewModels => _audioSessionViewModels;

    #endregion

    #region Methods

    public bool Close()
    {
        Clear();
        return true;
    }

    public void EnterLongPressed()
    {
    }

    public void NextTabPressed()
    {
    }

    public Task<bool> Open(bool skipModifier)
    {
        return Task.FromResult(false);
    }

    public Task RefreshItems()
    {
        Clear();
        foreach (var session in _audioSessionService.GetSessions())
        {
            Execute.OnUIThread(() => _audioSessionViewModels.Add(new AudioSessionViewModel(session, _mouseService)));
        }

        return Task.CompletedTask;
    }

    public void Search(string term)
    {
        Clear();

        if (string.IsNullOrEmpty(term))
        {
            ShowRecent();
            return;
        }

        var sessions = _audioSessionService.GetSessions();
        var startWith = sessions.Where(s => s.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase));
        var contain = sessions.Where(r => r.Name.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        contain.InsertRange(0, startWith);

        var matches = contain.Distinct().Take(5);

        foreach (var session in matches)
        {
            ItemViewModels.Add(new AudioSessionViewModel(session, _mouseService));
        }
    }

    public void ShowRecent()
        => RefreshItems();

    #endregion
}
