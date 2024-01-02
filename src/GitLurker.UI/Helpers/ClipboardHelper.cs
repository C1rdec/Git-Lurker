namespace GitLurker.UI.Helper;

using System;
using System.Threading;

internal class ClipboardHelper
{
    public static void SetText(string text)
    {
        RetryOnMainThread(() =>
        {
            System.Windows.Clipboard.SetText(text);
        });
    }

    private static void RetryOnMainThread(Action action)
    {
        var thread = new Thread(() =>
        {
            var retryCount = 3;
            while (retryCount != 0)
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    retryCount--;
                    Thread.Sleep(200);
                }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
