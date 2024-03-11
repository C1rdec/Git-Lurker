namespace GitLurker.UI.Extensions;

using System.Diagnostics;
using System.Text;
using GitLurker.Core;

public static class ProcessExtensions
{
    public static string GetMainModuleFileName(this Process process, int buffer = 1024)
    {
        var fileNameBuilder = new StringBuilder(buffer);
        var bufferLength = (uint)fileNameBuilder.Capacity + 1;

        return Native.QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
            fileNameBuilder.ToString() :
            null;
    }
}
