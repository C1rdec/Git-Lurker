namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Services;
using NHotkey.Wpf;

public class SnippetTileViewModel : PropertyChangedBase
{
    #region Fields

    private static readonly SettingsFile SettingsFile = IoC.Get<SettingsFile>();
    private static readonly FlyoutService CurrentFlyoutService = IoC.Get<FlyoutService>();
    private Snippet _snippet;

    #endregion

    #region Constructors

    public SnippetTileViewModel(Snippet snippet)
    {
        _snippet = snippet;
    }

    #endregion

    #region Properties

    public string SnippetName => _snippet.Name;

    #endregion

    #region Methods

    public void Open()
    {
        CurrentFlyoutService.Show(_snippet.Name, new SnippetViewModel(_snippet), MahApps.Metro.Controls.Position.Right);
    }

    public void Delete()
    {
        SettingsFile.RemoveSnippet(_snippet);

        HotkeyManager.Current.Remove(_snippet.Id.ToString());

        // To notify the parent
        NotifyOfPropertyChange("Deleted");
    }

    #endregion
}
