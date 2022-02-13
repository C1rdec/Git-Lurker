using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GitLurker.UI.Helpers
{
    internal class DockingHelper
    {
        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);

        public static void SetForeground(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            SetForegroundWindow(handle);
        }
    }
}
