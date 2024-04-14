namespace GitLurker.UI.Services;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GitLurker.UI.Models;
using Winook;

public class MouseService
{
    #region Fields

    private int _x;
    private int _y;
    private MouseHook _hook;

    #endregion

    public event EventHandler LeftButtonUp;

    public event EventHandler<MousePosition> MousePositionChanged;

    public MousePosition GetCurrentPosition()
        => new() { X = _x, Y = _y };

    public Task InstallAsync()
    {
        var process = Process.GetCurrentProcess();
        _hook = new MouseHook(process.Id);
        _hook.MouseMove += Hook_MouseMove;
        _hook.LeftButtonUp += Hook_LeftButtonUp;

        return _hook.InstallAsync();
    }

    private void Hook_LeftButtonUp(object sender, MouseMessageEventArgs e)
    {
        LeftButtonUp?.Invoke(this, EventArgs.Empty);
    }

    private void Hook_MouseMove(object sender, MouseMessageEventArgs e)
    {
        _x = e.X;
        _y = e.Y;
        MousePositionChanged?.Invoke(this, new MousePosition{ X = e.X, Y = e.Y});
    }
}
