﻿using System;
using Windows.UI.Input;

namespace GitLurker.UI.Models
{
    [System.Runtime.InteropServices.Guid("1B0535C9-57AD-45C1-9D79-AD5C34360513")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIInspectable)]
    public interface IRadialControllerInterop
    {
        RadialContro­ller CreateForWindow(
        IntPtr hwnd,
        [System.Runtime.InteropServices.In] ref Guid riid);
    }

    [System.Runtime.InteropServices.Guid("787cdaac-3186-476d-87e4-b9374a7b9970")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIInspectable)]
    public interface IRadialControllerConfigurationInterop
    {
        RadialControllerConfiguration GetForWindow(
        IntPtr hwnd,
        [System.Runtime.InteropServices.In] ref Guid riid);
    }
}
