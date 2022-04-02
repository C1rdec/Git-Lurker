namespace GitLurker.UI.ViewModels
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
        private bool _busy;

        #endregion

        #region Constructors

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

        public string RepoName => _repo.Name;

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

        public bool Busy
        {
            get => _busy;
            set
            {
                _busy = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => NotBusy);
            }
        }

        public bool NotBusy => !Busy;

        #endregion

        #region Methods

        public void Open()
        {
            IoC.Get<IEventAggregator>().PublishOnCurrentThreadAsync(CloseMessage);
            _repo.Open();
        }

        public async void Pull() 
        {
            if (Busy)
            {
                return;
            }

            Busy = true;
            await _repo.PullAsync();
            Busy = false;
        }

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
