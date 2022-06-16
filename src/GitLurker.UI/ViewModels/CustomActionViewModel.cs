using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class CustomActionViewModel : PropertyChangedBase
    {
        #region Fields

        private static readonly PackIconMaterialKind[] IconKinds = Enum.GetValues(typeof(PackIconMaterialKind)).Cast<PackIconMaterialKind>().ToArray();
        private PackIconMaterialKind _selectedIcon;
        private CustomAction _action;
        private bool _modified;
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
                _selectedIcon = PackIconMaterialKind.Cog;
            }
            else
            {

                _selectedIcon = Enum.Parse<PackIconMaterialKind>(action.Icon);
            }

            PropertyChanged += CustomActionViewModel_PropertyChanged;
            Icons = new ObservableCollection<PackIconMaterialKind>(IconKinds);

            var repos = _repositoryService.GetAllRepo().Select(r => r.Folder);
            Repositories = new ObservableCollection<string>(repos);
        }

        #endregion

        #region Properties

        public ObservableCollection<PackIconMaterialKind> Icons { get; set; }

        public ObservableCollection<string> Repositories { get; set; }

        public string SelectedRepository
        {
            get => _action.RepositoryPath;
            set
            {
                _action.RepositoryPath = value;
                NotifyOfPropertyChange();
            }
        }

        public PackIconMaterialKind SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;
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
            get => _modified;
            set
            {
                _modified = value;
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

        #endregion

        #region Methods

        public void Save()
        {
            Modified = false;

            _action.Icon = _selectedIcon.ToString();
            var file = IoC.Get<CustomActionSettingsFile>();

            IoC.Get<FlyoutService>().Close();

            if (_isNew)
            {
                _isNew = false;
                file.AddAction(_action);
            }
            else
            {
                file.Save();
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
}
