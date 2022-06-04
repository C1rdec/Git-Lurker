﻿namespace GitLurker.UI.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.Services;

    public class WorkspaceViewModel: PropertyChangedBase
    {
        #region Fields

        private KeyboardService _keyboardService;
        private IEnumerable<Workspace> _workspaces;
        private ObservableCollection<RepositoryViewModel> _repos;
        private RepositoryViewModel _selectedRepo;
        private string _lastSearchTerm;
        private bool _mouseOver;

        #endregion

        #region Constructors

        public WorkspaceViewModel(IEnumerable<Workspace> workspaces, KeyboardService keyboardService)
        {
            _workspaces = workspaces;
            _repos = new ObservableCollection<RepositoryViewModel>();
            _keyboardService = keyboardService;
            _keyboardService.DownPressed += KeyboardService_DownPressed;
            _keyboardService.UpPressed += KeyboardService_UpPressed;
        }

        public WorkspaceViewModel(KeyboardService keyboardService)
            : this(Enumerable.Empty<Workspace>(), keyboardService)
        {
        }

        #endregion

        #region Properties

        public ObservableCollection<RepositoryViewModel> Repos => _repos;

        public RepositoryViewModel SelectedRepo
        {
            get
            {
                return _selectedRepo;
            }

            private set
            {
                _selectedRepo = value;
                NotifyOfPropertyChange(() => HasSelectedRepo);
            }
        }

        public bool HasSelectedRepo => SelectedRepo != null || !string.IsNullOrEmpty(_lastSearchTerm) || _mouseOver;

        #endregion

        #region Methods

        public void OnMouseEnter()
        {
            _mouseOver = true;
            NotifyOfPropertyChange(() => HasSelectedRepo);
        }

        public void OnMouseLeave()
        {
            _mouseOver = false;
            NotifyOfPropertyChange(() => HasSelectedRepo);
        }

        public void Search(string term)
        {
            _lastSearchTerm = term;
            Clear();

            if (string.IsNullOrEmpty(term))
            {
                return;
            }

            var allRepos = new List<Repository>();
            foreach (var workspace in _workspaces)
            {
                allRepos.AddRange(workspace.Repositories);
            }

            var startWith = allRepos.Where(r => r.Name.ToUpper().StartsWith(term.ToUpper()));
            var contain = allRepos.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
            contain.InsertRange(0, startWith);
            foreach (var repo in contain.Distinct().Take(5))
            {
                Repos.Add(new RepositoryViewModel(repo));
            }
        }

        public void Clear()
        {
            SelectedRepo = null;
            _repos.Clear();
        }

        public void ShowRecent()
        {
            var file = new SettingsFile();
            file.Initialize();

            foreach (var folder in file.Entity.RecentRepos)
            {
                var repo = GetRepo(folder);
                if (repo != null)
                {
                    Repos.Add(new RepositoryViewModel(repo));
                }
            }
        }

        public void Open()
        {
            if (SelectedRepo != null)
            {
                SelectedRepo.Open();
                return;
            }

            _repos.FirstOrDefault()?.Open();
        }

        public void Refresh(IEnumerable<Workspace> workspaces)
        {
            _workspaces = workspaces;
            _repos.Clear();
        }

        private void KeyboardService_DownPressed(object sender, System.EventArgs e)
        {
            if (SelectedRepo == null)
            {
                SelectedRepo = _repos.FirstOrDefault();
                SelectedRepo?.Select();
                return;
            }

            var index = _repos.IndexOf(_selectedRepo);
            if (index == -1 || (index + 1) >= _repos.Count)
            {
                return;
            }

            index++;
            SelectedRepo.IsSelected = false;
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        private void KeyboardService_UpPressed(object sender, System.EventArgs e)
        {
            if (SelectedRepo == null)
            {
                return;
            }

            var index = _repos.IndexOf(SelectedRepo);
            if (index <= 0)
            {
                return;
            }

            index--;
            SelectedRepo.IsSelected = false;
            SelectedRepo = Repos.ElementAt(index);
            SelectedRepo.Select();
        }

        private Repository GetRepo(string folderPath)
        {
            foreach(var workspace in _workspaces)
            {
                var repo = workspace.GetRepo(folderPath);
                if (repo != null)
                {
                    return repo;
                }
            }

            return null;
        }

        #endregion
    }
}
