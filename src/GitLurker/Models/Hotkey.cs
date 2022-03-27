using Winook;

namespace GitLurker.Models
{
    public sealed class Hotkey
    {
        #region Properties

        public Modifiers Modifier { get; set; }

        public KeyCode KeyCode { get; set; }

        #endregion

        #region Methods

        public bool IsDefined()
        {
            return KeyCode != KeyCode.None;
        }

        #endregion
    }
}
