namespace GitLurker.UI.ViewModels;

using System.Linq;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Services;

public class SnippetViewModel : PropertyChangedBase
{
    #region Fields

    private Snippet _snippet;

    #endregion

    #region Constructors

    public SnippetViewModel(Snippet snippet)
    {
        _snippet = snippet;
        Hotkey = new HotkeyViewModel(snippet.Hotkey, (k, m) => Modified = true, "Hotkey");

        PropertyChanged += SnippetViewModel_PropertyChanged;
    }

    #endregion

    #region Properties

    public string SnippetName
    {
        get => _snippet.Name;
        set
        {
            _snippet.Name = value;
            NotifyOfPropertyChange();
        }
    }

    public string Value
    {
        get => _snippet.Value;
        set
        {
            _snippet.Value = value;
            NotifyOfPropertyChange();
        }
    }

    public bool Modified
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    public HotkeyViewModel Hotkey { get; init; }

    #endregion

    #region Methods

    public void Save()
    {
        Modified = false;

        var file = IoC.Get<SettingsFile>();

        var existingSnippet = file.Entity.Snippets.FirstOrDefault(s => s.Id == _snippet.Id);
        if (existingSnippet == null)
        {
            file.AddSnippet(_snippet);
        }
        else
        {
            existingSnippet.Value = _snippet.Value;
            existingSnippet.Name = _snippet.Name;
            existingSnippet.Hotkey = _snippet.Hotkey;
        }

        file.Save();

        IoC.Get<FlyoutService>().Close();
    }

    private void SnippetViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Modified))
        {
            return;
        }

        Modified = true;
    }

    #endregion
}
