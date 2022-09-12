using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionBarViewModel : PropertyChangedBase
    {
        #region Fields

        private bool _busy;
        private Repository _repo;
        private ConsoleService _consoleService;
        private CustomActionSettingsFile _actionsFile;

        #endregion

        #region Constructors

        public ActionBarViewModel(Repository repo)
        {
            _repo = repo;
            _actionsFile = IoC.Get<CustomActionSettingsFile>();
            _consoleService = IoC.Get<ConsoleService>();

            SetActions();
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

        public void AddAction(Func<Task<ExecutionResult>> task, PackIconControlBase icon) 
            => AddAction(task, icon, openConsole: false, permanent: true);

        public void AddAction(Func<Task<ExecutionResult>> task, PackIconControlBase icon, bool openConsole)
            => AddAction(task, icon, openConsole, permanent: true);

        public void AddAction(Func<Task<ExecutionResult>> task, PackIconControlBase icon, bool openConsole, bool permanent)
        {
            // Block the ability to insert the same action
            var existingAction = Actions.FirstOrDefault(a => a.Icon.Data == icon.Data);
            if (existingAction != null)
            {
                return;
            }

            var id = Guid.NewGuid();
            Func<Task> callback = async () =>
            {
                if (Busy)
                {
                    return;
                }

                SetDisable(true, id);
                _consoleService.Listen(_repo);

                if (openConsole)
                {
                    _consoleService.Show();
                }

                var result = await task();

                _repo.AddToRecent();
                SetDisable(false, id);
            };

            Actions.Insert(0, new ActionViewModel(callback, icon, permanent, id));
        }

        private void SetActions()
        {
            Actions = new ObservableCollection<ActionViewModel>();
            Execute.OnUIThread(() => AddAction(_repo.PullAsync, new PackIconMaterial() { Kind = PackIconMaterialKind.ChevronDown }));

            foreach (var action in _actionsFile.GetActions(_repo.Folder))
            {
                if (Enum.TryParse<PackIconMaterialKind>(action.Icon, out var kind))
                {
                    var icon = new PackIconMaterial() { Kind = kind };
                    AddAction(() => _repo.ExecuteCommandAsync(action.Command, true), icon, action.OpenConsole);
                }
            }

            if (_repo.HasFrontEnd)
            {
                var icon = new PackIconSimpleIcons
                {
                    Kind = PackIconSimpleIconsKind.VisualStudioCode,
                };

                AddAction(_repo.OpenFrontEnd, icon);
            }
        }

        public void SetDisable(bool value, Guid actionId)
        {
            foreach(var action in Actions)
            {
                if (action.Id == actionId)
                {
                    continue;
                }

                action.IsDisable = value;
            }

            var activeAction = Actions.FirstOrDefault(a => a.Id == actionId);
            if (activeAction != null)
            {
                activeAction.IsActive = value;
            }
        }

        #endregion
    }
}
