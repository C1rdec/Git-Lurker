using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Services;
using MahApps.Metro.IconPacks;
using Winook;

namespace GitLurker.UI.ViewModels
{
    public class HotkeyViewModel : PropertyChangedBase
    {
        #region Fields

        private string _name;
        private Hotkey _hotkey;
        private System.Action<KeyCode, Modifiers> _save;
        private PackIconControlBase _icon;

        #endregion

        #region Constructors

        public HotkeyViewModel(Hotkey hotkey, System.Action<KeyCode, Modifiers> callback, string name)
        {
            _name = name;
            _hotkey = hotkey;
            _save = callback;

            if (hotkey != null)
            {
                HandleIcon(hotkey.KeyCode);
            }
        }

        public HotkeyViewModel(Hotkey hotkey, System.Action<KeyCode, Modifiers> callback)
            : this(hotkey, callback, "Open")
        {
        }

        #endregion

        #region Properties

        public bool NotDefined => !_hotkey.IsDefined();

        public bool HasModifier => Modifier != Modifiers.None;

        public bool HasKeyCode => KeyCode != KeyCode.None && Icon == null;

        public bool HasIcon => Icon != null;

        public Modifiers Modifier
        {
            get => _hotkey.Modifier;
            private set
            {
                _hotkey.Modifier = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasModifier);
            }
        }

        public KeyCode KeyCode
        {
            get => _hotkey.KeyCode;
            private set
            {
                _hotkey.KeyCode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasKeyCode);
                NotifyOfPropertyChange(() => NotDefined);
            }
        }

        public PackIconControlBase Icon
        {
            get => _icon;
            private set
            {
                _icon = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasIcon);
                NotifyOfPropertyChange(() => HasKeyCode);
            }
        }

        public string NameValue => _name;

        #endregion

        #region Methods

        public async void SetKeyCode()
        {
            var dialog = IoC.Get<DialogService>();

            var task = IoC.Get<KeyboardService>().GetNextKeyAsync();
            await dialog.ShowProgressAsync("Waiting...", "Press any keys", task);
            var result = await task;

            if (result.Key == KeyCode.Escape)
            {
                return;
            }

            Modifier = result.Modifier;
            KeyCode = result.Key;

            HandleIcon(KeyCode);

            _save(result.Key, result.Modifier);
        }

        private void HandleIcon(KeyCode code)
        {
            Icon = null;
            var codeValue = code.ToString();

            if (codeValue.ToLower().Contains("launchapplication"))
            {
                Icon = new PackIconBootstrapIcons() { Kind = PackIconBootstrapIconsKind.Calculator };
            }
        }

        #endregion
    }
}
