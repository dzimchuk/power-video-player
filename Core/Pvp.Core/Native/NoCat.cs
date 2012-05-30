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

namespace Pvp.Core.Native
{
    /// <summary>
    /// 
    /// </summary>
    public class NoCat
    {
        #region Funtions
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetErrorMode(int uMode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SystemParametersInfo(int uiAction, int uiParam, 
            IntPtr pvParam,	int fWinIni);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out GDI.POINT pt);

        #endregion

        #region Defines
        public const int SEM_FAILCRITICALERRORS		= 0x0001;
        public const int SEM_NOGPFAULTERRORBOX		= 0x0002;
        public const int SEM_NOALIGNMENTFAULTEXCEPT	= 0x0004;
        public const int SEM_NOOPENFILEERRORBOX		= 0x8000;
        
        /*
        * Parameter for SystemParametersInfo()
        */
        public const int SPI_GETBEEP                 = 0x0001;
        public const int SPI_SETBEEP                 = 0x0002;
        public const int SPI_GETMOUSE                = 0x0003;
        public const int SPI_SETMOUSE                = 0x0004;
        public const int SPI_GETBORDER               = 0x0005;
        public const int SPI_SETBORDER               = 0x0006;
        public const int SPI_GETKEYBOARDSPEED        = 0x000A;
        public const int SPI_SETKEYBOARDSPEED        = 0x000B;
        public const int SPI_LANGDRIVER              = 0x000C;
        public const int SPI_ICONHORIZONTALSPACING   = 0x000D;
        public const int SPI_GETSCREENSAVETIMEOUT    = 0x000E;
        public const int SPI_SETSCREENSAVETIMEOUT    = 0x000F;
        public const int SPI_GETSCREENSAVEACTIVE     = 0x0010;
        public const int SPI_SETSCREENSAVEACTIVE     = 0x0011;
        public const int SPI_GETGRIDGRANULARITY      = 0x0012;
        public const int SPI_SETGRIDGRANULARITY      = 0x0013;
        public const int SPI_SETDESKWALLPAPER        = 0x0014;
        public const int SPI_SETDESKPATTERN          = 0x0015;
        public const int SPI_GETKEYBOARDDELAY        = 0x0016;
        public const int SPI_SETKEYBOARDDELAY        = 0x0017;
        public const int SPI_ICONVERTICALSPACING     = 0x0018;
        public const int SPI_GETICONTITLEWRAP        = 0x0019;
        public const int SPI_SETICONTITLEWRAP        = 0x001A;
        public const int SPI_GETMENUDROPALIGNMENT    = 0x001B;
        public const int SPI_SETMENUDROPALIGNMENT    = 0x001C;
        public const int SPI_SETDOUBLECLKWIDTH       = 0x001D;
        public const int SPI_SETDOUBLECLKHEIGHT      = 0x001E;
        public const int SPI_GETICONTITLELOGFONT     = 0x001F;
        public const int SPI_SETDOUBLECLICKTIME      = 0x0020;
        public const int SPI_SETMOUSEBUTTONSWAP      = 0x0021;
        public const int SPI_SETICONTITLELOGFONT     = 0x0022;
        public const int SPI_GETFASTTASKSWITCH       = 0x0023;
        public const int SPI_SETFASTTASKSWITCH       = 0x0024;

        public const int SPI_SETDRAGFULLWINDOWS      = 0x0025;
        public const int SPI_GETDRAGFULLWINDOWS      = 0x0026;
        public const int SPI_GETNONCLIENTMETRICS     = 0x0029;
        public const int SPI_SETNONCLIENTMETRICS     = 0x002A;
        public const int SPI_GETMINIMIZEDMETRICS     = 0x002B;
        public const int SPI_SETMINIMIZEDMETRICS     = 0x002C;
        public const int SPI_GETICONMETRICS          = 0x002D;
        public const int SPI_SETICONMETRICS          = 0x002E;
        public const int SPI_SETWORKAREA             = 0x002F;
        public const int SPI_GETWORKAREA             = 0x0030;
        public const int SPI_SETPENWINDOWS           = 0x0031;

        public const int SPI_GETHIGHCONTRAST         = 0x0042;
        public const int SPI_SETHIGHCONTRAST         = 0x0043;
        public const int SPI_GETKEYBOARDPREF         = 0x0044;
        public const int SPI_SETKEYBOARDPREF         = 0x0045;
        public const int SPI_GETSCREENREADER         = 0x0046;
        public const int SPI_SETSCREENREADER         = 0x0047;
        public const int SPI_GETANIMATION            = 0x0048;
        public const int SPI_SETANIMATION            = 0x0049;
        public const int SPI_GETFONTSMOOTHING        = 0x004A;
        public const int SPI_SETFONTSMOOTHING        = 0x004B;
        public const int SPI_SETDRAGWIDTH            = 0x004C;
        public const int SPI_SETDRAGHEIGHT           = 0x004D;
        public const int SPI_SETHANDHELD             = 0x004E;
        public const int SPI_GETLOWPOWERTIMEOUT      = 0x004F;
        public const int SPI_GETPOWEROFFTIMEOUT      = 0x0050;
        public const int SPI_SETLOWPOWERTIMEOUT      = 0x0051;
        public const int SPI_SETPOWEROFFTIMEOUT      = 0x0052;
        public const int SPI_GETLOWPOWERACTIVE       = 0x0053;
        public const int SPI_GETPOWEROFFACTIVE       = 0x0054;
        public const int SPI_SETLOWPOWERACTIVE       = 0x0055;
        public const int SPI_SETPOWEROFFACTIVE       = 0x0056;
        public const int SPI_SETCURSORS              = 0x0057;
        public const int SPI_SETICONS                = 0x0058;
        public const int SPI_GETDEFAULTINPUTLANG     = 0x0059;
        public const int SPI_SETDEFAULTINPUTLANG     = 0x005A;
        public const int SPI_SETLANGTOGGLE           = 0x005B;
        public const int SPI_GETWINDOWSEXTENSION     = 0x005C;
        public const int SPI_SETMOUSETRAILS          = 0x005D;
        public const int SPI_GETMOUSETRAILS          = 0x005E;
        public const int SPI_SETSCREENSAVERRUNNING   = 0x0061;
        public const int SPI_SCREENSAVERRUNNING      = SPI_SETSCREENSAVERRUNNING;

        public const int SPI_GETFILTERKEYS          = 0x0032;
        public const int SPI_SETFILTERKEYS          = 0x0033;
        public const int SPI_GETTOGGLEKEYS          = 0x0034;
        public const int SPI_SETTOGGLEKEYS          = 0x0035;
        public const int SPI_GETMOUSEKEYS           = 0x0036;
        public const int SPI_SETMOUSEKEYS           = 0x0037;
        public const int SPI_GETSHOWSOUNDS          = 0x0038;
        public const int SPI_SETSHOWSOUNDS          = 0x0039;
        public const int SPI_GETSTICKYKEYS          = 0x003A;
        public const int SPI_SETSTICKYKEYS          = 0x003B;
        public const int SPI_GETACCESSTIMEOUT       = 0x003C;
        public const int SPI_SETACCESSTIMEOUT       = 0x003D;

        public const int SPI_GETSERIALKEYS          = 0x003E;
        public const int SPI_SETSERIALKEYS          = 0x003F;

        public const int SPI_GETSOUNDSENTRY         = 0x0040;
        public const int SPI_SETSOUNDSENTRY         = 0x0041;

        public const int SPI_GETSNAPTODEFBUTTON     = 0x005F;
        public const int SPI_SETSNAPTODEFBUTTON     = 0x0060;

        public const int SPI_GETMOUSEHOVERWIDTH     = 0x0062;
        public const int SPI_SETMOUSEHOVERWIDTH     = 0x0063;
        public const int SPI_GETMOUSEHOVERHEIGHT    = 0x0064;
        public const int SPI_SETMOUSEHOVERHEIGHT    = 0x0065;
        public const int SPI_GETMOUSEHOVERTIME      = 0x0066;
        public const int SPI_SETMOUSEHOVERTIME      = 0x0067;
        public const int SPI_GETWHEELSCROLLLINES    = 0x0068;
        public const int SPI_SETWHEELSCROLLLINES    = 0x0069;
        public const int SPI_GETMENUSHOWDELAY       = 0x006A;
        public const int SPI_SETMENUSHOWDELAY       = 0x006B;

        public const int SPI_GETSHOWIMEUI          = 0x006E;
        public const int SPI_SETSHOWIMEUI          = 0x006F;

        public const int SPI_GETMOUSESPEED         = 0x0070;
        public const int SPI_SETMOUSESPEED         = 0x0071;
        public const int SPI_GETSCREENSAVERRUNNING = 0x0072;
        public const int SPI_GETDESKWALLPAPER      = 0x0073;

        public const int SPI_GETACTIVEWINDOWTRACKING         = 0x1000;
        public const int SPI_SETACTIVEWINDOWTRACKING         = 0x1001;
        public const int SPI_GETMENUANIMATION                = 0x1002;
        public const int SPI_SETMENUANIMATION                = 0x1003;
        public const int SPI_GETCOMBOBOXANIMATION            = 0x1004;
        public const int SPI_SETCOMBOBOXANIMATION            = 0x1005;
        public const int SPI_GETLISTBOXSMOOTHSCROLLING       = 0x1006;
        public const int SPI_SETLISTBOXSMOOTHSCROLLING       = 0x1007;
        public const int SPI_GETGRADIENTCAPTIONS             = 0x1008;
        public const int SPI_SETGRADIENTCAPTIONS             = 0x1009;
        public const int SPI_GETKEYBOARDCUES                 = 0x100A;
        public const int SPI_SETKEYBOARDCUES                 = 0x100B;
        public const int SPI_GETMENUUNDERLINES               = SPI_GETKEYBOARDCUES;
        public const int SPI_SETMENUUNDERLINES               = SPI_SETKEYBOARDCUES;
        public const int SPI_GETACTIVEWNDTRKZORDER           = 0x100C;
        public const int SPI_SETACTIVEWNDTRKZORDER           = 0x100D;
        public const int SPI_GETHOTTRACKING                  = 0x100E;
        public const int SPI_SETHOTTRACKING                  = 0x100F;
        public const int SPI_GETMENUFADE                     = 0x1012;
        public const int SPI_SETMENUFADE                     = 0x1013;
        public const int SPI_GETSELECTIONFADE                = 0x1014;
        public const int SPI_SETSELECTIONFADE                = 0x1015;
        public const int SPI_GETTOOLTIPANIMATION             = 0x1016;
        public const int SPI_SETTOOLTIPANIMATION             = 0x1017;
        public const int SPI_GETTOOLTIPFADE                  = 0x1018;
        public const int SPI_SETTOOLTIPFADE                  = 0x1019;
        public const int SPI_GETCURSORSHADOW                 = 0x101A;
        public const int SPI_SETCURSORSHADOW                 = 0x101B;

        public const int SPI_GETMOUSESONAR                   = 0x101C;
        public const int SPI_SETMOUSESONAR                   = 0x101D;
        public const int SPI_GETMOUSECLICKLOCK               = 0x101E;
        public const int SPI_SETMOUSECLICKLOCK               = 0x101F;
        public const int SPI_GETMOUSEVANISH                  = 0x1020;
        public const int SPI_SETMOUSEVANISH                  = 0x1021;
        public const int SPI_GETFLATMENU                     = 0x1022;
        public const int SPI_SETFLATMENU                     = 0x1023;
        public const int SPI_GETDROPSHADOW                   = 0x1024;
        public const int SPI_SETDROPSHADOW                   = 0x1025;
        public const int SPI_GETBLOCKSENDINPUTRESETS         = 0x1026;
        public const int SPI_SETBLOCKSENDINPUTRESETS         = 0x1027;

        public const int SPI_GETUIEFFECTS                    = 0x103E;
        public const int SPI_SETUIEFFECTS                    = 0x103F;

        public const int SPI_GETFOREGROUNDLOCKTIMEOUT        = 0x2000;
        public const int SPI_SETFOREGROUNDLOCKTIMEOUT        = 0x2001;
        public const int SPI_GETACTIVEWNDTRKTIMEOUT          = 0x2002;
        public const int SPI_SETACTIVEWNDTRKTIMEOUT          = 0x2003;
        public const int SPI_GETFOREGROUNDFLASHCOUNT         = 0x2004;
        public const int SPI_SETFOREGROUNDFLASHCOUNT         = 0x2005;
        public const int SPI_GETCARETWIDTH                   = 0x2006;
        public const int SPI_SETCARETWIDTH                   = 0x2007;

        public const int SPI_GETMOUSECLICKLOCKTIME           = 0x2008;
        public const int SPI_SETMOUSECLICKLOCKTIME           = 0x2009;
        public const int SPI_GETFONTSMOOTHINGTYPE            = 0x200A;
        public const int SPI_SETFONTSMOOTHINGTYPE            = 0x200B;

        /* constants for SPI_GETFONTSMOOTHINGTYPE and SPI_SETFONTSMOOTHINGTYPE: */
        public const int FE_FONTSMOOTHINGSTANDARD            = 0x0001;
        public const int FE_FONTSMOOTHINGCLEARTYPE           = 0x0002;
        public const int FE_FONTSMOOTHINGDOCKING             = 0x8000;

        public const int SPI_GETFONTSMOOTHINGCONTRAST           = 0x200C;
        public const int SPI_SETFONTSMOOTHINGCONTRAST           = 0x200D;

        public const int SPI_GETFOCUSBORDERWIDTH             = 0x200E;
        public const int SPI_SETFOCUSBORDERWIDTH             = 0x200F;
        public const int SPI_GETFOCUSBORDERHEIGHT            = 0x2010;
        public const int SPI_SETFOCUSBORDERHEIGHT            = 0x2011;

        public const int SPI_GETFONTSMOOTHINGORIENTATION           = 0x2012;
        public const int SPI_SETFONTSMOOTHINGORIENTATION           = 0x2013;

        /* constants for SPI_GETFONTSMOOTHINGORIENTATION and SPI_SETFONTSMOOTHINGORIENTATION: */
        public const int FE_FONTSMOOTHINGORIENTATIONBGR   = 0x0000;
        public const int FE_FONTSMOOTHINGORIENTATIONRGB   = 0x0001;

        /*
        * Flags
        */
        public const int SPIF_UPDATEINIFILE    = 0x0001;
        public const int SPIF_SENDWININICHANGE = 0x0002;
        public const int SPIF_SENDCHANGE       = SPIF_SENDWININICHANGE;

        #endregion
    }
}
