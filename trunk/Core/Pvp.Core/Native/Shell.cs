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
    /// <summary>
    /// 
    /// </summary>
    public class Shell
    {
        public enum NotifyInfoFlags {Error=0x03, Info=0x01, None=0x00, Warning=0x02}
        public enum NotifyCommand {Add=0x00, Delete=0x02, Modify=0x01}
        public enum NotifyFlags {Message=0x01, Icon=0x02, Tip=0x04, Info=0x10, State=0x08}

        [ComVisible(false)]
        public enum ASSOCIATIONTYPE 
        {
            AT_FILEEXTENSION,
            AT_URLPROTOCOL,
            AT_STARTMENUCLIENT,
            AT_MIMETYPE
        }

        [ComVisible(false)]
        public enum ASSOCIATIONLEVEL 
        {
            AL_MACHINE,
            AL_EFFECTIVE,
            AL_USER
        }

        // CLSID_ApplicationAssociationRegistration
        public static readonly Guid CLSID_ApplicationAssociationRegistration = new Guid("591209c7-767b-42b2-9fba-44ee4615f2c7");

        // CLSID_ApplicationAssociationRegistrationUI
        public static readonly Guid CLSID_ApplicationAssociationRegistrationUI = new Guid("1968106d-f3b5-44cf-890e-116fcb9ecef1");
            
        #region Structures
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] 
        public struct NotifyIconData
        {
            public int cbSize; // DWORD
            public IntPtr hWnd; // HWND
            public int uID; // UINT
            public NotifyFlags uFlags; // UINT
            public int uCallbackMessage; // UINT
            public IntPtr hIcon; // HICON
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
            public string szTip; // char[128]
            public int dwState; // DWORD
            public int dwStateMask; // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
            public string szInfo; // char[256]
            public int uTimeoutOrVersion; // UINT
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
            public string szInfoTitle; // char[64]
            public NotifyInfoFlags dwInfoFlags; // DWORD
        }
        #endregion

        #region Funtions
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int Shell_NotifyIcon(NotifyCommand cmd, ref NotifyIconData data);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern void SHChangeNotify(int wEventId, int uFlags,	IntPtr dwItem1,	IntPtr dwItem2);

        #endregion

        #region Defines
        public const int SHCNE_RENAMEITEM        = 0x00000001;
        public const int SHCNE_CREATE            = 0x00000002;
        public const int SHCNE_DELETE            = 0x00000004;
        public const int SHCNE_MKDIR             = 0x00000008;
        public const int SHCNE_RMDIR             = 0x00000010;
        public const int SHCNE_MEDIAINSERTED     = 0x00000020;
        public const int SHCNE_MEDIAREMOVED      = 0x00000040;
        public const int SHCNE_DRIVEREMOVED      = 0x00000080;
        public const int SHCNE_DRIVEADD          = 0x00000100;
        public const int SHCNE_NETSHARE          = 0x00000200;
        public const int SHCNE_NETUNSHARE        = 0x00000400;
        public const int SHCNE_ATTRIBUTES        = 0x00000800;
        public const int SHCNE_UPDATEDIR         = 0x00001000;
        public const int SHCNE_UPDATEITEM        = 0x00002000;
        public const int SHCNE_SERVERDISCONNECT  = 0x00004000;
        public const int SHCNE_UPDATEIMAGE       = 0x00008000;
        public const int SHCNE_DRIVEADDGUI       = 0x00010000;
        public const int SHCNE_RENAMEFOLDER      = 0x00020000;
        public const int SHCNE_FREESPACE         = 0x00040000;
        public const int SHCNE_EXTENDED_EVENT    = 0x04000000;

        public const int SHCNE_ASSOCCHANGED      = 0x08000000;

        public const int SHCNE_DISKEVENTS        = 0x0002381F;
        public const int SHCNE_GLOBALEVENTS      = 0x0C0581E0; 
        public const int SHCNE_ALLEVENTS         = 0x7FFFFFFF;
        public const uint SHCNE_INTERRUPT         = 0x80000000;

        // Flags
        public const int SHCNF_IDLIST    = 0x0000;        // LPITEMIDLIST
        public const int SHCNF_PATHA     = 0x0001;        // path name
        public const int SHCNF_PRINTERA  = 0x0002;        // printer friendly name
        public const int SHCNF_DWORD     = 0x0003;        // DWORD
        public const int SHCNF_PATHW     = 0x0005;        // path name
        public const int SHCNF_PRINTERW  = 0x0006;        // printer friendly name
        public const int SHCNF_TYPE      = 0x00FF;
        public const int SHCNF_FLUSH     = 0x1000;
        public const int SHCNF_FLUSHNOWAIT = 0x2000;

        public const int SHCNF_PATH    = SHCNF_PATHW;
        public const int SHCNF_PRINTER = SHCNF_PRINTERW;
        
        #endregion

        #region Interfaces

        [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
        GuidAttribute("4e530b0a-e611-4c77-a3ac-9031d022281b"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IApplicationAssociationRegistration
        {
            [PreserveSig]
            int QueryCurrentDefault([In, MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
                                    ASSOCIATIONTYPE atQueryType,
                                    ASSOCIATIONLEVEL alQueryLevel,
                                    [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszAssociation);
        
            [PreserveSig]
            int QueryAppIsDefault([In, MarshalAs(UnmanagedType.LPWStr)] string pszQuery,
                                ASSOCIATIONTYPE atQueryType,
                                ASSOCIATIONLEVEL alQueryLevel,
                                [In, MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName,
                                out bool pfDefault);
        
            [PreserveSig]
            int QueryAppIsDefaultAll(ASSOCIATIONLEVEL alQueryLevel,
                                    [In, MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName,
                                    out bool pfDefault);
        
            [PreserveSig]
            int SetAppAsDefault([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName,
                                [In, MarshalAs(UnmanagedType.LPWStr)] string pszSet,
                                ASSOCIATIONTYPE atSetType);
        
            [PreserveSig]
            int SetAppAsDefaultAll([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName);
        
            [PreserveSig]
            int ClearUserAssociations();
        }

        [ComVisible(true), ComImport, SuppressUnmanagedCodeSecurity,
        GuidAttribute("1f76a169-f994-40ac-8fc8-0959e8874710"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IApplicationAssociationRegistrationUI
        {
            int LaunchAdvancedAssociationUI([In, MarshalAs(UnmanagedType.LPWStr)] string pszAppRegistryName);
        }

        #endregion
    }
}
