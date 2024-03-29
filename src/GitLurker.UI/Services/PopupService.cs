﻿namespace GitLurker.UI.Services;

public class PopupService
{
    #region Fields

    private readonly IDebounceService _debounce;

    #endregion

    #region Constructors

    public PopupService(IDebounceService debounceService)
    {
        _debounce = debounceService;
    }

    #endregion

    #region Properties

    public bool IsOpen { get; set; }

    public bool JustClosed { get; set; }

    #endregion

    #region Methods

    public void SetClosed()
    {
        JustClosed = true;
        _debounce.Debounce(150, () => JustClosed = false);
    }

    #endregion
}
