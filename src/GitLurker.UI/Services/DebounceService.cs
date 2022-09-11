using System;
using System.Windows.Threading;

namespace GitLurker.Services
{
    public class DebounceService
    {
        #region Fields

        private DispatcherTimer _timer;
        private bool _cancelAction;
        private bool _executed;

        #endregion

        #region Constructors

        public DebounceService()
            : this(true)
        {

        }

        public DebounceService(bool cancelAction)
        {
            _cancelAction = cancelAction;
        }

        #endregion

        #region Methods

        public void Debounce(int interval, Action action)
        {
            if (_timer != null && !_cancelAction)
            {
                return;
            }

            _timer?.Stop();
            _timer = null;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.ApplicationIdle, (s, e) =>
            {
                if (_timer == null || _executed)
                {
                    return;
                }

                if (_cancelAction)
                {
                    _executed = true;
                }

                _timer?.Stop();
                _timer = null;
                action.Invoke();
            }, Dispatcher.CurrentDispatcher);

            _timer.Start();
        }

        public bool Reset()
        {
            _executed = false;
            if (_timer == null)
            {
                return false;
            }

            _timer?.Stop();
            _timer = null;

            return true;
        }

        #endregion
    }
}
