using System;
using System.Windows.Threading;

namespace GitLurker.Services
{
    public class DebounceService : IDebounceService
    {
        #region Fields

        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public bool HasTimer => _timer != null;

        #endregion

        #region Methods

        public void Debounce(int interval, Action action)
        {
            _timer?.Stop();
            _timer = null;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), DispatcherPriority.ApplicationIdle, (s, e) =>
            {
                if (_timer == null)
                {
                    return;
                }

                _timer?.Stop();
                _timer = null;

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
