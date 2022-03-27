﻿namespace GitLurker.UI.ViewModels
{
    using Caliburn.Micro;
    using GitLurker.Models;
    using GitLurker.UI.Messages;
    using GitLurker.UI.Views;

    public class RepositoryViewModel : Screen
    {
        #region Fields

        private static readonly CloseMessage CloseMessage = new();
        private Repository _repo;
        private bool _isSelected;
        private IEventAggregator _aggregator;
        private string _branchName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryViewModel"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        public RepositoryViewModel(Repository repo)
        {
            _repo = repo;
            _aggregator = IoC.Get<IEventAggregator>();
            _repo.NewProcessMessage += Repo_NewProcessMessage;
        }

        private void Repo_NewProcessMessage(object sender, string e)
        {
            _aggregator.PublishOnUIThreadAsync(e);
        }

        #endregion

        #region Properties

        public bool HasIcon => _repo.HasIcon;

        public string IconSource => _repo.IconPath;

        public string RepoName => this._repo.Name;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public string BranchName
        {
            get => _branchName;
            set
            {
                _branchName = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void Open()
        {
            IoC.Get<IEventAggregator>().PublishOnCurrentThreadAsync(CloseMessage);
            _repo.Open();
        }

        public void Pull() => _repo.Pull();

        public void ShowBranchName()
        {
            if (string.IsNullOrEmpty(BranchName))
            {
                BranchName = _repo.GetCurrentBranchName();
            }
            else
            {
                BranchName = string.Empty;
            }
        }

        #endregion

        #region Methods

        public void Select()
        {
            IsSelected = true;

            Execute.OnUIThread(() =>
            {
                var view = GetView() as RepositoryView;
                if (view == null)
                {
                    return;
                }

                view.MainBorder.BringIntoView();
            });
        }

        #endregion
    }
}
