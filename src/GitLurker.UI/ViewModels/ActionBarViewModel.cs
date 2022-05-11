using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Models;
using MahApps.Metro.IconPacks;

namespace GitLurker.UI.ViewModels
{
    public class ActionBarViewModel : PropertyChangedBase
    {
        #region Fields

        private bool _busy;

        #endregion

        #region Constructors

        public ActionBarViewModel(Repository repo)
        {
            Actions = new ObservableCollection<ActionViewModel>();

            if (repo.HasFrontEnd)
            {
                AddAction(repo.OpenFrontEnd, new PackIconSimpleIcons() { Kind = PackIconSimpleIconsKind.VisualStudioCode });
            }

            AddAction(repo.PullAsync, new PackIconMaterial() { Kind = PackIconMaterialKind.ChevronDown });
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

        private void AddAction(Func<Task> task, PackIconControlBase icon)
        {
            var callback = async () =>
            {
                if (Busy)
                {
                    return;
                }

                Busy = true;
                await task();
                Busy = false;
            };

            Actions.Add(new ActionViewModel(callback, icon));
        }

        #endregion
    }
}
