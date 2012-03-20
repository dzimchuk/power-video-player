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

/* 
 * PIN_DIRECTION
 * FILTER_STATE
 * AM_MEDIA_TYPE
 * FILTER_INFO
 * PIN_INFO 
 * IPin
 * IFilterGraph
 * IEnumPins
 * IEnumFilters
 * IPersist
 * IReferenceClock
 * IMediaFilter
 * IBaseFilter
 * IEnumMediaTypes
 * 
 * ISpecifyPropertyPages
 * CAUUID
 * IFileSourceFilter
*/
namespace Pvp.Core.DirectShow
{
    [ComVisible(false)]
    public enum PinDirection		// PIN_DIRECTION
    {
        Input,		// PINDIR_INPUT
        Output		// PINDIR_OUTPUT
    }

    [ComVisible(false)]
    public enum FilterState		// FILTER_STATE
    {
        State_Stopped,
        State_Paused,
        State_Running
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct AMMediaType		//  AM_MEDIA_TYPE
    {
        public Guid		majorType;
        public Guid		subType;
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fixedSizeSamples;
        [MarshalAs(UnmanagedType.Bool)]
        public bool		temporalCompression;
        public int		sampleSize;
        public Guid		formatType;
        public IntPtr	unkPtr;
        public int		formatSize;
        public IntPtr	formatPtr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), ComVisible(false)]
    public struct FilterInfo		//  FILTER_INFO
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] // MAX_FILTER_NAME == 128
        public string		achName;
    //	[MarshalAs(UnmanagedType.IUnknown)]
    //	public object		pUnk;
        public IFilterGraph	pGraph;
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), ComVisible(false)]
    public struct PinInfo		//  PIN_INFO
    {
        public IBaseFilter pFilter;
        public PinDirection dir;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] // MAX_PIN_NAME == 128
        public string achName;
    }

    [ComVisible(false)]
    public class DsHlp
    {
        public const int OATRUE		= -1;
        public const int OAFALSE	= 0;
        public const int S_OK		= 0;
        public const int S_FALSE	= 1;
        public const uint E_FAIL = 0x80004005;
        public const uint VFW_E_NOT_CONNECTED = 0x80040209;
        public const uint VFW_E_DVD_DECNOTENOUGH = 0x8004027B;
        public const int VFW_S_DVD_NON_ONE_SEQUENTIAL = 0x00040280;
        public const int VFW_S_CANT_CUE = 0x00040268;
        public const int VFW_S_STATE_INTERMEDIATE = 0x00040237;
        public const int VFW_S_PARTIAL_RENDER = 0x00040242;
        public const int AM_RENDEREX_RENDERTOEXISTINGRENDERERS = 0x1;
        public const int ROTFLAGS_REGISTRATIONKEEPSALIVE = 0x1;
        public const int ROTFLAGS_ALLOWANYCLIENT = 0x2;

        public const short WAVE_FORMAT_PCM			= 0x0001;
        public const short WAVE_FORMAT_IEEE_FLOAT	= 0x0003;
        public const short WAVE_FORMAT_DRM         = 0x0009;
        public const short WAVE_FORMAT_MSNAUDIO    = 0x0032;
        public const short WAVE_FORMAT_MPEG        = 0x0050;
        public const short WAVE_FORMAT_MPEGLAYER3  = 0x0055;
        public const short WAVE_FORMAT_VOXWARE = 0x0062; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_BYTE_ALIGNED	= 0x0069; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_AC8		= 0x0070; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_AC10		= 0x0071; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_AC16		= 0x0072; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_AC20		= 0x0073; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_RT24		= 0x0074; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_RT29		= 0x0075; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_RT29HW	= 0x0076; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_VR12		= 0x0077; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_VR18		= 0x0078; /* Voxware Inc */
        public const short WAVE_FORMAT_VOXWARE_TQ40		= 0x0079; /* Voxware Inc */
        public const short WAVE_FORMAT_SOFTSOUND		= 0x0080; /* Softsound, Ltd. */
        public const short WAVE_FORMAT_VOXWARE_TQ60		= 0x0081; /* Voxware Inc */
        public const short WAVE_FORMAT_DOLBY_AC3_SPDIF	= 0x0092;
        public const short WAVE_FORMAT_AAC = 0x00FF;
        public const short WAVE_FORMAT_MSAUDIO1    = 0x0160;
        public const short WAVE_FORMAT_RAW_SPORT   = 0x0240;
        public const short WAVE_FORMAT_ESST_AC3    = 0x0241;
        
        public static bool FAILED(int HRESULT)
        {
            return HRESULT < 0;
        }

        public static bool SUCCEEDED(int HRESULT)
        {
            return HRESULT >= 0;
        }
    }
    
    [ComVisible(true), ComImport,
    GuidAttribute("56a86891-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPin
    {
        [PreserveSig]
        int Connect([In] IPin pReceivePin, [In] ref AMMediaType pmt);

        [PreserveSig]
        int ReceiveConnection([In] IPin pReceivePin, [In] ref AMMediaType pmt);

        [PreserveSig]
        int Disconnect();

        [PreserveSig]
        int ConnectedTo([Out] out IPin ppPin);

        [PreserveSig]
        int ConnectionMediaType([Out] out AMMediaType pmt);

        [PreserveSig]
        int QueryPinInfo([Out] out PinInfo pInfo);

        [PreserveSig]
        int QueryDirection(out PinDirection pPinDir);

        [PreserveSig]
        int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string Id);

        [PreserveSig]
        int QueryAccept([In] ref AMMediaType pmt);

        [PreserveSig]
        int EnumMediaTypes([Out] out IEnumMediaTypes ppEnum);

        [PreserveSig]
        int QueryInternalConnections(IntPtr apPin, [In, Out] ref int nPin);

        [PreserveSig]
        int EndOfStream();

        [PreserveSig]
        int BeginFlush();

        [PreserveSig]
        int EndFlush();

        [PreserveSig]
        int NewSegment(long tStart, long tStop, double dRate);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a8689f-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterGraph
    {
        [PreserveSig]
        int AddFilter([In] IBaseFilter pFilter, [In, MarshalAs(UnmanagedType.LPWStr)] string pName);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter);

        [PreserveSig]
        int ConnectDirect([In] IPin ppinOut, [In] IPin ppinIn,
            [In] ref AMMediaType pmt);

        [PreserveSig]
        int Reconnect([In] IPin ppin);

        [PreserveSig]
        int Disconnect([In] IPin ppin);

        [PreserveSig]
        int SetDefaultSyncSource();
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a86892-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumPins
    {
        [PreserveSig]
        int Next([In] int cPins, [Out] out IPin ppPins, out int pcFetched); // cPins must be 1

        [PreserveSig]
        int Skip([In] int cPins);
        
        void Reset();
        void Clone([Out] out IEnumPins ppEnum);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a86893-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumFilters
    {
        [PreserveSig]
        int Next([In] int cFilters, [Out] out IBaseFilter ppFilter,
            out int pcFetched); // cFilters must be 1

        [PreserveSig]
        int Skip([In] int cFilters);

        void Reset();
        void Clone([Out] out IEnumFilters ppEnum);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("0000010c-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        [PreserveSig]
        int GetClassID([Out] out Guid pClassID);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a86897-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IReferenceClock
    {
        [PreserveSig]
        int GetTime(out long pTime);

        [PreserveSig]
        int AdviseTime(long baseTime, long streamTime, IntPtr hEvent, out int pdwAdviseCookie);

        [PreserveSig]
        int AdvisePeriodic(long startTime, long periodTime, IntPtr hSemaphore, out int pdwAdviseCookie);

        [PreserveSig]
        int Unadvise(int dwAdviseCookie);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a86899-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaFilter
    {
        #region "IPersist Methods"
        [PreserveSig]
        int GetClassID([Out] out Guid pClassID);
        #endregion

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Run(long tStart);

        [PreserveSig]
        int GetState(int dwMilliSecsTimeout, out FilterState filtState);
    
        [PreserveSig]
        int SetSyncSource([In] IReferenceClock pClock);

        [PreserveSig]
        int GetSyncSource([Out] out IReferenceClock pClock);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a86895-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBaseFilter
    {
        #region "IPersist Methods"
        [PreserveSig]
        int GetClassID([Out] out Guid pClassID);
        #endregion

        #region "IMediaFilter Methods"
        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Run(long tStart);

        [PreserveSig]
        int GetState(uint dwMilliSecsTimeout, out int filtState);

        [PreserveSig]
        int SetSyncSource([In] IReferenceClock pClock);

        [PreserveSig]
        int GetSyncSource([Out] out IReferenceClock pClock);
        #endregion

        [PreserveSig]
        int EnumPins([Out] out IEnumPins ppEnum);

        [PreserveSig]
        int FindPin([In, MarshalAs(UnmanagedType.LPWStr)] string Id,
            [Out] out IPin ppPin);

        [PreserveSig]
        int QueryFilterInfo([Out] out FilterInfo pInfo);

        [PreserveSig]
        int JoinFilterGraph([In] IFilterGraph pGraph,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName);

        [PreserveSig]
        int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("89c31040-846b-11ce-97d3-00aa0055595a"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumMediaTypes
    {
        // to call this member function pass in the address of a pointer to a
        // media type. The interface will allocate the necessary AM_MEDIA_TYPE
        // structures and initialise them with the variable format block

        [PreserveSig]
        int Next([In] int cMediaTypes,         // place this many types... MUST BE 1
                [Out] out IntPtr ppMediaTypes,	// array of pointers (AM_MEDIA_TYPE*)
                out int pcFetched);			// actual count passed
                     
        [PreserveSig]
        int Skip([In] int cMediaTypes);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumMediaTypes ppEnum);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("B196B28B-BAB4-101A-B69C-00AA00341D07"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISpecifyPropertyPages
    {
        [PreserveSig]
        int GetPages(out CAUUID pPages);
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct CAUUID
    {
        public int		cElems;
        public IntPtr	pElems;
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868a6-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFileSourceFilter
    {
        [PreserveSig]
        int Load(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In] IntPtr pmt); // const AM_MEDIA_TYPE* should properly be marshaled as '[In] ref AMMediaType' but we want to be able to pass NULL which is allowed
        
        [PreserveSig]
        int GetCurFile( 
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName,
            [Out] out AMMediaType pmt);
    }
}