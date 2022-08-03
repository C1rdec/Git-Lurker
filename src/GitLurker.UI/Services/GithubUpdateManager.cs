using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using GitLurker.Models;

namespace GitLurker.UI.Services
{
    public class GithubUpdateManager
    {
        #region Fields

        private Repository _repo;
        private Timer _timer;

        #endregion

        #region Events

        public event EventHandler UpdateRequested;

        #endregion

        #region Methods

        public async Task Update()
        {
            await _repo.PullAsync();
            _ = _repo.ExecuteCommandAsync("dotnet run --project .\\src\\GitLurker.UI\\GitLurker.UI.csproj -c release");
            Process.GetCurrentProcess().Kill();
        }

        public void Watch(Repository repo)
        {
            _repo = repo;

            if (CheckForUpdate())
            {
                return;
            }

            if (_timer != null)
            {
                DisposeTimer();
            }

            _timer = new Timer(TimeSpan.FromMinutes(8).TotalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckForUpdate();
        }

        private bool CheckForUpdate()
        {
            try
            {
                _repo.Fetch();
            }
            catch
            {
                return false;
            }

            var needUpdate = _repo.IsBehind();
            if (needUpdate)
            {
                UpdateRequested?.Invoke(this, EventArgs.Empty);
                DisposeTimer();
            }

            return needUpdate;
        }

        private void DisposeTimer()
        {
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            _timer.Elapsed -= Timer_Elapsed;
            _timer.Dispose();
            _timer = null;
        }

        #endregion
    }
}
