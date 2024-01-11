namespace GitLurker.UI.ViewModels;

using Caliburn.Micro;
using GitLurker.Core.Models;

public class NugetSettingsViewModel : PropertyChangedBase
{
    #region Fields

    private SettingsFile _settingsFile;
    private bool _modified;

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
        get => _modified;
        set
        {
            _modified = value;
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

    public string NugetSource
    {
        get => _settingsFile.Entity.NugetSource;
        set
        {
            _settingsFile.Entity.NugetSource = value;
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

        NugetSource = path;
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
        if (e.PropertyName == nameof(Modified) || e.PropertyName == nameof(NugetSource))
        {
            return;
        }

        Modified = true;
    }

    #endregion
}
