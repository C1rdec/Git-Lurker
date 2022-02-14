using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Winook;

namespace GitLurker.Services
{
    public class KeyboardService: IDisposable
    {
        #region Fields

        private KeyboardHook _hook;
        private bool _enableWASD;

        #endregion

        #region Events

        public event EventHandler OnePressed;

        public event EventHandler TwoPressed;

        public event EventHandler UpPressed;

        public event EventHandler DownPressed;

        public event EventHandler LeftPressed;

        public event EventHandler RightPressed;

        public event EventHandler NextTabPressed;

        public event EventHandler PreviousTabPressed;

        #endregion

        #region Constructors

        public KeyboardService(bool enableWASD)
        {
            _enableWASD = enableWASD;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            _hook.RemoveAllHandlers();
            _hook.Dispose();
        }

        public async Task InstallAsync()
        {
            var process = Process.GetCurrentProcess();
            _hook = new KeyboardHook(process.Id);

            // Left
            _hook.AddHandler(KeyCode.Left, (o, e) => LeftPressed?.Invoke(this, EventArgs.Empty));

            // Right
            _hook.AddHandler(KeyCode.Right, (o, e) => RightPressed?.Invoke(this, EventArgs.Empty));

            // Up
            _hook.AddHandler(KeyCode.Up, (o, e) => UpPressed?.Invoke(this, EventArgs.Empty));

            // Down
            _hook.AddHandler(KeyCode.Down, (o, e) => DownPressed?.Invoke(this, EventArgs.Empty));

            _hook.AddHandler(KeyCode.Tab, (o, e) => NextTabPressed?.Invoke(this, EventArgs.Empty));
            _hook.AddHandler(KeyCode.Tab, Modifiers.Shift,(o, e) => PreviousTabPressed?.Invoke(this, EventArgs.Empty));

            _hook.AddHandler(KeyCode.D1, (o, e) => OnePressed?.Invoke(this, EventArgs.Empty));
            _hook.AddHandler(KeyCode.D2, (o, e) => TwoPressed?.Invoke(this, EventArgs.Empty));

            if (_enableWASD)
            {
                _hook.AddHandler(KeyCode.W, (o, e) => UpPressed?.Invoke(this, EventArgs.Empty));
                _hook.AddHandler(KeyCode.A, (o, e) => LeftPressed?.Invoke(this, EventArgs.Empty));
                _hook.AddHandler(KeyCode.S, (o, e) => DownPressed?.Invoke(this, EventArgs.Empty));
                _hook.AddHandler(KeyCode.D, (o, e) => RightPressed?.Invoke(this, EventArgs.Empty));
            }

            await _hook.InstallAsync();
        }

        #endregion
    }
}
