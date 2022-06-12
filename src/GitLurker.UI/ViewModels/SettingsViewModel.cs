using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;
using GitLurker.UI.Models;
using GitLurker.UI.Services;
using WindowsUtilities;

namespace GitLurker.UI.ViewModels
{
    public class SettingsViewModel : Screen
    {
        #region Fields

        private System.Action _onSave;
        private SettingsFile _settingsFile;
        private WindowsLink _windowsStartupService;
        private FlyoutService _flyoutService;
        private bool _flyoutOpen;
        private string _flyoutHeader;
        private PropertyChangedBase _flyoutContent;
        private int _selectedTabIndex;

        #endregion

        #region Constructors

        public SettingsViewModel(System.Action onSave)
        {
            _onSave = onSave;
            _flyoutService = IoC.Get<FlyoutService>();
            _settingsFile = IoC.Get<SettingsFile>();
            _settingsFile.Initialize();

            RepoManager = new RepoManagerViewModel(_settingsFile);
            Hotkey = new HotkeyViewModel(_settingsFile.Entity.HotKey, Save);
            ActionManager = new CustomActionManagerViewModel();
            _windowsStartupService = IoC.Get<WindowsLink>();
            IoC.Get<DialogService>().Register(this);

            _flyoutService.ShowFlyoutRequested += FlyoutService_ShowFlyout;
            _flyoutService.CloseFlyoutRequested += FlyoutService_CloseFlyout;
        }

        #endregion

        #region Properties

        public RepoManagerViewModel RepoManager { get; set; }

        public HotkeyViewModel Hotkey { get; set; }

        public CustomActionManagerViewModel ActionManager { get; set; }

        public bool HasNugetSource => _settingsFile.HasNugetSource();

        public string FlyoutHeader
        {
            get
            {
                return _flyoutHeader;
            }

            set
            {
                _flyoutHeader = value;
                NotifyOfPropertyChange(() => FlyoutHeader);
            }
        }

        public bool FlyoutOpen
        {
            get
            {
                return _flyoutOpen;
            }

            set
            {
                if (!value)
                {
                    _flyoutService.NotifyFlyoutClosed();
                }

                _flyoutOpen = value;
                NotifyOfPropertyChange(() => FlyoutOpen);
            }
        }

        public PropertyChangedBase FlyoutContent
        {
            get
            {
                return _flyoutContent;
            }

            set
            {
                _flyoutContent = value;
                NotifyOfPropertyChange(() => FlyoutContent);
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                NotifyOfPropertyChange();
            }
        }

        public bool StartWithWindows
        {
            get => _settingsFile.Entity.StartWithWindows;
            set
            {
                _settingsFile.Entity.StartWithWindows = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AddToStartMenu
        {
            get => _settingsFile.Entity.AddToStartMenu;
            set
            {
                _settingsFile.Entity.AddToStartMenu = value;
                NotifyOfPropertyChange();
            }
        }

        public bool DoubleTabEnabled
        {
            get => _settingsFile.Entity.DoubleTabEnabled;
            set
            {
                _settingsFile.Entity.DoubleTabEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        public void ShowFlyout(string header, PropertyChangedBase content)
        {
            FlyoutHeader = header;
            FlyoutContent = content;
            FlyoutOpen = true;
        }

        public void CloseFlyout()
        {
            FlyoutOpen = false;

            // We use the field since we dont want to notify the UI
            _flyoutContent = null;
        }

        public void Save()
        {
            _settingsFile.Save();
            _onSave?.Invoke();
        }

        public void ToggleAddMenu()
        {
            AddToStartMenu = !AddToStartMenu;
            if (AddToStartMenu)
            {
                _windowsStartupService.AddStartMenu();
            }
            else
            {
                _windowsStartupService.RemoveStartMenu();
            }

            Save();
        }

        public void ToggleStartWithWindows()
        {
            StartWithWindows = !StartWithWindows;
            if (StartWithWindows)
            {
                _windowsStartupService.AddStartWithWindows();
            }
            else
            {
                _windowsStartupService.RemoveStartWithWindows();
            }

            Save();
        }

        public void ToggleLocalNuget()
        {
            string path = null;
            if (!HasNugetSource)
            {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                path = dialog.SelectedPath;
            }

            _settingsFile.Entity.NugetSource = path;
            NotifyOfPropertyChange(() => HasNugetSource);
            Save();
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Save();
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void FlyoutService_ShowFlyout(object _, FlyoutRequest e) => ShowFlyout(e.Header, e.Content);

        private void FlyoutService_CloseFlyout(object _, EventArgs e) => CloseFlyout();

        #endregion
    }
}
