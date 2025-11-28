namespace GitLurker.UI.ViewModels;

using System;
using Caliburn.Micro;
using GitLurker.UI.Models;
using GitLurker.UI.Services;

public abstract class FlyoutScreenBase : Screen
{
    private FlyoutService _flyoutService;
    private PropertyChangedBase _flyoutContent;

    public FlyoutScreenBase(FlyoutService service)
    {
        _flyoutService = service;
        _flyoutService.ShowFlyoutRequested += FlyoutService_ShowFlyout;
        _flyoutService.CloseFlyoutRequested += FlyoutService_CloseFlyout;
    }

    public string FlyoutHeader
    {
        get => field;
        set
        {
            field = value;
            NotifyOfPropertyChange(() => FlyoutHeader);
        }
    }

    public bool FlyoutOpen
    {
        get => field;
        set
        {
            if (!value)
            {
                _flyoutService.NotifyFlyoutClosed();
            }

            field = value;
            NotifyOfPropertyChange(() => FlyoutOpen);
        }
    }

    public PropertyChangedBase FlyoutContent
    {
        get => _flyoutContent;
        set
        {
            _flyoutContent = value;
            NotifyOfPropertyChange(() => FlyoutContent);
        }
    }

    public void ShowFlyout(string header, PropertyChangedBase content)
    {
        FlyoutHeader = header;
        FlyoutContent = content;
        FlyoutOpen = true;
    }

    public void CloseFlyout()
    {
        FlyoutOpen = false;

        // We use the field since we dont want to notify the UI
        _flyoutContent = null;
    }

    private void FlyoutService_ShowFlyout(object _, FlyoutRequest e) => ShowFlyout(e.Header, e.Content);

    private void FlyoutService_CloseFlyout(object _, EventArgs e) => CloseFlyout();
}
