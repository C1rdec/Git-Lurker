using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
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

        #endregion
    }
}
