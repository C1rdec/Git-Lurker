using System;
using System.Windows.Threading;

namespace GitLurker.Services
{
    public class DebounceService
    {
        #region Fields

        private DispatcherTimer _timer;
        private bool _manual;

        #endregion

        #region Constructors

        public DebounceService()
            : this(false)
        {

        }

        public DebounceService(bool manual)
        {
            _manual = manual;
        }

        #endregion

        #region Methods

        public void Debounce(int interval, Action action)
        {
            if (_timer != null)
            {
                return;
            }

            _timer?.Stop();
            _timer = null;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.ApplicationIdle, (s, e) =>
            {
                if (_timer == null)
                {
                    return;
                }

                if (!_manual)
                {
                    _timer?.Stop();
                    _timer = null;
                }

                action.Invoke();
            }, Dispatcher.CurrentDispatcher);

            _timer.Start();
        }

        public bool Reset()
        {
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
