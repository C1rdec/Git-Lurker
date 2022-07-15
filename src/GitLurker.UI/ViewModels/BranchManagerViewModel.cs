using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class BranchManagerViewModel : PropertyChangedBase
    {
        #region Fields

        private Repository _repo;
        private string _selectedBranch;
        private Action<string> _onSelected;
        private bool _isLoading;
        private bool _isCreateBranch;
        private string _newBranchName;

        #endregion

        #region Constructors

        public BranchManagerViewModel(Repository repo, Action<string> onSelected)
        {
            _repo = repo;
            _onSelected = onSelected;
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

        public async void Select()
        {
            if (string.IsNullOrEmpty(SelectedBranchName))
            {
                return;
            }

            IsLoading = true;
            var result = await _repo.ExecuteCommandAsync($"git checkout {SelectedBranchName}");
            _repo.AddToRecent();

            _onSelected(SelectedBranchName);
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
            var branches = _repo.GetBranchNames();

            Execute.OnUIThread(() =>
            {
                BranchNames.Clear();

                foreach (var branch in branches)
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

        public void ShowCreateBranch()
        {
            IsCreateBranch = true;
        }

        public async void CreateBranch()
        {
            _repo.AddToRecent();
            await _repo.ExecuteCommandAsync($"git checkout -b {NewBranchName}");
            _onSelected(NewBranchName);
            NewBranchName = string.Empty;
        }

        #endregion
    }
}
