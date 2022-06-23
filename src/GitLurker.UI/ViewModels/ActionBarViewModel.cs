using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Messages;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionBarViewModel : PropertyChangedBase
    {
        #region Fields

        private bool _busy;
        private Repository _repo;

        #endregion

        #region Constructors

        public ActionBarViewModel(Repository repo)
        {
            _repo = repo;
            Actions = new ObservableCollection<ActionViewModel>();
            AddAction(repo.PullAsync, new PackIconMaterial() { Kind = PackIconMaterialKind.ChevronDown });

            if (repo.HasFrontEnd)
            {
                AddAction(repo.OpenFrontEnd, new PackIconSimpleIcons() { Kind = PackIconSimpleIconsKind.VisualStudioCode });
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<ActionViewModel> Actions { get; set; }

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

        public void RemoveActions()
        {
            Execute.OnUIThread(() =>
            {
                var notPermanentActions = Actions.Where(a => !a.Permanent).ToArray();
                foreach (var action in notPermanentActions)
                {
                    Actions.Remove(action);
                }
            });
        }

        public void AddAction(Func<Task> task, PackIconControlBase icon) => AddAction(task, icon, false);

        public void AddAction(Func<Task> task, PackIconControlBase icon, bool openConsole)
        {
            Func<Task> callback = async () =>
            {
                if (Busy)
                {
                    return;
                }

                Busy = true;
                _ = IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new ConsoleMessage()
                {
                    Repository = _repo,
                    OpenConsole = openConsole,
                });

                await task();
                Busy = false;
            };

            Actions.Insert(0, new ActionViewModel(callback, icon));
        }

        #endregion
    }
}
