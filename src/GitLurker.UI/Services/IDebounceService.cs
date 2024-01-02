namespace GitLurker.UI.Services;

using System;

public interface IDebounceService
{
    bool HasTimer { get; }

    void Debounce(int interval, Action action);

    bool Reset();
}