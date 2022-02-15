using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GitLurker.UI.Helpers
{
    internal class DockingHelper
    {
        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        public static void SetForeground(Window window, Action callback)
        {
            var handle = new WindowInteropHelper(window).Handle;

            var foreground = GetForegroundWindow();

            if (handle == foreground)
            {
                callback();
            }

            var result = SetForegroundWindow(handle);
            Debug.WriteLine("SetForeground");
        }
    }
}
