/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Pvp.Core.Native
{
    [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
    GuidAttribute("00000001-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        [PreserveSig]
        int CreateInstance([In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
                           [In] ref Guid riid,
                           [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);

        [PreserveSig]
        int LockServer([In, MarshalAs(UnmanagedType.Bool)] bool fLock);
    }
}
