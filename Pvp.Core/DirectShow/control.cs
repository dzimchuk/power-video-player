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
 * AM_SEEKING_SEEKING_CAPABILITIES
 * AM_SEEKING_SEEKING_FLAGS
 * DsEvCode
 * IMediaSeeking
 * IMediaControl
 * IMediaEvent
 * IMediaEventEx
 * IBasicVideo2
 * IVideoWindow
 * IMediaPosition
 * IBasicAudio
*/

namespace Pvp.Core.DirectShow
{
    public enum DsEvCode
    {
        None,
        Complete			= 0x01,		// EC_COMPLETE
        UserAbort			= 0x02,		// EC_USERABORT
        ErrorAbort			= 0x03,		// EC_ERRORABORT
        Time				= 0x04,		// EC_TIME
        Repaint				= 0x05,		// EC_REPAINT
        StErrStopped		= 0x06,		// EC_STREAM_ERROR_STOPPED
        StErrStPlaying		= 0x07,		// EC_STREAM_ERROR_STILLPLAYING
        ErrorStPlaying		= 0x08,		// EC_ERROR_STILLPLAYING
        PaletteChanged		= 0x09,		// EC_PALETTE_CHANGED
        VideoSizeChanged	= 0x0a,		// EC_VIDEO_SIZE_CHANGED
        QualityChange		= 0x0b,		// EC_QUALITY_CHANGE
        ShuttingDown		= 0x0c,		// EC_SHUTTING_DOWN
        ClockChanged		= 0x0d,		// EC_CLOCK_CHANGED
        Paused				= 0x0e,		// EC_PAUSED
        OpeningFile			= 0x10,		// EC_OPENING_FILE
        BufferingData		= 0x11,		// EC_BUFFERING_DATA
        FullScreenLost		= 0x12,		// EC_FULLSCREEN_LOST
        Activate			= 0x13,		// EC_ACTIVATE
        NeedRestart			= 0x14,		// EC_NEED_RESTART
        WindowDestroyed		= 0x15,		// EC_WINDOW_DESTROYED
        DisplayChanged		= 0x16,		// EC_DISPLAY_CHANGED
        Starvation			= 0x17,		// EC_STARVATION
        OleEvent			= 0x18,		// EC_OLE_EVENT
        NotifyWindow		= 0x19,		// EC_NOTIFY_WINDOW

        STREAM_CONTROL_STOPPED = 0x1A,	// EC_STREAM_CONTROL_STOPPED
        STREAM_CONTROL_STARTED = 0x1B,	// EC_STREAM_CONTROL_STARTED
        END_OF_SEGMENT		= 0x1C,		// EC_END_OF_SEGMENT
        SEGMENT_STARTED		= 0x1D,		// EC_SEGMENT_STARTED
        LENGTH_CHANGED		= 0x1E,		// EC_LENGTH_CHANGED
        DEVICE_LOST			= 0x1f,		// EC_DEVICE_LOST
        STEP_COMPLETE		= 0x24,		// EC_STEP_COMPLETE

        RESERVED			= 0x25,		// Event code 25 is reserved for future use.

        TIMECODE_AVAILABLE	= 0x30,		// EC_TIMECODE_AVAILABLE
        EXTDEVICE_MODE_CHANGE = 0x31,	// EC_EXTDEVICE_MODE_CHANGE
        STATE_CHANGE		= 0x32,		// EC_STATE_CHANGE
        GRAPH_CHANGED		= 0x50,		// EC_GRAPH_CHANGED
        CLOCK_UNSET			= 0x51,		// EC_CLOCK_UNSET
        VMR_RENDERDEVICE_SET = 0x53,	// EC_VMR_RENDERDEVICE_SET
        VMR_SURFACE_FLIPPED	= 0x54,		// EC_VMR_SURFACE_FLIPPED
        VMR_RECONNECTION_FAILED	= 0x55,	// EC_VMR_RECONNECTION_FAILED
        PREPROCESS_COMPLETE	= 0x56,		// EC_PREPROCESS_COMPLETE
        CODECAPI_EVENT		= 0x57,		// EC_CODECAPI_EVENT

        WMT_EVENT_BASE		= 0x0251,	// EC_WMT_EVENT_BASE
        WMT_INDEX_EVENT		= 0x0251,	// EC_WMT_INDEX_EVENT
        WMT_EVENT			= 0x0251+1, // EC_WMT_EVENT

        BUILT				= 0x300,	// EC_BUILT
        UNBUILT				= 0x301,	// EC_UNBUILT
        // EC_ ....

        // DVDevCod.h
        DvdDomChange		= 0x101,	// EC_DVD_DOMAIN_CHANGE
        DvdTitleChange		= 0x102,	// EC_DVD_TITLE_CHANGE
        DvdChaptStart		= 0x103,	// EC_DVD_CHAPTER_START
        DvdAudioStChange	= 0x104,	// EC_DVD_AUDIO_STREAM_CHANGE

        DvdSubPicStChange	= 0x105,	// EC_DVD_SUBPICTURE_STREAM_CHANGE
        DvdAngleChange		= 0x106,	// EC_DVD_ANGLE_CHANGE
        DvdButtonChange		= 0x107,	// EC_DVD_BUTTON_CHANGE
        DvdValidUopsChange	= 0x108,	// EC_DVD_VALID_UOPS_CHANGE
        DvdStillOn			= 0x109,	// EC_DVD_STILL_ON
        DvdStillOff			= 0x10a,	// EC_DVD_STILL_OFF
        DvdCurrentTime		= 0x10b,	// EC_DVD_CURRENT_TIME
        DvdError			= 0x10c,	// EC_DVD_ERROR
        DvdWarning			= 0x10d,	// EC_DVD_WARNING
        DvdChaptAutoStop	= 0x10e,	// EC_DVD_CHAPTER_AUTOSTOP
        DvdNoFpPgc			= 0x10f,	// EC_DVD_NO_FP_PGC
        DvdPlaybRateChange	= 0x110,	// EC_DVD_PLAYBACK_RATE_CHANGE
        DvdParentalLChange	= 0x111,	// EC_DVD_PARENTAL_LEVEL_CHANGE
        DvdPlaybStopped		= 0x112,	// EC_DVD_PLAYBACK_STOPPED
        DvdAnglesAvail		= 0x113,	// EC_DVD_ANGLES_AVAILABLE
        DvdPeriodAStop		= 0x114,	// EC_DVD_PLAYPERIOD_AUTOSTOP
        DvdButtonAActivated	= 0x115,	// EC_DVD_BUTTON_AUTO_ACTIVATED
        DvdCmdStart			= 0x116,	// EC_DVD_CMD_START
        DvdCmdEnd			= 0x117,	// EC_DVD_CMD_END
        DvdDiscEjected		= 0x118,	// EC_DVD_DISC_EJECTED
        DvdDiscInserted		= 0x119,	// EC_DVD_DISC_INSERTED
        DvdCurrentHmsfTime	= 0x11a,	// EC_DVD_CURRENT_HMSF_TIME
        DvdKaraokeMode		= 0x11b		// EC_DVD_KARAOKE_MODE
    }
    
    [Flags, ComVisible(false)]
    public enum SeekingCapabilities		// AM_SEEKING_SEEKING_CAPABILITIES
    {
        CanSeekAbsolute		= 0x001,
        CanSeekForwards		= 0x002,
        CanSeekBackwards	= 0x004,
        CanGetCurrentPos	= 0x008,
        CanGetStopPos		= 0x010,
        CanGetDuration		= 0x020,
        CanPlayBackwards	= 0x040,
        CanDoSegments		= 0x080,
        Source				= 0x100		// Doesn't pass thru used to count segment ends
    }

    [Flags, ComVisible(false)]
    public enum SeekingFlags		// AM_SEEKING_SEEKING_FLAGS
    {
        NoPositioning			= 0x00,		// No change
        AbsolutePositioning		= 0x01,		// Position is supplied and is absolute
        RelativePositioning		= 0x02,		// Position is supplied and is relative
        IncrementalPositioning	= 0x03,		// (Stop) position relative to current, useful for seeking when paused (use +1)
        PositioningBitsMask		= 0x03,		// Useful mask
        SeekToKeyFrame			= 0x04,		// Just seek to key frame (performance gain)
        ReturnTime				= 0x08,		// Plug the media time equivalents back into the supplied LONGLONGs
        Segment					= 0x10,		// At end just do EC_ENDOFSEGMENT, don't do EndOfStream
        NoFlush					= 0x20		// Don't flush
    }

    [ComVisible(true), ComImport,
    GuidAttribute("36b73880-c2c8-11cf-8b46-00805f6cef60"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaSeeking
    {
        [PreserveSig]
        int GetCapabilities(out SeekingCapabilities pCapabilities);

        [PreserveSig]
        int CheckCapabilities([In, Out] ref SeekingCapabilities pCapabilities);

        [PreserveSig]
        int IsFormatSupported([In] ref Guid pFormat);

        [PreserveSig]
        int QueryPreferredFormat([Out] out Guid pFormat);

        [PreserveSig]
        int GetTimeFormat([Out] out Guid pFormat);

        [PreserveSig]
        int IsUsingTimeFormat([In] ref Guid pFormat);

        [PreserveSig]
        int SetTimeFormat([In] ref Guid pFormat);

        [PreserveSig]
        int GetDuration(out long pDuration);

        [PreserveSig]
        int GetStopPosition(out long pStop);

        [PreserveSig]
        int GetCurrentPosition(out long pCurrent);

        [PreserveSig]
        int ConvertTimeFormat(out long pTarget, [In] ref Guid pTargetFormat,
            long Source, [In] ref Guid pSourceFormat);

        [PreserveSig]
        int SetPositions(ref long pCurrent, SeekingFlags dwCurrentFlags,
            ref long pStop, SeekingFlags dwStopFlags);

        [PreserveSig]
        int GetPositions(out long pCurrent, out long pStop);

        [PreserveSig]
        int GetAvailable(out long pEarliest, out long pLatest);

        [PreserveSig]
        int SetRate(double dRate);

        [PreserveSig]
        int GetRate(out double pdRate);

        [PreserveSig]
        int GetPreroll(out long pllPreroll);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868b1-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaControl
    {
        [PreserveSig]
        int Run();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int GetState(int msTimeout, out FilterState pfs);

        [PreserveSig]
        int RenderFile(string strFilename); // BStr by default

        [PreserveSig]
        int AddSourceFilter([In] string strFilename,
            [Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int get_FilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int get_RegFilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int StopWhenReady();
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868b6-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaEvent
    {
        [PreserveSig]
        int GetEventHandle(out IntPtr hEvent);

        [PreserveSig]
        int GetEvent(out int lEventCode, out int lParam1, out int lParam2, int msTimeout);

        [PreserveSig]
        int WaitForCompletion(int msTimeout, out int pEvCode);

        [PreserveSig]
        int CancelDefaultHandling(int lEvCode);

        [PreserveSig]
        int RestoreDefaultHandling(int lEvCode);

        [PreserveSig]
        int FreeEventParams(int lEvCode, int lParam1, int lParam2);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868c0-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaEventEx
    {
        #region "IMediaEvent Methods"
        [PreserveSig]
        int GetEventHandle(out IntPtr hEvent);

        [PreserveSig]
        int GetEvent(out int lEventCode, out int lParam1, out int lParam2, int msTimeout);

        [PreserveSig]
        int WaitForCompletion(int msTimeout, out int pEvCode);

        [PreserveSig]
        int CancelDefaultHandling(int lEvCode);

        [PreserveSig]
        int RestoreDefaultHandling(int lEvCode);

        [PreserveSig]
        int FreeEventParams(int lEvCode, int lParam1, int lParam2);
        #endregion

        [PreserveSig]
        int SetNotifyWindow(IntPtr hwnd, int lMsg, IntPtr lInstanceData);

        [PreserveSig]
        int SetNotifyFlags(int lNoNotifyFlags);
        
        [PreserveSig]
        int GetNotifyFlags(out int lplNoNotifyFlags);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("329bb360-f6ea-11d1-9038-00a0c9697298"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBasicVideo2
    {
        [PreserveSig]
        int AvgTimePerFrame(out double pAvgTimePerFrame);

        [PreserveSig]
        int BitRate(out int pBitRate);

        [PreserveSig]
        int BitErrorRate(out int pBitRate);

        [PreserveSig]
        int VideoWidth(out int pVideoWidth);

        [PreserveSig]
        int VideoHeight(out int pVideoHeight);

        [PreserveSig]
        int put_SourceLeft(int SourceLeft);

        [PreserveSig]
        int get_SourceLeft(out int pSourceLeft);

        [PreserveSig]
        int put_SourceWidth(int SourceWidth);

        [PreserveSig]
        int get_SourceWidth(out int pSourceWidth);

        [PreserveSig]
        int put_SourceTop(int SourceTop);

        [PreserveSig]
        int get_SourceTop(out int pSourceTop);

        [PreserveSig]
        int put_SourceHeight(int SourceHeight);

        [PreserveSig]
        int get_SourceHeight(out int pSourceHeight);

        [PreserveSig]
        int put_DestinationLeft(int DestinationLeft);

        [PreserveSig]
        int get_DestinationLeft(out int pDestinationLeft);

        [PreserveSig]
        int put_DestinationWidth(int DestinationWidth);

        [PreserveSig]
        int get_DestinationWidth(out int pDestinationWidth);

        [PreserveSig]
        int put_DestinationTop(int DestinationTop);

        [PreserveSig]
        int get_DestinationTop(out int pDestinationTop);

        [PreserveSig]
        int put_DestinationHeight(int DestinationHeight);

        [PreserveSig]
        int get_DestinationHeight(out int pDestinationHeight);

        [PreserveSig]
        int SetSourcePosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetSourcePosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int SetDefaultSourcePosition();

        [PreserveSig]
        int SetDestinationPosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetDestinationPosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int SetDefaultDestinationPosition();

        [PreserveSig]
        int GetVideoSize(out int pWidth, out int pHeight);

        [PreserveSig]
        int GetVideoPaletteEntries(int StartIndex, int Entries, out int pRetrieved, IntPtr pPalette);

        [PreserveSig]
        int GetCurrentImage(ref int pBufferSize, IntPtr pDIBImage);

        [PreserveSig]
        int IsUsingDefaultSource();

        [PreserveSig]
        int IsUsingDefaultDestination();

        [PreserveSig]
        int GetPreferredAspectRatio(out int plAspectX, out int plAspectY);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868b4-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IVideoWindow
    {
        [PreserveSig]
        int put_Caption(string caption);

        [PreserveSig]
        int get_Caption([Out] out string caption);

        [PreserveSig]
        int put_WindowStyle(int windowStyle);

        [PreserveSig]
        int get_WindowStyle(out int windowStyle);

        [PreserveSig]
        int put_WindowStyleEx(int windowStyleEx);

        [PreserveSig]
        int get_WindowStyleEx(out int windowStyleEx);
        
        [PreserveSig]
        int put_AutoShow(int autoShow);

        [PreserveSig]
        int get_AutoShow(out int autoShow);
        
        [PreserveSig]
        int put_WindowState(int windowState);

        [PreserveSig]
        int get_WindowState(out int windowState);

        [PreserveSig]
        int put_BackgroundPalette(int backgroundPalette);

        [PreserveSig]
        int get_BackgroundPalette(out int backgroundPalette);
        
        [PreserveSig]
        int put_Visible(int visible);

        [PreserveSig]
        int get_Visible(out int visible);
        
        [PreserveSig]
        int put_Left(int left);

        [PreserveSig]
        int get_Left(out int left);
        
        [PreserveSig]
        int put_Width(int width);

        [PreserveSig]
        int get_Width(out int width);
        
        [PreserveSig]
        int put_Top(int top);

        [PreserveSig]
        int get_Top(out int top);
        
        [PreserveSig]
        int put_Height(int height);

        [PreserveSig]
        int get_Height(out int height);
        
        [PreserveSig]
        int put_Owner(IntPtr owner);

        [PreserveSig]
        int get_Owner(out IntPtr owner);
        
        [PreserveSig]
        int put_MessageDrain(IntPtr drain);

        [PreserveSig]
        int get_MessageDrain(out IntPtr drain);
        
        [PreserveSig]
        int get_BorderColor(out int color);

        [PreserveSig]
        int put_BorderColor(int color);
        
        [PreserveSig]
        int get_FullScreenMode(out int fullScreenMode);

        [PreserveSig]
        int put_FullScreenMode(int fullScreenMode);
        
        [PreserveSig]
        int SetWindowForeground(int focus);
        
        [PreserveSig]
        int NotifyOwnerMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        
        [PreserveSig]
        int SetWindowPosition(int left, int top, int width, int height);
        
        [PreserveSig]
        int GetWindowPosition(out int left, out int top, out int width, out int height);
        
        [PreserveSig]
        int GetMinIdealImageSize(out int width, out int height);
        
        [PreserveSig]
        int GetMaxIdealImageSize(out int width, out int height);
        
        [PreserveSig]
        int GetRestorePosition(out int left, out int top, out int width, out int height);
        
        [PreserveSig]
        int HideCursor(int hideCursor);
        
        [PreserveSig]
        int IsCursorHidden(out int hideCursor);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868b2-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaPosition
    {
        [PreserveSig]
        int get_Duration(out double pLength); // typedef double REFTIME;

        [PreserveSig]
        int put_CurrentPosition(double llTime);

        [PreserveSig]
        int get_CurrentPosition(out double pllTime);

        [PreserveSig]
        int get_StopTime(out double pllTime);

        [PreserveSig]
        int put_StopTime(double llTime);

        [PreserveSig]
        int get_PrerollTime(out double pllTime);

        [PreserveSig]
        int put_PrerollTime(double llTime);

        [PreserveSig]
        int put_Rate(double dRate);

        [PreserveSig]
        int get_Rate(out double pdRate);

        [PreserveSig]
        int CanSeekForward(out int pCanSeekForward);

        [PreserveSig]
        int CanSeekBackward(out int pCanSeekBackward);
    }

    [ComVisible(true), ComImport,
    GuidAttribute("56a868b3-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBasicAudio
    {
        [PreserveSig]
        int put_Volume(int lVolume);
        [PreserveSig]
        int get_Volume(out int plVolume);

        [PreserveSig]
        int put_Balance(int lBalance);

        [PreserveSig]
        int get_Balance(out int plBalance);
    }
}