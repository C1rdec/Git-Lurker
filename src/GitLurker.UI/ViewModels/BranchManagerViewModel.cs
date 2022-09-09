using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Views;

namespace GitLurker.UI.ViewModels
{
    public class BranchManagerViewModel : ViewAware
    {
        #region Fields

        private static readonly string Origin = "origin/";
        private BranchManagerView _view;
        private Repository _repo;
        private string _selectedBranch;
        private Action<string> _onSelected;
        private System. Action _onClose;
        private bool _isLoading;
        private bool _isCreateBranch;
        private string _newBranchName;
        private string _searchTerm;
        private List<string> _branchNames;

        #endregion

        #region Constructors

        public BranchManagerViewModel(Repository repo, Action<string> onSelected, System.Action onClose)
        {
            _repo = repo;
            _onSelected = onSelected;
            _onClose = onClose;
            BranchNames = new ObservableCollection<string>();
        }

        #endregion

        #region Properties

        public ObservableCollection<string> BranchNames { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange();
            }
        }

        public string NewBranchName
        {
            get => _newBranchName;
            set
            {
                _newBranchName = value;
                NotifyOfPropertyChange();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                Search(value);
                NotifyOfPropertyChange();
            }
        }

        public bool IsCreateBranch
        {
            get => _isCreateBranch;
            set
            {
                _isCreateBranch = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsNotCreateBranch);
            }
        }

        public bool IsNotCreateBranch => !IsCreateBranch;

        public string SelectedBranchName
        {
            get => _selectedBranch;
            set
            {
                _selectedBranch = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        protected override void OnViewLoaded(object view)
        {
            _view = view as BranchManagerView;
            base.OnViewLoaded(view);
        }

        public async void Select()
        {
            if (string.IsNullOrEmpty(SelectedBranchName))
            {
                return;
            }

            SearchTerm = string.Empty;
            IsLoading = true;
            var branchName = SelectedBranchName;
            if (branchName.StartsWith(Origin))
            {
                branchName = branchName.Replace(Origin, string.Empty);
            }

            var result = await _repo.ExecuteCommandAsync($"git checkout {branchName}");
            _repo.AddToRecent();

            _onSelected(branchName);
            IsLoading = false;
        }

        public void SelectPreviousBranch()
        {
            if (string.IsNullOrEmpty(SelectedBranchName))
            {
                SelectedBranchName = BranchNames.FirstOrDefault();
                return;
            }

            var index = BranchNames.IndexOf(SelectedBranchName);
            if (index == -1)
            {
                return;
            }

            if ((index - 1) < 0)
            {
                SelectedBranchName = BranchNames.LastOrDefault();
                return;
            }

            index--;
            SelectedBranchName = BranchNames.ElementAt(index);
        }

        public void SelectNextBranch()
        {
            if (string.IsNullOrEmpty(SelectedBranchName))
            {
                SelectedBranchName = BranchNames.FirstOrDefault();
                return;
            }

            var index = BranchNames.IndexOf(SelectedBranchName);
            if (index == -1)
            {
                return;
            }

            if ((index + 1) >= BranchNames.Count)
            {
                SelectedBranchName = BranchNames.FirstOrDefault();
                return;
            }

            index++;
            SelectedBranchName = BranchNames.ElementAt(index);
        }

        public void ShowBranches()
        {
            IsCreateBranch = false;
            NewBranchName = string.Empty;

            _branchNames = _repo.GetBranchNames().ToList();
            SearchTerm = string.Empty;

            Execute.OnUIThread(() =>
            {
                BranchNames.Clear();

                foreach (var branch in _branchNames)
                {
                    BranchNames.Add(branch);
                }

                SelectedBranchName = BranchNames.FirstOrDefault();
                _view.SearchTerm.Focus();
            });
        }

        public void Search(string term)
        {
            Execute.OnUIThread(() =>
            {
                BranchNames.Clear();
                foreach (var branch in _branchNames.Where(b => b.Contains(term)))
                {
                    BranchNames.Add(branch);
                }

                SelectedBranchName = BranchNames.FirstOrDefault();
            });
        }

        public async void CleanBranches()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;
            await _repo.CleanBranches();

            ShowBranches();
            IsLoading = false;
        }

        public void Close()
        {
            _onClose?.Invoke();
        }

        public void ShowCreateBranch()
        {
            IsCreateBranch = true;
            Execute.OnUIThread(() => _view.NewBranchName.Focus());
        }

        public void CreateBranch2() => CreateBranch();

        public async void CreateBranch()
        {
            var branchName = NewBranchName.Trim();
            if (string.IsNullOrEmpty(branchName))
            {
                return;
            }

            _repo.AddToRecent();
            await _repo.ExecuteCommandAsync($"git checkout -b {branchName}");
            _onSelected(branchName);
            NewBranchName = string.Empty;
        }

        #endregion
    }
}
