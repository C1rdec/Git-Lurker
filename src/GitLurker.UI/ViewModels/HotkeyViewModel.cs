using Caliburn.Micro;
using GitLurker.Models;
using GitLurker.Services;
using Winook;

namespace GitLurker.UI.ViewModels
{
    public class HotkeyViewModel : Caliburn.Micro.PropertyChangedBase
    {
        #region Fields

        private Hotkey _hotkey;
        private System.Action _save;

        #endregion

        #region Constructors

        public HotkeyViewModel(Hotkey hotkey, System.Action callback)
        {
            _hotkey = hotkey;
            _save = callback;
        }

        #endregion

        #region Properties

        public bool NotDefined => !_hotkey.IsDefined();

        public bool HasModifier => Modifier != Modifiers.None;

        public bool HasKeyCode => KeyCode != KeyCode.None;

        public Modifiers Modifier
        {
            get
            {
                return _hotkey.Modifier;
            }

            private set
            {
                _hotkey.Modifier = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasModifier);
            }
        }

        public KeyCode KeyCode
        {
            get
            {
                return _hotkey.KeyCode;
            }

            private set
            {
                _hotkey.KeyCode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasKeyCode);
                NotifyOfPropertyChange(() => NotDefined);
            }
        }

        public string NameValue => "Open";

        #endregion

        #region Methods

        public async void SetKeyCode()
        {
            var dialog = IoC.Get<DialogService>();

            var task = IoC.Get<KeyboardService>().GetNextKeyAsync();
            await dialog.ShowProgressAsync("Waiting...", "Press any keys", task);
            var result = await task;

            Modifier = result.Modifier;
            KeyCode = result.Key;

            _save();
        }

        #endregion
    }
}
