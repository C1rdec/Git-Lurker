namespace GitLurker.UI.Helpers;

using System;
using System.Windows;
using System.Windows.Interop;
using GitLurker.Core;

internal class DockingHelper
{
    public static void SetForeground(Window window, Action callback)
    {
        if (window == null)
        {
            return;
        }

        var handle = new WindowInteropHelper(window).Handle;
        _ = Native.SetForegroundWindow(handle);

        callback();
    }
}
