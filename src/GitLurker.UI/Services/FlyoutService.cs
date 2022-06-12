using System;
using Caliburn.Micro;
using GitLurker.UI.Models;
using MahApps.Metro.Controls;

namespace GitLurker.UI.Services
{
    public class FlyoutService
    {
        #region Events

        public event EventHandler<FlyoutRequest> ShowFlyoutRequested;
        public event EventHandler CloseFlyoutRequested;
        public event EventHandler FlyoutClosed;

        #endregion

        #region Methods

        public Position CurrentPosition { get; private set; }

        #endregion

        #region Methods

        public void Show(string header, PropertyChangedBase content, Position position)
        {
            CurrentPosition = position;
            var request = new FlyoutRequest
            {
                Header = header,
                Content = content,
                Position = position
            };
            ShowFlyoutRequested?.Invoke(this, request);
        }

        public void Close()
        {
            CloseFlyoutRequested?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyFlyoutClosed()
        {
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
