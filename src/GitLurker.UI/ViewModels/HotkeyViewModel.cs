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

        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyViewModel" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="hotkey">The hotkey.</param>
        /// <param name="getKey">The get key.</param>
        public HotkeyViewModel(Hotkey hotkey, System.Action callback)
        {
            _hotkey = hotkey;
            _save = callback;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [not defined].
        /// </summary>
        public bool NotDefined => !this._hotkey.IsDefined();

        /// <summary>
        /// Gets a value indicating whether this instance has modifier.
        /// </summary>
        public bool HasModifier => this.Modifier != Modifiers.None;

        /// <summary>
        /// Gets a value indicating whether this instance has key code.
        /// </summary>
        public bool HasKeyCode => this.KeyCode != KeyCode.None;

        /// <summary>
        /// Gets the modifier.
        /// </summary>
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
                NotifyOfPropertyChange(() => this.HasModifier);
            }
        }

        /// <summary>
        /// Gets the key code.
        /// </summary>
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

        /// <summary>
        /// Gets the name value.
        /// </summary>
        public string NameValue => "Open";

        #endregion

        #region Methods

        /// <summary>
        /// Sets the key code.
        /// </summary>
        public async void SetKeyCode()
        {
            var result = await IoC.Get<KeyboardService>().GetNextKeyAsync();

            Modifier = result.Modifier;
            KeyCode = result.Key;

            _save();
        }

        #endregion
    }
}
