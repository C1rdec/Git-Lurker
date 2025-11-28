namespace GitLurker.UI.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.Core.Services;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;

public class CustomActionViewModel : PropertyChangedBase
{
    #region Fields

    private static readonly PackIconMaterialKind[] IconKinds = Enum.GetValues(typeof(PackIconMaterialKind)).Cast<PackIconMaterialKind>().ToArray();
    private CustomAction _action;
    private RepositoryService _repositoryService;
    private bool _isNew;

    #endregion

    #region Constructors

    public CustomActionViewModel(CustomAction action)
        : this(action, false)
    {

    }

    public CustomActionViewModel(CustomAction action, bool isNew)
    {
        _action = action;
        _isNew = isNew;
        _repositoryService = IoC.Get<RepositoryService>();
        if (string.IsNullOrEmpty(action.Icon))
        {
            SelectedIcon = PackIconMaterialKind.Cog;
        }
        else
        {
            SelectedIcon = Enum.Parse<PackIconMaterialKind>(action.Icon);
        }

        PropertyChanged += CustomActionViewModel_PropertyChanged;
        Icons = new ObservableCollection<PackIconMaterialKind>(IconKinds);

        var repos = _repositoryService.GetAllRepo().OrderBy(r => r.Name);
        Repositories = new ObservableCollection<Repository>(repos);

        var selectedRepos = repos.Where(r => _action.Repositories.Contains(r.Folder));
        SelectedRepositories = new ObservableCollection<Repository>(selectedRepos);
    }

    #endregion

    #region Properties

    public ObservableCollection<Repository> SelectedRepositories { get; set; }

    public ObservableCollection<PackIconMaterialKind> Icons { get; set; }

    public ObservableCollection<Repository> Repositories { get; set; }

    public Repository SelectedRepository
    {
        get => field;
        set
        {
            field = value;
            AddRepository(value);
            NotifyOfPropertyChange();
        }
    }

    public PackIconMaterialKind SelectedIcon
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange();
        }
    }

    public string ActionName
    {
        get => _action.Name;
        set
        {
            _action.Name = value;
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

    public string Command
    {
        get => _action.Command;
        set
        {
            _action.Command = value;
            NotifyOfPropertyChange();
        }
    }

    public bool OpenConsole
    {
        get => _action.OpenConsole;
        set
        {
            _action.OpenConsole = value;
            NotifyOfPropertyChange();
        }
    }

    public bool ExcludeRepositories
    {
        get => _action.ExcludeRepositories;
        set
        {
            _action.ExcludeRepositories = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region Methods

    public void Save()
    {
        Modified = false;

        _action.Icon = SelectedIcon.ToString();
        var file = IoC.Get<CustomActionSettingsFile>();

        if (_isNew)
        {
            _isNew = false;
            file.AddAction(_action);
        }
        else
        {
            file.Save();
        }

        IoC.Get<FlyoutService>().Close();
    }

    public void ToggleExcludeRepositories() => ExcludeRepositories = !ExcludeRepositories;

    public void ToggleConsole() => OpenConsole = !OpenConsole;

    public void AddRepository(Repository repo)
    {
        if (_action.AddRepository(repo))
        {
            SelectedRepositories.Add(repo);
        }
    }

    public void RemoveRepository(Repository repo)
    {
        if (_action.RemoveRepository(repo))
        {
            Modified = true;
            SelectedRepositories.Remove(repo);
        }
    }

    private void CustomActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Modified))
        {
            return;
        }

        Modified = true;
    }

    #endregion
}
