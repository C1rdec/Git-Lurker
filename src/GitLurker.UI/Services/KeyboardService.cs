using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;
using Winook;

namespace GitLurker.Services
{
    public class KeyboardService : IDisposable
    {
        #region Fields

        private DebounceService _debounceService;
        private KeyboardHook _hook;
        private bool _enableWASD;

        #endregion

        #region Events

        public event EventHandler EnterPressed;

        public event EventHandler EnterLongPressed;

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
            _debounceService = new DebounceService(false);
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

            // Enter
            _hook.AddHandler(KeyCode.Enter, Modifiers.None, KeyDirection.Down, (o, e) => 
            {
                Execute.OnUIThread(() => 
                { 
                    _debounceService.Debounce(666, () => EnterLongPressed?.Invoke(this, EventArgs.Empty));
                });
            });
            _hook.AddHandler(KeyCode.Enter, (o, e) => 
            {
                _debounceService.Stop();
                EnterPressed?.Invoke(this, EventArgs.Empty); 
            });

            // Left
            _hook.AddHandler(KeyCode.Enter, (o, e) => LeftPressed?.Invoke(this, EventArgs.Empty));

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

        public Task<(KeyCode Key, Modifiers Modifier)> GetNextKeyAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<(KeyCode key, Modifiers modifier)>();

            EventHandler<KeyboardMessageEventArgs> handler = default;
            handler = (object s, KeyboardMessageEventArgs e) =>
            {
                if (e.Direction == KeyDirection.Up)
                {
                    var code = (KeyCode)e.KeyValue;
                    if (code == KeyCode.Escape)
                    {
                        return;
                    }

                    var keyCode = code;
                    var modifier = Modifiers.None;
                    if (e.Control)
                    {
                        modifier = Modifiers.Control;
                    }
                    else if (e.Alt)
                    {
                        modifier = Modifiers.Alt;
                    }
                    else if (e.Shift)
                    {
                        modifier = Modifiers.Shift;
                    }

                    _hook.MessageReceived -= handler;
                    taskCompletionSource.SetResult((keyCode, modifier));
                }
            };

            _hook.MessageReceived += handler;

            return taskCompletionSource.Task;
        }

        #endregion
    }
}
