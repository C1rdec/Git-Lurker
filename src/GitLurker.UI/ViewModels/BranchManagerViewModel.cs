using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Caliburn.Micro;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class BranchManagerViewModel : PropertyChangedBase
    {
        #region Fields

        private Repository _repo;
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

        #endregion

        #region Methods

        public async void OnSelectionChanged(SelectionChangedEventArgs selection)
        {
            if (selection.AddedItems.Count <= 0)
            {
                return;
            }

            IsLoading = true;
            var selectedBranch = selection.AddedItems[0] as string;
            var result = await _repo.ExecuteCommandAsync($"git checkout {selectedBranch}");

            _onSelected(selectedBranch);
            IsLoading = false;
        }

        public void ShowBranches()
        {
            BranchNames.Clear();
            var branches = _repo.GetBranchNames();

            foreach (var branch in branches)
            {
                BranchNames.Add(branch);
            }
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
