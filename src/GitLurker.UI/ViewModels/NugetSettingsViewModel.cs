namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;
using GitLurker.Core.Models;

public class NugetSettingsViewModel : PropertyChangedBase
{
    #region Fields

    private SettingsFile _settingsFile;

    #endregion

    #region Constructors

    public NugetSettingsViewModel(SettingsFile settingsFile)
    {
        _settingsFile = settingsFile;
        NugetApiKey = _settingsFile.Entity.NugetApiKey;
        PropertyChanged += NugetSettingsViewModel_PropertyChanged;
    }

    #endregion

    #region Properties

    public bool Modified
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    public string NugetApiKey
    {
        get => _settingsFile.Entity.NugetApiKey;
        set
        {
            _settingsFile.Entity.NugetApiKey = value;
            NotifyOfPropertyChange();
        }
    }

    public string LocalSource
    {
        get => _settingsFile.Entity.LocalNugetSource;
        set
        {
            _settingsFile.Entity.LocalNugetSource = value;
            NotifyOfPropertyChange();
        }
    }

    public string RemoteSource
    {
        get => _settingsFile.Entity.RemoteNugetSource;
        set
        {
            _settingsFile.Entity.RemoteNugetSource = value;
            NotifyOfPropertyChange();
        }
    }

    public bool HasNugetSource => _settingsFile.HasNugetSource();

    #endregion

    #region Methods

    public void ToggleLocalNuget()
    {
        string path = null;
        if (!HasNugetSource)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            path = dialog.SelectedPath;
        }

        LocalSource = path;
        NotifyOfPropertyChange(() => HasNugetSource);
        _settingsFile.Save();
    }

    public void Save()
    {
        Modified = false;
        _settingsFile.Save();
    }

    private void NugetSettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Modified) || e.PropertyName == nameof(LocalSource))
        {
            return;
        }

        Modified = true;
    }

    #endregion
}
