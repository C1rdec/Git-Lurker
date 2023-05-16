using System;

namespace GitLurker.UI.Services
{
    public interface IDebounceService
    {
        bool HasTimer { get; }

        void Debounce(int interval, Action action);

        bool Reset();
    }
}