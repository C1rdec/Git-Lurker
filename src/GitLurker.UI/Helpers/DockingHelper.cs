using System;
using System.Windows;
using System.Windows.Interop;
using GitLurker.Core;

namespace GitLurker.UI.Helpers
{
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
}
