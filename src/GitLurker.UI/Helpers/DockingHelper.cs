using System;
using System.Windows;
using System.Windows.Interop;

namespace GitLurker.UI.Helpers
{
    internal class DockingHelper
    {
        public static void SetForeground(Window window, Action callback)
        {
            var handle = new WindowInteropHelper(window).Handle;

            var foreground = Native.GetForegroundWindow();

            if (handle == foreground)
            {
                callback();
            }

            Native.SetForegroundWindow(handle);
        }
    }
}
