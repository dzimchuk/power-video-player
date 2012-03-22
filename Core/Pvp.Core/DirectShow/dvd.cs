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
using Pvp.Core.Native;

/* 
 * DVD_ERROR
 * DVD_WARNING
 * DVD_VIDEO_COMPRESSION
 * DVD_AUDIO_FORMAT
 * AM_DVD_GRAPH_FLAGS
 * AM_DVD_STREAM_FLAGS
 * AM_DVD_RENDERSTATUS
 * DVD_CMD_FLAGS
 * DVD_HMSF_TIMECODE
 * DVD_MENU_ID
 * DVD_RELATIVE_BUTTON
 * DVD_KARAOKE_DOWNMIX
 * DVD_PREFERRED_DISPLAY_MODE
 * DVD_OPTION_FLAG
 * DVD_AUDIO_LANG_EXT
 * DVD_SUBPICTURE_LANG_EXT
 * DVD_DOMAIN
 * DVD_PLAYBACK_LOCATION2
 * VALID_UOP_FLAG
 * DVD_MenuAttributes
 * DVD_VideoAttributes
 * DVD_AudioAttributes
 * DVD_AUDIO_APPMODE
 * DVD_SubpictureAttributes
 * DVD_SUBPICTURE_TYPE
 * DVD_SUBPICTURE_CODING
 * DVD_TitleAttributes
 * DVD_TITLE_APPMODE
 * DVD_MultichannelAudioAttributes
 * DVD_MUA_MixingInfo
 * DVD_MUA_Coeff
 * DVD_KaraokeAttributes
 * DVD_KARAOKE_ASSIGNMENT
 * DVD_DISC_SIDE
 * DVD_TextCharSet
 * DVD_TextStringType
 * DVD_DECODER_CAPS
 * IDvdGraphBuilder
 * IDvdCmd
 * IDvdState
 * IDvdControl2
 * IDvdInfo2
 * 
 * AM_LINE21_CCLEVEL
 * AM_LINE21_CCSERVICE
 * AM_LINE21_CCSTATE
 * AM_LINE21_CCSTYLE
 * AM_LINE21_DRAWBGMODE
 * IAMLine21Decoder
*/
namespace Pvp.Core.DirectShow
{
    [ComVisible(false)]
    public class DvdHlp
    {
        public const int AMINTERLACE_IsInterlaced            = 0x00000001;  // if 0, other interlace bits are irrelevent
        public const int AMINTERLACE_1FieldPerSample         = 0x00000002;  // else 2 fields per media sample
        public const int AMINTERLACE_Field1First             = 0x00000004;  // else Field 2 is first;  top field in PAL is field 1, top field in NTSC is field 2?
        public const int AMINTERLACE_UNUSED                  = 0x00000008;  //
        public const int AMINTERLACE_FieldPatternMask        = 0x00000030;  // use this mask with AMINTERLACE_FieldPat*
        public const int AMINTERLACE_FieldPatField1Only      = 0x00000000;  // stream never contains a Field2
        public const int AMINTERLACE_FieldPatField2Only      = 0x00000010;  // stream never contains a Field1
        public const int AMINTERLACE_FieldPatBothRegular     = 0x00000020;  // There will be a Field2 for every Field1 (required for Weave?)
        public const int AMINTERLACE_FieldPatBothIrregular   = 0x00000030;  // Random pattern of Field1s and Field2s
        public const int AMINTERLACE_DisplayModeMask         = 0x000000c0;
        public const int AMINTERLACE_DisplayModeBobOnly      = 0x00000000;
        public const int AMINTERLACE_DisplayModeWeaveOnly    = 0x00000040;
        public const int AMINTERLACE_DisplayModeBobOrWeave   = 0x00000080;

        // DVD_KARAOKE_CONTENTS
        public const short DVD_Karaoke_GuideVocal1  = 0x0001;
        public const short DVD_Karaoke_GuideVocal2  = 0x0002;
        public const short DVD_Karaoke_GuideMelody1 = 0x0004;
        public const short DVD_Karaoke_GuideMelody2 = 0x0008;
        public const short DVD_Karaoke_GuideMelodyA = 0x0010;
        public const short DVD_Karaoke_GuideMelodyB = 0x0020;
        public const short DVD_Karaoke_SoundEffectA = 0x0040;
        public const short DVD_Karaoke_SoundEffectB = 0x0080;

        // DVD_AUDIO_CAPS
        public const int DVD_AUDIO_CAPS_AC3		= 0x00000001;
        public const int DVD_AUDIO_CAPS_MPEG2		= 0x00000002;
        public const int DVD_AUDIO_CAPS_LPCM		= 0x00000004;
        public const int DVD_AUDIO_CAPS_DTS		= 0x00000008;
        public const int DVD_AUDIO_CAPS_SDDS		= 0x00000010;
    }

    [ComVisible(false)]
    public enum DVD_ERROR 
    {
        DVD_ERROR_Unexpected                          = 1,
        DVD_ERROR_CopyProtectFail                     = 2,   
        DVD_ERROR_InvalidDVD1_0Disc                   = 3,
        DVD_ERROR_InvalidDiscRegion                   = 4,
        DVD_ERROR_LowParentalLevel                    = 5,
        DVD_ERROR_MacrovisionFail                     = 6,
        DVD_ERROR_IncompatibleSystemAndDecoderRegions = 7,
        DVD_ERROR_IncompatibleDiscAndDecoderRegions   = 8
    }

    [ComVisible(false)]
    public enum DVD_WARNING 
    {
        DVD_WARNING_InvalidDVD1_0Disc=1,
        DVD_WARNING_FormatNotSupported=2,
        DVD_WARNING_IllegalNavCommand=3,
        DVD_WARNING_Open = 4,
        DVD_WARNING_Seek = 5,
        DVD_WARNING_Read = 6
    }

    [ComVisible(false)]
    public enum DVD_VIDEO_COMPRESSION
    {
        DVD_VideoCompression_Other  = 0,
        DVD_VideoCompression_MPEG1  = 1,
        DVD_VideoCompression_MPEG2  = 2
    }

    [ComVisible(false)]
    public enum DVD_AUDIO_FORMAT 
    {
        DVD_AudioFormat_AC3       = 0,
        DVD_AudioFormat_MPEG1     = 1,
        DVD_AudioFormat_MPEG1_DRC = 2,    
        DVD_AudioFormat_MPEG2     = 3,
        DVD_AudioFormat_MPEG2_DRC = 4,
        DVD_AudioFormat_LPCM      = 5,
        DVD_AudioFormat_DTS       = 6,
        DVD_AudioFormat_SDDS      = 7,
        DVD_AudioFormat_Other     = 8
    }

    [Flags, ComVisible(false)]
    public enum AM_DVD_GRAPH_FLAGS 
    {
        AM_DVD_HWDEC_PREFER =  0x01,   // default 
        AM_DVD_HWDEC_ONLY   =  0x02,
        AM_DVD_SWDEC_PREFER =  0x04,
        AM_DVD_SWDEC_ONLY   =  0x08,
        AM_DVD_NOVPE        = 0x100,
        AM_DVD_VMR9_ONLY    = 0x800    // only use VMR9 (otherwise fail) for rendering
    }

    [Flags, ComVisible(false)]
    public enum AM_DVD_STREAM_FLAGS 
    {
        AM_DVD_STREAM_VIDEO  = 0x01,
        AM_DVD_STREAM_AUDIO  = 0x02,
        AM_DVD_STREAM_SUBPIC = 0x04
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct AM_DVD_RENDERSTATUS
    {
        public int hrVPEStatus;			// VPE mixing error code (0 => success)
        [MarshalAs(UnmanagedType.Bool)]
        public bool bDvdVolInvalid;		// Is specified DVD volume invalid?
        [MarshalAs(UnmanagedType.Bool)]
        public bool bDvdVolUnknown;		// Is DVD volume to be played not specified/not found?
        [MarshalAs(UnmanagedType.Bool)]
        public bool bNoLine21In;		// video decoder doesn't produce line21 (CC) data
        [MarshalAs(UnmanagedType.Bool)]
        public bool bNoLine21Out;		// can't show decoded line21 data as CC on video
        public int iNumStreams;			// number of DVD streams to render
        public int iNumStreamsFailed;	// number of streams failed to render
        public AM_DVD_STREAM_FLAGS dwFailedStreamsFlag; // combination of flags to indicate failed streams
    }

    [Flags, ComVisible(false)]
    public enum DVD_CMD_FLAGS
    {
        DVD_CMD_FLAG_None				= 0x00000000,
        DVD_CMD_FLAG_Flush				= 0x00000001,
        DVD_CMD_FLAG_SendEvents			= 0x00000002,
        DVD_CMD_FLAG_Block				= 0x00000004,
        DVD_CMD_FLAG_StartWhenRendered  = 0x00000008,
        DVD_CMD_FLAG_EndAfterRendered   = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_HMSF_TIMECODE 
    {
        public byte bHours;
        public byte bMinutes;
        public byte bSeconds;
        public byte bFrames;
    }

    [ComVisible(false)]
    public enum DVD_MENU_ID 
    {
        DVD_MENU_Title      = 2,     
        DVD_MENU_Root       = 3, 
        DVD_MENU_Subpicture = 4,
        DVD_MENU_Audio      = 5,    
        DVD_MENU_Angle      = 6,     
        DVD_MENU_Chapter    = 7    
    }

    [ComVisible(false)]
    public enum DVD_RELATIVE_BUTTON 
    {
        DVD_Relative_Upper = 1,
        DVD_Relative_Lower = 2,
        DVD_Relative_Left =  3,
        DVD_Relative_Right = 4
    }

    [ComVisible(false)]
    public enum DVD_KARAOKE_DOWNMIX 
    {
        DVD_Mix_0to0 = 0x0001,
        DVD_Mix_1to0 = 0x0002,
        DVD_Mix_2to0 = 0x0004,
        DVD_Mix_3to0 = 0x0008,
        DVD_Mix_4to0 = 0x0010,
        DVD_Mix_Lto0 = 0x0020,
        DVD_Mix_Rto0 = 0x0040,

        DVD_Mix_0to1 = 0x0100,
        DVD_Mix_1to1 = 0x0200,
        DVD_Mix_2to1 = 0x0400,
        DVD_Mix_3to1 = 0x0800,
        DVD_Mix_4to1 = 0x1000,
        DVD_Mix_Lto1 = 0x2000,
        DVD_Mix_Rto1 = 0x4000
    }

    [ComVisible(false)]
    public enum DVD_PREFERRED_DISPLAY_MODE 
    {
        DISPLAY_CONTENT_DEFAULT = 0,
        DISPLAY_16x9 = 1,
        DISPLAY_4x3_PANSCAN_PREFERRED = 2, 
        DISPLAY_4x3_LETTERBOX_PREFERRED = 3
    }

    [ComVisible(false)]
    public enum DVD_OPTION_FLAG
    {
        DVD_ResetOnStop               = 1,
        DVD_NotifyParentalLevelChange = 2,
        DVD_HMSF_TimeCodeEvents       = 3,
        DVD_AudioDuringFFwdRew        = 4
    }

    [ComVisible(false)]
    public enum DVD_AUDIO_LANG_EXT 
    {
        DVD_AUD_EXT_NotSpecified        = 0,
        DVD_AUD_EXT_Captions            = 1,
        DVD_AUD_EXT_VisuallyImpaired    = 2,
        DVD_AUD_EXT_DirectorComments1   = 3,
        DVD_AUD_EXT_DirectorComments2   = 4
    }

    [ComVisible(false)]
    public enum DVD_SUBPICTURE_LANG_EXT 
    {
        DVD_SP_EXT_NotSpecified     = 0,
        DVD_SP_EXT_Caption_Normal   = 1,
        DVD_SP_EXT_Caption_Big      = 2,
        DVD_SP_EXT_Caption_Children = 3,
        DVD_SP_EXT_CC_Normal        = 5,
        DVD_SP_EXT_CC_Big           = 6,
        DVD_SP_EXT_CC_Children      = 7,
        DVD_SP_EXT_Forced           = 9,
        DVD_SP_EXT_DirectorComments_Normal   =13,
        DVD_SP_EXT_DirectorComments_Big      =14,
        DVD_SP_EXT_DirectorComments_Children =15
    }

    [ComVisible(false)]
    public enum DVD_DOMAIN 
    {
        DVD_DOMAIN_FirstPlay = 1,
        DVD_DOMAIN_VideoManagerMenu, 
        DVD_DOMAIN_VideoTitleSetMenu,  
        DVD_DOMAIN_Title,         
        DVD_DOMAIN_Stop     
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_PLAYBACK_LOCATION2 
    {
        //
        // TitleNum & ChapterNum or TitleNum & TimeCode are sufficient to save 
        // playback location for One_Sequential_PGC_Titles.
        //
        public int   TitleNum;			// title number for whole disc (TTN not VTS_TTN)
        public int   ChapterNum;		// part-of-title number with title. 0xffffffff if not Once_Sequential_PGC_Title
        public DVD_HMSF_TIMECODE   TimeCode;   // use DVD_TIMECODE for current playback time.
        public int   TimeCodeFlags;	// union of DVD_TIMECODE_EVENT_FLAGS
    }

    [Flags, ComVisible(false)]
    public enum VALID_UOP_FLAG
    {
        //
        // Annex J User Functions ---
        //
        UOP_FLAG_Play_Title_Or_AtTime           = 0x00000001,   // Title_Or_Time_Play
        UOP_FLAG_Play_Chapter                   = 0x00000002,   // Chapter_Search_Or_Play
        UOP_FLAG_Play_Title                     = 0x00000004,   // Title_Play
        UOP_FLAG_Stop                           = 0x00000008,   // Stop
        UOP_FLAG_ReturnFromSubMenu              = 0x00000010,   // GoUp
        UOP_FLAG_Play_Chapter_Or_AtTime         = 0x00000020,   // Time_Or_Chapter_Search
        UOP_FLAG_PlayPrev_Or_Replay_Chapter     = 0x00000040,   // Prev_Or_Top_PG_Search
        UOP_FLAG_PlayNext_Chapter               = 0x00000080,   // Next_PG_Search
        UOP_FLAG_Play_Forwards                  = 0x00000100,   // Forward_Scan
        UOP_FLAG_Play_Backwards                 = 0x00000200,   // Backward_Scan
        UOP_FLAG_ShowMenu_Title                 = 0x00000400,   // Title_Menu_Call
        UOP_FLAG_ShowMenu_Root                  = 0x00000800,   // Root_Menu_Call
        UOP_FLAG_ShowMenu_SubPic                = 0x00001000,   // SubPic_Menu_Call
        UOP_FLAG_ShowMenu_Audio                 = 0x00002000,   // Audio_Menu_Call
        UOP_FLAG_ShowMenu_Angle                 = 0x00004000,   // Angle_Menu_Call
        UOP_FLAG_ShowMenu_Chapter               = 0x00008000,   // Chapter_Menu_Call
        UOP_FLAG_Resume                         = 0x00010000,   // Resume
        UOP_FLAG_Select_Or_Activate_Button      = 0x00020000,   // Button_Select_Or_Activate
        UOP_FLAG_Still_Off                      = 0x00040000,   // Still_Off
        UOP_FLAG_Pause_On                       = 0x00080000,   // Pause_On
        UOP_FLAG_Select_Audio_Stream            = 0x00100000,   // Audio_Stream_Change
        UOP_FLAG_Select_SubPic_Stream           = 0x00200000,   // SubPic_Stream_Change
        UOP_FLAG_Select_Angle                   = 0x00400000,   // Angle_Change
        UOP_FLAG_Select_Karaoke_Audio_Presentation_Mode = 0x00800000, // Karaoke_Audio_Pres_Mode_Change
        UOP_FLAG_Select_Video_Mode_Preference           = 0x01000000  // Video_Pres_Mode_Change
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_MenuAttributes 
    {
        // for VMG only 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public int[]						fCompatibleRegion;  // indeces 0..7 correspond to regions 1..8

        // Attributes about the main menu (VMGM or VTSM)
        public DVD_VideoAttributes			VideoAttributes;

        [MarshalAs(UnmanagedType.Bool)]
        public bool							fAudioPresent;
        public DVD_AudioAttributes			AudioAttributes;

        [MarshalAs(UnmanagedType.Bool)]
        public bool							fSubpicturePresent;
        public DVD_SubpictureAttributes		SubpictureAttributes;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_VideoAttributes
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fPanscanPermitted;      // if a 4x3 display, can be shown as PanScan
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fLetterboxPermitted;    // if a 4x3 display, can be shown as Letterbox
        public int		ulAspectX;              // 4x3 or 16x9
        public int		ulAspectY;
        public int		ulFrameRate;            // 50hz or 60hz
        public int		ulFrameHeight;          // 525 (60hz) or 625 (50hz)
        public DVD_VIDEO_COMPRESSION   Compression;// MPEG1 or MPEG2

        [MarshalAs(UnmanagedType.Bool)]
        public bool		fLine21Field1InGOP;     // true if there is user data in field 1 of GOP of video stream
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fLine21Field2InGOP;     // true if there is user data in field 1 of GOP of video stream

        public int		ulSourceResolutionX;    // X source resolution (352,704, or 720)
        public int		ulSourceResolutionY;    // Y source resolution (240,480, 288 or 576)

        [MarshalAs(UnmanagedType.Bool)]
        public bool		fIsSourceLetterboxed;   // subpictures and highlights (e.g. subtitles or menu buttons) are only
        // displayed in the active video area and cannot be displayed in the top/bottom 'black' bars
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fIsFilmMode;          // for 625/50hz systems, is film mode (true) or camera mode (false) 
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_AudioAttributes
    {
        public DVD_AUDIO_APPMODE	AppMode;
        public byte					AppModeData;            
        public DVD_AUDIO_FORMAT		AudioFormat;            // Use GetKaraokeAttributes()
        public int					Language;               // 0 if no language is present
        public DVD_AUDIO_LANG_EXT	LanguageExtension;      // (captions, if for children etc)
        [MarshalAs(UnmanagedType.Bool)]
        public bool					fHasMultichannelInfo;   // multichannel attributes are present (Use GetMultiChannelAudioAttributes())
        public int					dwFrequency;            // in hertz (48k, 96k)
        public byte					bQuantization;          // resolution (16, 20, 24 bits etc), 0 is unknown
        public byte					bNumberOfChannels;      // 5.1 AC3 has 6 channels
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public int[]				dwReserved;
    }

    [ComVisible(false)]
    public enum DVD_AUDIO_APPMODE
    {
        DVD_AudioMode_None     = 0, // no special mode
        DVD_AudioMode_Karaoke  = 1,
        DVD_AudioMode_Surround = 2, 
        DVD_AudioMode_Other    = 3 
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_SubpictureAttributes
    {
        public DVD_SUBPICTURE_TYPE     Type;
        public DVD_SUBPICTURE_CODING   CodingMode;
        public int                    Language;
        public DVD_SUBPICTURE_LANG_EXT LanguageExtension;
    }

    [ComVisible(false)]
    public enum DVD_SUBPICTURE_TYPE
    {
        DVD_SPType_NotSpecified = 0,
        DVD_SPType_Language     = 1,
        DVD_SPType_Other        = 2
    }

    [ComVisible(false)]
    public enum DVD_SUBPICTURE_CODING
    {
        DVD_SPCoding_RunLength    = 0,
        DVD_SPCoding_Extended     = 1,
        DVD_SPCoding_Other        = 2
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_TitleAttributes 
    {
        // for Titles
        public DVD_TITLE_APPMODE					AppMode;

        // Attributes about the 'main' video of the menu or title
        public DVD_VideoAttributes					VideoAttributes;

        public int									ulNumberOfAudioStreams;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public DVD_AudioAttributes[]				AudioAttributes;
        // present if the multichannel bit is set in the corresponding stream's audio attributes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public DVD_MultichannelAudioAttributes[]	MultichannelAudioAttributes;

        public int									ulNumberOfSubpictureStreams;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
        public DVD_SubpictureAttributes[]			SubpictureAttributes;
    }

    [ComVisible(false)]
    public enum DVD_TITLE_APPMODE
    {
        DVD_AppMode_Not_Specified = 0, // no special mode
        DVD_AppMode_Karaoke  = 1,
        DVD_AppMode_Other    = 3
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_MultichannelAudioAttributes
    {
        // actual Data for each data stream
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public DVD_MUA_MixingInfo[]	Info;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public DVD_MUA_Coeff[]		Coeff;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_MUA_MixingInfo
    {   
        // surround sound mixing information applied when:
        // AppMode = DVD_AudioMode_Surround
        // AudioFormat = DVD_AudioFormat_LPCM,
        // fHasMultichannelInfo=1 modes are all on
        //
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fMixTo0;
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fMixTo1;

        //
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fMix0InPhase;
        [MarshalAs(UnmanagedType.Bool)]
        public bool		fMix1InPhase;

        public int		dwSpeakerPosition;  // see ksmedia.h: SPEAKER_FRONT_LEFT, SPEAKER_FRONT_RIGHT, etc
    }

    //  The alpha coeff is used to mix to ACH0 and beta is used to mix to ACH1
    //
    //  In general:
    //      ACH0 = coeff[0].alpha * value[0] + coeff[1].alpha * value[1] + ... 
    //      ACH1 = coeff[0].beta * value[0]  + coeff[1].beta * value[1] + ... 
    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_MUA_Coeff
    {
        public double	log2_alpha; // actual coeff = 2^alpha
        public double	log2_beta;  // actual coeff = 2^beta
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_KaraokeAttributes
    {
        public byte						bVersion;
        [MarshalAs(UnmanagedType.Bool)]
        public bool						fMasterOfCeremoniesInGuideVocal1;
        [MarshalAs(UnmanagedType.Bool)]
        public bool						fDuet;  // false = solo
        public DVD_KARAOKE_ASSIGNMENT	ChannelAssignment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public short[]					wChannelContents; // logical OR of DVD_KARAOKE_CONTENTS  
    }

    [ComVisible(false)]
    public enum DVD_KARAOKE_ASSIGNMENT
    {
        DVD_Assignment_reserved0   = 0,
        DVD_Assignment_reserved1   = 1,
        DVD_Assignment_LR    = 2,   // left right
        DVD_Assignment_LRM   = 3,   // left right middle
        DVD_Assignment_LR1   = 4,   // left right audio1
        DVD_Assignment_LRM1  = 5,   // left right middle audio1
        DVD_Assignment_LR12  = 6,   // left right audio1 audio2
        DVD_Assignment_LRM12 = 7        // left right middle audio1 audio2
    }

    [ComVisible(false)]
    public enum DVD_DISC_SIDE 
    {
        DVD_SIDE_A = 1,
        DVD_SIDE_B = 2
    }

    [ComVisible(false)]
    public enum DVD_TextCharSet 
    {
        DVD_CharSet_Unicode                       = 0,
        DVD_CharSet_ISO646                        = 1,
        DVD_CharSet_JIS_Roman_Kanji               = 2,
        DVD_CharSet_ISO8859_1                     = 3,
        DVD_CharSet_ShiftJIS_Kanji_Roman_Katakana = 4
    }

    [ComVisible(false)]
    public enum DVD_TextStringType 
    {
        // disc structure (0x00..0x0f)
        DVD_Struct_Volume               = 0x01, 
        DVD_Struct_Title                = 0x02, 
        DVD_Struct_ParentalID           = 0x03,
        DVD_Struct_PartOfTitle          = 0x04,
        DVD_Struct_Cell                 = 0x05,
        // stream (0x10..0x1f)
        DVD_Stream_Audio                = 0x10,
        DVD_Stream_Subpicture           = 0x11,
        DVD_Stream_Angle                = 0x12,
        // channel in stream (0x20..0x2f)
        DVD_Channel_Audio               = 0x20,

        // Application information
        // General (0x30..0x37)
        DVD_General_Name                = 0x30,
        DVD_General_Comments            = 0x31,

        // Title (0x38..0x3f)
        DVD_Title_Series                = 0x38,
        DVD_Title_Movie                 = 0x39,
        DVD_Title_Video                 = 0x3a,
        DVD_Title_Album                 = 0x3b,
        DVD_Title_Song                  = 0x3c,
        DVD_Title_Other                 = 0x3f,

        // Title (sub) (0x40..0x47)
        DVD_Title_Sub_Series            = 0x40,
        DVD_Title_Sub_Movie             = 0x41,
        DVD_Title_Sub_Video             = 0x42,
        DVD_Title_Sub_Album             = 0x43,
        DVD_Title_Sub_Song              = 0x44,
        DVD_Title_Sub_Other             = 0x47,

        // Title (original) (0x48..0x4f)
        DVD_Title_Orig_Series           = 0x48,
        DVD_Title_Orig_Movie            = 0x49,
        DVD_Title_Orig_Video            = 0x4a,
        DVD_Title_Orig_Album            = 0x4b,
        DVD_Title_Orig_Song             = 0x4c,
        DVD_Title_Orig_Other            = 0x4f,

        // Other info (0x50..0x57)
        DVD_Other_Scene                 = 0x50,
        DVD_Other_Cut                   = 0x51,
        DVD_Other_Take                  = 0x52

        // Language     0x58..0x5b
        // Work         0x5c..0x6b
        // Character    0x6c..0x8f
        // Data         0x90..0x93
        // Karaoke      0x94..0x9b
        // Category     0x9c..0x9f
        // Lyrics       0xa0..0xa3
        // Document     0xa4..0xa7
        // Others       0xa8..0xab
        // Reserved     0xac..0xaf
        // Admin        0xb0..0xb7
        // more admin   0xb8..0xc0
        // Reserved     0xd0..0xdf
        // vendor       0xe0..0xef
        // extension    0xf0..0xf7
        // reserved     0xf8..0xff
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    public struct DVD_DECODER_CAPS
    {
        public int   dwSize;            // size of this struct
        public int   dwAudioCaps;       // bits indicating audio support (AC3, DTS, SDDS, LPCM etc.) of decoder
        public double  dFwdMaxRateVideo;  // max data rate for video going forward
        public double  dFwdMaxRateAudio;  // ...  ..   ..  ... audio  ...    ...
        public double  dFwdMaxRateSP;     // ...  ..   ..  ...   SP   ...    ...
        public double  dBwdMaxRateVideo;  // if smooth reverse is not available, this will be set to 0
        public double  dBwdMaxRateAudio;  //   -- ditto --
        public double  dBwdMaxRateSP;     //   -- ditto --
        public int   dwRes1;            // reserved for future expansion
        public int   dwRes2;            //   -- ditto --
        public int   dwRes3;            //   -- ditto --
        public int   dwRes4;            //   -- ditto --
    }

    [ComVisible(true), ComImport,
    GuidAttribute("FCC152B6-F372-11d0-8E00-00C04FD7C08B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDvdGraphBuilder
    {
        // Returns the IGraphBuilder interface for the filtergraph used by the
        // CDvdGraphBuilder object.
        // Remember to *ppGB->Release() when you're done with it
        [PreserveSig]
        int GetFiltergraph([Out] out IGraphBuilder ppGB);

        // Gets specific interface pointers in the DVD-Video playback graph to 
        // make DVD-Video playback development easier.
        // It helps get the following interfaces to control playback/show CC/
        // position window/control volume etc:
        // - IDvdControl, IDvdInfo
        // - IAMLine21Decoder
        // - IVideoWindow, IBasicVideo
        // - IBasicAudio
        // This method will return 
        // a) E_INVALIDARG if ppvIF is invalid
        // b) E_NOINTERFACE if riid is an IID we don't know about
        // c) VFW_E_DVD_GRAPHNOTREADY if the graph has not been built through 
        //    RenderDvdVideoVolume() yet.
        // Remember to *ppvIF->Release() when you're done with it
        [PreserveSig]
        int GetDvdInterface([In] ref Guid riid,    // IID of the interface required
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppvIF);   // returns pointer to the required interface
        
        // Builds a filter graph according to user specs for playing back a
        // DVD-Video volume.
        // This method returns S_FALSE if
        // 1.  the graph has been either built, but either
        //     a) VPE mixing doesn't work (app didn't use AM_DVD_NOVPE flag)
        //     b) video decoder doesn't produce line21 data
        //     c) line21 data couldn't be rendered (decoding/mixing problem)
        //     d) the call specified an invalid volume path or DVD Nav couldn't
        //        locate any DVD-Video volume to be played.
        // 2.  some streams didn't render (completely), but the others have
        //     been rendered so that the volume can be partially played back.
        // The status is indicated through the fields of the pStatus (out)
        // parameter.
        // About 1(a), the app will have enough info to tell the user that the
        // video won't be visible unless a TV is connected to the NTSC out 
        // port of the DVD decoder (presumably HW in this case).
        // For case 1(b) & (c), the app "can" put up a warning/informative message
        // that closed captioning is not available because of the decoder.
        // 1(d) helps an app to ask the user to insert a DVD-Video disc if none 
        // is specified/available in the drive when playback is started.
        // This method builds the graph even if 
        // - an invalid DVD-Video volume is specified
        // - the caller uses lpwszPathName = NULL to make the DVD Nav to locate
        //   the default volume to be played back, but DVD Nav doesn't find a 
        //   default DVD-Video volume to be played back.
        // An app can later specify the volume using IDvdControl::SetRoot() 
        // method.
        // #2 will help the app indicate to the user that some of the streams
        // can't be played.
        // 
        // The graph is built using filters based on the dwFlags value (to use 
        // HW decoders or SW decoders or a mix of them).
        // The dwFlags value is one of the values in AM_DVD_GRAPH_FLAGS enum
        // type.  The default value is AM_DVD_HWDEC_PREFER. None of the 
        // AM_DVD_HWDEC_xxx or AM_DVD_SWDEC_xxx flags can be mixed. However
        // AM_DVD_NOVPE can be OR-ed with any of the AM_DVD_HWDEC_xxx flags.
        //
        // The method returns S_OK if the playback graph is built successfully
        // with all the streams completely rendered and a valid DVD-Video volume 
        // is specified or a default one has been located.
        //
        // If the dwFlags specify conflicting options, E_INVALIDARG is returned.
        // If the graph building fails, the method returns one of the following 
        // error codes:
        //    VFW_E_DVD_RENDERFAIL, VFW_E_DVD_DECNOTENOUGH
        //
        [PreserveSig]
        int RenderDvdVideoVolume(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwszPathName,  // Can be NULL too
            AM_DVD_GRAPH_FLAGS dwFlags,        // 0 is the default (use max HW)
            [Out] out AM_DVD_RENDERSTATUS pStatus); // returns indications of ANY failure
    }

    [ComVisible(true), ComImport,
    GuidAttribute("5a4a97e4-94ee-4a55-9751-74b5643aa27d"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDvdCmd
    {
        //
        // WaitForStart
        //
        // Blocks the application until the command has begun.
        //
        [PreserveSig]
        int WaitForStart();

        //
        // WaitForEnd
        //
        // Blocks until the command has completed or has been cancelled.
        [PreserveSig]
        int WaitForEnd();
    }

    [ComVisible(true), ComImport,
    GuidAttribute("86303d6d-1c4a-4087-ab42-f711167048ef"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDvdState
    {
        //
        // GetDiscID
        //
        // Returns the disc ID from which the bookmark was made.
        //
        [PreserveSig]
        int GetDiscID(out long pullUniqueID); // 64-bit unique id for the disc

        //
        // GetParentalLevel
        //
        // Returns the state's parental level
        //
        [PreserveSig]
        int GetParentalLevel(out int pulParentalLevel); 
    }
    
    [ComVisible(true), ComImport,
    GuidAttribute("33BC7430-EEC0-11D2-8201-00A0C9D74842"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDvdControl2
    {
        // PlayTitle
        //
        // Start playing from the beginning of the specified title number.
        // Annex J: Title_Play
        // Title numbers range between 1 and 99.
        [PreserveSig]
        int PlayTitle(int ulTitle,	DVD_CMD_FLAGS dwFlags, [Out] out IDvdCmd ppCmd);

        // PlayChapterInTitle
        //
        // Start playing from the beginning of the given chapter (or part-of-title) number
        // within the specified title number.
        // Annex J: PTT_Play
        // Title numbers range between 1 and 99.
        // Chapters range from 1 to 999.
        [PreserveSig]
        int PlayChapterInTitle(int ulTitle, int ulChapter,
                                DVD_CMD_FLAGS dwFlags,  
                                [Out] out IDvdCmd ppCmd);

        // PlayAtTimeInTitle
        //
        // Start playing from the specified time within the specified title number.
        // NOTE: the actual start time will be the closest sync point before
        // or equal to the specified frame number.
        // Annex J: Time_Play
        // Title numbers range between 1 and 99.
        [PreserveSig]
        int PlayAtTimeInTitle(int ulTitle,
                        [In] ref DVD_HMSF_TIMECODE pStartTime,
                        DVD_CMD_FLAGS dwFlags,  
                        [Out] out IDvdCmd ppCmd);

        // Stop
        // Stop playback by transferring DVD Navigator to the DVD "Stop State" (same 
        // as DVD_DOMAIN_Stop), but filter graph remains in DirectShow's Run state.
        // Annex J: Stop
        [PreserveSig]
        int Stop();

        // ReturnFromSubmenu
        //
        // Stop playback of current program chain (PGC) and start playing the PGC 
        // specified by "GoUp_PGCN".in the PGCI.
        // If the GoUp_PGCN value is 0xFFFF the Resume() operation is carried out.
        // Annex J: GoUp
        [PreserveSig]
        int ReturnFromSubmenu(DVD_CMD_FLAGS dwFlags,  
                      [Out] out IDvdCmd ppCmd);

        // PlayAtTime
        // Start playing at the specified time within the current title.
        // NOTE: the actual start time will be the closest sync point before
        // or equal to the specified frame number.
        // Annex J: Time_Search
        // The time is in BCD format, passed in as a long.
        [PreserveSig]
        int PlayAtTime([In] ref DVD_HMSF_TIMECODE pTime,
                        DVD_CMD_FLAGS dwFlags,  
                        [Out] out IDvdCmd ppCmd);

        // PlayChapter
        // Start playing at the specified chapter (or part-of-title) within
        // the current title.
        // Annex J: PTT_Search
        // Chapters range from 1 to 999.
        [PreserveSig]
        int PlayChapter([In] int ulChapter,
                        DVD_CMD_FLAGS dwFlags,  
                        [Out] out IDvdCmd ppCmd);

        // PlayPrevChapter
        // Start playing at the beginning of the previous DVD "program".
        // For One-Sequential_PGC_Titles (which includes most titles) a program 
        // is equivalent to a chapter, otherwise a program is part of a chapter. 
        // Annex J: PrevPG_Search
        [PreserveSig]
        int PlayPrevChapter(DVD_CMD_FLAGS dwFlags, [Out] out IDvdCmd ppCmd);

        // ReplayChapter
        // Start playing from the beginning of they current program.
        // Annex J: TopPG_Search
        [PreserveSig]
        int ReplayChapter(DVD_CMD_FLAGS dwFlags, [Out] out IDvdCmd ppCmd);

        // PlayNextChapter
        // Start playing from the beginning of the next program.
        // Annex J: NextPG_Search
        [PreserveSig]
        int PlayNextChapter(DVD_CMD_FLAGS dwFlags, [Out] out IDvdCmd ppCmd);

        // PlayForwards
        // Set forward play at the specified speed.  
        // Annex J: Forward_Scan
        //      dSpeed == 1 is normal play
        //      dSpeed  < 1 is slow play
        //      dSpeed  > 1 is fast play
        // For dSpeed != 1, audio and subpicture is muted.
        [PreserveSig]
        int PlayForwards(double dSpeed,
                            DVD_CMD_FLAGS dwFlags,  
                            [Out] out IDvdCmd ppCmd);

        // PlayBackwards
        // Set reverse play at the specified speed.  
        // Annex J: Backward_Scan
        //      dSpeed == 1 is normal play speed in reverse
        //      dSpeed  < 1 is slow play in reverse
        //      dSpeed  > 1 is fast play in reverse
        // For reverse play, audio and subpicture are always muted.
        [PreserveSig]
        int PlayBackwards(double dSpeed,
                            DVD_CMD_FLAGS dwFlags,  
                            [Out] out IDvdCmd ppCmd);

        // ShowMenu
        // Start playback of the Menu specified by an enum DVD_MENU_ID.
        // Annex J: Menu_Call
        [PreserveSig]
        int ShowMenu(DVD_MENU_ID MenuID,
                    DVD_CMD_FLAGS dwFlags,  
                    [Out] out IDvdCmd ppCmd);

        // Resume
        // Returns to title playback in DVD_DOMAIN_Title. This is typically
        // done after MenuCall which puts the DVD Navigator in 
        // DVD_DOMAIN_VideoTitleSetMenu or DVD_DOMAIN_VideoManagerMenu.
        // Annex J: Resume
        [PreserveSig]
        int Resume(DVD_CMD_FLAGS dwFlags, [Out] out IDvdCmd ppCmd);

        // SelectRelativeButton
        // Moves the selection highlight above, below, to the left of, or to the right of the
        // currently selected.
        // "Selecting" a DVD button simply highlights the button but does
        // not "Activate" the button.  Selecting is the Windows equivalent 
        // to tabbing to a button but not pressing the space bar or enter key.
        // Activating is the Windows equivalent of pressing the space bar or
        // enter key after tabbing to a button.
        // Annex J: Upper_button_Select, Lower_button_Select, Left_button_Select, Right_button_Select
        [PreserveSig]
        int SelectRelativeButton(DVD_RELATIVE_BUTTON buttonDir);

        // ActivateButton
        // Activates current button.
        // Annex J: Button_Activate
        [PreserveSig]
        int ActivateButton();

        // SelectButton
        // Selects a specific button (with the index from 1 to 36).
        // ulButton is intended to be a number entered by a user corresponding
        // to button numbers currently displayed on screen.  
        // Button numbers range from 1 to 36.
        [PreserveSig]
        int SelectButton(int ulButton);

        // SelectAndActivateButton
        // Selects and then activates the button specified by the user.  
        // ulButton is intended to be a number entered by a user corresponding
        // to button numbers currently displayed on screen.  
        // Annex J: Button_Select_And_Activate
        // Button numbers range from 1 to 36.
        [PreserveSig]
        int SelectAndActivateButton(int ulButton);

        // StillOff
        // Releases any current still if there are no available buttons.
        // This includes VOBU stills, Cell stills, and PGC stills, whether the 
        // still is infinite.  When buttons are available, stills are released by
        // activating a button.  Note this does not release a Pause.
        // Annex J: Still_Off
        [PreserveSig]
        int StillOff();

        // Pause
        // Freezes / unfreezes playback and any internal timers. This is similar to
        // IMediaControl::Pause(), but not the same in effect as IMediaControl::Pause
        // puts the filter (all filters, if done to the graph) in paused state.
        // Annex J: Pause_On and Pause_Off
        // bState is TRUE or FALSE to indicate whether to do Puase_on/Pause_Off according
        // to Annex J terminology.
        [PreserveSig]
        int Pause([In, MarshalAs(UnmanagedType.Bool)] bool bState);

        // SelectAudioStream
        // Changes the current audio stream to ulAudio.
        // Annex J: Audio_Stream_Change
        // Audio stream number ranges between 0 and 7 or DEFAULT_AUDIO_STREAM (15 - default based on default language & language extension)
        [PreserveSig]
        int SelectAudioStream(int ulAudio,
                                DVD_CMD_FLAGS dwFlags,  
                                [Out] out IDvdCmd ppCmd);

        // SelectSubpictureStream
        // Changes the current subpicture stream number to ulSubPicture
        // Annex J: Sub-picture_Stream_Change (first param)
        // Subpicture stream number should be between 0 and 31 or 63.
        [PreserveSig]
        int SelectSubpictureStream(int ulSubPicture,
                                    DVD_CMD_FLAGS dwFlags,  
                                    [Out] out IDvdCmd ppCmd);

        // SetSubpictureState
        // Turns on/off current subpicture stream display.
        // Annex J: Sub-picture_Stream_Change (second param)
        // Subpicture state is On or Off (TRUE or FALSE)
        [PreserveSig]
        int SetSubpictureState([In, MarshalAs(UnmanagedType.Bool)] bool bState,
                                DVD_CMD_FLAGS dwFlags,  
                                [Out] out IDvdCmd ppCmd);

        // SelectAngle
        // Changes the current angle number.
        // Annex J: Angle_Change
        // Angle number is between 1 and 9.
        [PreserveSig]
        int SelectAngle(int ulAngle,
                        DVD_CMD_FLAGS dwFlags,  
                        [Out] out IDvdCmd ppCmd);

        // SelectParentalLevel
        // Selects the current player parental level.  
        // Annex J: Parental_Level_Select
        // Parental level ranges between 1 and 8.
        // The defined parental levels are listed below :
        //
        //      Level   Rating  
        //      -----   ------  
        //      1       G       
        //      3       PG      
        //      4       PG13    
        //      6       R       
        //      7       NC17    
        // Higher levels can play lower level content; lower levels cannot play 
        // higher level content.  The DVD Navigator provides no restriction on
        // setting the parental level.  DVD player application may enforce 
        // restriction on parental level setting, such as password protection for 
        // raising the current parental level.  Parental Management is disabled in
        // the Navigator by default.
        //
        // Note : To disable parental management, pass 0xffffffff for ulParentalLevel
        //        If parental management is disabled, then the player will play the
        //        first PGC in a parental block regardless of parental IDs.
        //
        [PreserveSig]
        int SelectParentalLevel(int ulParentalLevel);

        // SelectParentalCountry
        // Sets the country in which to interpret the Parental Level.
        // Annex J: Parental_Country_Select
        // The country specified using the Alpha-2 code of the ISO-3166 standard,
        [PreserveSig]
        int SelectParentalCountry([In, MarshalAs(UnmanagedType.LPArray)] byte[] bCountry);

        // SelectKaraokeAudioPresentationMode
        // Sets the Karaoke audio mode.  
        // Annex J: Karaoke_Audio_Presentation_Mode_Change
        // NOTE: This and all other Karoke support is currently not implemented.
        // Mode represents the audio mixing mode for Karaoke (same info as SPRM11).
        // Use a bitwise OR of the bits in DVD_KARAOKE_DOWNMIX
        [PreserveSig]
        int SelectKaraokeAudioPresentationMode(DVD_KARAOKE_DOWNMIX ulMode);

        // SelectVideoModePreference
        // The user can specify the (initial) preferred display mode (aspect ratio) 
        // (wide / letterbox / pan-scan) that should be used to display content
        // (16 : 9).
        // Annex J: Video_Presentation_Mode_Change
        // The parameter is a long that has one of the values defined in 
        // DVD_PREFERRED_DISPLAY_MODE
        [PreserveSig]
        int SelectVideoModePreference(DVD_PREFERRED_DISPLAY_MODE ulPreferredDisplayMode);
    
        // SetDVDDirectory
        // Sets the root directory containing the DVD-Video volume. 
        // Can only be called from the DVD Stop State (DVD_DOMAIN_Stop).
        // If the root directory is not successfully set before 
        // IMediaControl::Play is called, the first drive starting from c:
        // containing a VIDEO_TS directory in the top level directory
        // will be used as the root.
        [PreserveSig]
        int SetDVDDirectory([In, MarshalAs(UnmanagedType.LPWStr)] string pszwPath);

        // ActivateAtPosition
        // This is typically called in response to a mouse click.
        // The specified point within the display window is to see if it is
        // within a current DVD button's highlight rect.  If it is, that 
        // button is first selected, then activated.  
        // NOTE: DVD Buttons do not all necessarily have highlight rects,
        // button rects can overlap, and button rects do not always
        // correspond to the visual representation of DVD buttons.
        [PreserveSig]
        int ActivateAtPosition    // typically called after a mouse click
            ([In] GDI.POINT point);

        // SelectAtPosition
        // This is typically called in response to a mouse move within the 
        // display window.
        // The specified point within the display window is to see if it is
        // within a current DVD button's highlight rect.  If it is, that 
        // button is selected.
        // NOTE: DVD Buttons do not all necessarily have highlight rects,
        // button rects can overlap, and button rects do not always
        // correspond to the visual representation of DVD buttons.
        [PreserveSig]
        int SelectAtPosition    // typically called after a mouse move
            ([In] GDI.POINT point);
    
        // PlayChaptersAutoStop
        // Start playing at the specified chapter within the specified title
        // and play the number of chapters specified by the third parameter.
        // Then the playback stops by sending an event EC_DVD_CHAPTER_AUTOSTOP.
        // Title ranges from 1 to 99.
        // Chapter (and number of chapters to play) ranges from 1 to 999.
        [PreserveSig]
        int PlayChaptersAutoStop(int ulTitle,           // title number
                                int ulChapter,         // chapter number to start playback
                                int ulChaptersToPlay,   // number of chapters to play from the start chapter
                                DVD_CMD_FLAGS dwFlags,  
                                [Out] out IDvdCmd ppCmd);

        // AcceptParentalLevelChange
        //
        // Application's way of informing the Navigator that the required parental 
        // level change indicated through a previous event was accepted or rejected 
        // by the app (and unblock the Navigator).
        //
        // FALSE - reject the disc's request to change the current parental level.
        // TRUE  - change the parental level as required by the disc.
        [PreserveSig]
        int AcceptParentalLevelChange([In, MarshalAs(UnmanagedType.Bool)] bool bAccept);

        // SetOption(flag, true/false )
        // Flags:
        //
        // DVD_ResetOnStop
        //      Disable reset of the Navigator's internal state on the 
        //      subsequent IMediaControl::Stop() call(s).
        //
        //      FALSE - Navigator does not reset its state on the subsequent Stop calls 
        //              (play from the current location on next Run call).
        //      TRUE  - (default) Navigator resets its state on the subsequent Stop call 
        //              (play from the first play PGC on the Run call after the Stop).
        //
        // DVD_NotifyParentalLevelChange
        //
        //      Allows the application to indicate to the Navigator that it wants to control 
        //      parent level change (e.g., through a dialog box) and indicate the acceptance
        //      or rejection of the new parental level to the Navigator through 
        //      AcceptParentalLevelChange().
        //
        //      FALSE - disable (default).  Always reject request by the disc to change parental level.
        //      TRUE  - enable.  Navigator will send the app a 'EC_DVD_PARENTAL_LEVEL_CHANGE' event
        //              and block until AcceptParentalLevelChange() is called by the app.
        //
        // DVD_HMSF_TimeCodeEvents
        //
        //      Lets the application specify to the Navigator if it wants to get the new time
        //      event EC_DVD_CURRENT_HMSF_TIME with the HMSF format rather than the older 
        //      EC_DVD_CURRENT_TIME events.
        //
        //      FALSE - disable (default).  Older EC_DVD_CURRENT_TIME events are returned.
        //      TRUE  - enable.  Navigator will send the app EC_DVD_CURRENT_HMSF_TIME events.

        [PreserveSig]
        int SetOption(DVD_OPTION_FLAG flag,
                       [In, MarshalAs(UnmanagedType.Bool)] bool fState);

        // SetState
        //
        // The navigator will use the location information in the given state object to restore
        // the navigator's position to a specific location on the disc.
        // A valid state object is returned by either calling GetState(), or by using
        // "CoCreateInstance( CLSID_DVDState, NULL, CLSCTX_INPROC_SERVER, IID_IDvdState, (void **) ppState )"
        // to create a state object, followed by pState->IPersist::Load() to read it from memory or disk. 
        //
        [PreserveSig]
        int SetState([In] IDvdState pState,
                    DVD_CMD_FLAGS dwFlags,  
                    [Out] out IDvdCmd ppCmd);

        // PlayPeriodInTitleAutoStop
        //
        // Start playing from the specified time within the specified title number until the specified end time.
        // NOTE: the actual start and end times will be the closest sync points before
        // or equal to the specified frame number.
        // Annex J: Time_Play for a limited range
        // Title numbers range between 1 and 99.
        [PreserveSig]
        int PlayPeriodInTitleAutoStop(int ulTitle,
                                        [In] ref DVD_HMSF_TIMECODE pStartTime,
                                        [In] ref DVD_HMSF_TIMECODE pEndTime,
                                        DVD_CMD_FLAGS dwFlags,  
                                        [Out] out IDvdCmd ppCmd);

        // SetGPRM
        // Sets the current contents of a DVD General Parameter Register.
        // Use of GPRMs is title specific.

        [PreserveSig]
        int SetGPRM(int ulIndex, short wValue,	DVD_CMD_FLAGS dwFlags,  
            [Out] out IDvdCmd ppCmd);

        // SelectDefaultMenuLanguage
        // Selects the default language for menus.  
        // Languages are specified with Windows standard LCIDs.  LCIDs can be created 
        // from ISO-639 codes with
        // MAKELCID( MAKELANGID(wISO639LangID ,SUBLANG_DEFAULT ), SORT_DEFAULT ).
        // SelectMenuLanguage may only called from the DVD Stop state (DVD_DOMAIN_Stop).
        // Annex J: Menu_Language_Select
        //
        // NOT TRUE ANYMORE:
        // NOTE: MAKELANGID seems to have a bug so 'jp' may have to be used 
        // instead of 'ja' for the ISO639 code for Japanese.
        [PreserveSig]
        int SelectDefaultMenuLanguage(int Language);

        // SelectDefaultAudioLanguage 
        // Selects the default audio language.  
        // Languages are specified with Windows standard LCIDs.
        [PreserveSig]
        int SelectDefaultAudioLanguage(int Language, DVD_AUDIO_LANG_EXT audioExtension);

        // SelectDefaultSubpictureLanguage 
        // Selects the default subpicture language.  
        // Languages are specified with Windows standard LCIDs.
        [PreserveSig]
        int SelectDefaultSubpictureLanguage(int Language,
                       DVD_SUBPICTURE_LANG_EXT subpictureExtension);

    }

    [ComVisible(true), ComImport,
    GuidAttribute("34151510-EEC0-11D2-8201-00A0C9D74842"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDvdInfo2
    {
        // GetCurrentDomain
        // Returns the current DVD Domain of the DVD player.
        [PreserveSig]
        int GetCurrentDomain([Out] out DVD_DOMAIN pDomain);

        // GetCurrentLocation
        // Returns information sufficient to restart playback of a video
        // from the current playback location in titles that don't explicitly
        // disable seeking to the current location.
        [PreserveSig]
        int GetCurrentLocation([Out] out DVD_PLAYBACK_LOCATION2 pLocation);

        // GetTotalTitleTime
        // Returns the total playback time for the current title.  Only works
        // for One_Sequential_PGC_Titles.
        // THIS SHOULD CHANGE, RIGHT?
        [PreserveSig]
        int GetTotalTitleTime([Out] out DVD_HMSF_TIMECODE pTotalTime,
                        out int ulTimeCodeFlags);  // union of DVD_TIMECODE_FLAGS
                                  
        // GetCurrentButton
        // Indicates the number of currently available buttons and the current
        // selected button number. If buttons are not present it returns 0 for
        // both pulButtonsAvailable and pulCurrentButton
        [PreserveSig]
        int GetCurrentButton(out int pulButtonsAvailable, out int pulCurrentButton);

        // GetCurrentAngle
        // Indicates the number of currently available angles and the current
        // selected angle number.  If *pulAnglesAvailable is returned as 1 then 
        // the current content is not multiangle.
        [PreserveSig]
        int GetCurrentAngle(out int pulAnglesAvailable, out int pulCurrentAngle);

        // GetCurrentAudio
        // Indicates the number of currently available audio streams and 
        // the currently selected audio stream number.
        // This only works inside the Title domain.
        [PreserveSig]
        int GetCurrentAudio(out int pulStreamsAvailable, out int pulCurrentStream);

        // GetCurrentSubpicture
        // Indicates the number of currently available subpicture streams,
        // the currently selected subpicture stream number, and if the 
        // subpicture display is currently disabled.  Subpicture streams 
        // authored as "Forcedly Activated" stream will be displayed even if
        // subpicture display has been disabled by the app with 
        // IDVDControl::SetSubpictureState.
        // This only works inside the Title domain.
        [PreserveSig]
        int GetCurrentSubpicture(out int pulStreamsAvailable,
                        out int pulCurrentStream,
                        [Out, MarshalAs(UnmanagedType.Bool)] out bool pbIsDisabled);

        // GetCurrentUOPS
        // Indicates which IDVDControl methods (Annex J user operations) are 
        // currently valid.  DVD titles can enable or disable individual user 
        // operations at almost any point during playback.
        [PreserveSig]
        int GetCurrentUOPS([Out] out VALID_UOP_FLAG pulUOPs);

        // GetAllSPRMs
        // Returns the current contents of all DVD System Parameter Registers.
        // See DVD-Video spec for use of individual registers.
        // WE SHOULD DOC THE SPRMs RATHER THAN ASKING TO REFER TO DVD SPEC.
        [PreserveSig]
        int GetAllSPRMs(out IntPtr pRegisterArray);

        // GetAllGPRMs
        // Returns the current contents of all DVD General Parameter Registers.
        // Use of GPRMs is title specific.
        // WE SHOULD DOC THE GPRMs RATHER THAN ASKING TO REFER TO DVD SPEC.
        [PreserveSig]
        int GetAllGPRMs(out IntPtr pRegisterArray);

        // GetAudioLanguage
        // Returns the language of the specified stream within the current title.
        // Does not return languages for menus.  Returns *pLanguage as 0 if the
        // stream does not include language.
        // Use Win32 API GetLocaleInfo(*pLanguage, LOCALE_SENGLANGUAGE, pszString, cbSize)
        // to create a human readable string name from the returned LCID.
        [PreserveSig]
        int GetAudioLanguage(int ulStream, out int pLanguage);

        // GetSubpictureLanguage
        // Returns the language of the specified stream within the current title.
        // Does not return languages for menus.  Returns *pLanguage=0 as 0 if the
        // stream does not include language.
        // Use Win32 API GetLocaleInfo(*pLanguage, LOCALE_SENGLANGUAGE, pszString, cbSize)
        // to create a human readable string name from the returned LCID.
        [PreserveSig]
        int GetSubpictureLanguage(int ulStream, out int pLanguage);

        // GetTitleAttributes
        // Returns attributes of all video, audio, and subpicture streams for the 
        // specified title including menus.  
        // If 0xffffffff is specified as ulTitle, attributes for the current title 
        // are returned.
        [PreserveSig]
        int GetTitleAttributes(int ulTitle, // requested title number
                                [Out] out DVD_MenuAttributes pMenu,
                                [Out] out DVD_TitleAttributes pTitle);

        // GetVMGAttributes
        // Returns attributes of all video, audio, and subpicture 
        // streams for Video Manager Menus.  This method suppliments GetTitleAttributes()
        // for some menus, such as the Title menu, which are in a separate group of 
        // streams called the VMG (Video Manager) and are not associated with any 
        // particular title number.
        [PreserveSig]
        int GetVMGAttributes([Out] out DVD_MenuAttributes pATR);

        // GetCurrentVideoAttributes
        // Returns the video attributes for the current title or menu.
        //
        [PreserveSig]
        int GetCurrentVideoAttributes([Out] out DVD_VideoAttributes pATR);

        // GetAudioAttributes
        // Returns the audio attributes for the specified stream in the current title 
        // or menu.
        [PreserveSig]
        int GetAudioAttributes(int ulStream, [Out] out DVD_AudioAttributes pATR);

        // GetKaraokeChannelContents
        // Returns the karaoke contents of each channel of the specified stream in the current title 
        // or menu.
        [PreserveSig]
        int GetKaraokeAttributes(int ulStream, [Out] out DVD_KaraokeAttributes pAttributes);

        // GetSubpictureAttributes
        // Returns the subpicture attributes for the specified stream in the current
        // title or menu.
        [PreserveSig]
        int GetSubpictureAttributes(int ulStream, [Out] out DVD_SubpictureAttributes pATR);

        // GetDVDVolumeInfo
        // Returns current DVD volume information.
        [PreserveSig]
        int GetDVDVolumeInfo(out int pulNumOfVolumes,  // number of volumes (disc sides?) in a volume set
                            out int pulVolume,        // volume number for current DVD directory
                            out DVD_DISC_SIDE pSide,    // current disc side
                            out int pulNumOfTitles);    // number of titles available in this volume
                                
        // GetDVDTextNumberOfLanguages
        // Returns the number of text languages for the current DVD directory.
        // Should return some error code if no root directory is found.
        [PreserveSig]
        int GetDVDTextNumberOfLanguages(out int pulNumOfLangs);
    
        // GetDVDTextLanguageInfo
        // Returns the text languages information (number of strings, language code, 
        // char set) for the specified language index.
        // Should return some error code if an invalid text index is specified.
        [PreserveSig]
        int GetDVDTextLanguageInfo(int ulLangIndex,
                                    out int pulNumOfStrings, 
                                    out int pLangCode, 
                                    out DVD_TextCharSet pbCharacterSet);

        // GetDVDTextStringAsNative
        // Returns the text string as an array of bytes for the specified language 
        // index.and string index.
        // Should return some error code if an invalid text or string index is specified.
        // It also just returns the length of the string if pchBuffer is specified as NULL.
        [PreserveSig]
        int GetDVDTextStringAsNative(int ulLangIndex, 
                                    int ulStringIndex, 
                                    IntPtr pbBuffer, 
                                    int ulMaxBufferSize, 
                                    out int pulActualSize, 
                                    out DVD_TextStringType pType);

        // GetDVDTextStringAsUnicode
        // Returns the text string in Unicode for the specified language index.and string index.
        // Should return some error code if an invalid text or string index is specified.
        // It also just returns the length of the string if pchBuffer is specified as NULL.
        [PreserveSig]
        int GetDVDTextStringAsUnicode(int ulLangIndex, 
                                    int ulStringIndex, 
                                    IntPtr pchwBuffer, 
                                    int ulMaxBufferSize, 
                                    out int pulActualSize, 
                                    out DVD_TextStringType pType);

        // GetPlayerParentalLevel
        // Returns the current parental level and the current country code that has 
        // been set in the system registers in player.
        // See Table 3.3.4-1 of the DVD-Video spec for the defined parental levels.
        // Valid Parental Levels range from 1 to 8 if parental management is enabled.
        // Returns 0xffffffff if parental management is disabled
        // See ISO3166 : Alpha-2 Code for the country codes.
        [PreserveSig]
        int GetPlayerParentalLevel(out int pulParentalLevel,    // current parental level
                                    [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbCountryCode);       // current country code
                            
        //  GetNumberOfChapters
        //  Returns the number of chapters that are defined for a
        //  given title.
        [PreserveSig]
        int GetNumberOfChapters(int ulTitle,           // Title for which number of chapters is requested
                                out int pulNumOfChapters);  // Number of chapters for the specified title
                                
        // GetTitleParentalLevels
        // Returns the parental levels that are defined for a particular title. 
        // pulParentalLevels will be combination of DVD_PARENTAL_LEVEL_8, 
        // DVD_PARENTAL_LEVEL_6, or DVD_PARENTAL_LEVEL_1 OR-ed together
        [PreserveSig]
        int GetTitleParentalLevels(int ulTitle,            // Title for which parental levels are requested
                                    out int pulParentalLevels);  // Parental levels defined for the title "OR"ed together
                                 
        // GetDVDDirectory
        // Returns the root directory that is set in the player. If a valid root
        // has been found, it returns the root string. Otherwise, it returns 0 for
        // pcbActualSize indicating that a valid root directory has not been found
        // or initialized.
        //
        // !!! used to return LPTSTR. interface was changed to return
        // LPSTR (ansi) for compatibility. COM APIs should pass with
        // UNICODE strings only.
        // 
        [PreserveSig]
        int GetDVDDirectory(IntPtr pszwPath, // pointer to buffer to get root string
                            int ulMaxSize,  // size of buffer in WCHARs passed in
                            out int pulActualSize); // size of actual data returned (in WCHARs)
                        
        // IsAudioStreamEnabled
        //
        // Determines if the specified audio stream is enabled/disabled in the current PGC.
        //
        // ulStreamNum - audio stream number to test
        // pbEnabled   - where to place the result
        [PreserveSig]
        int IsAudioStreamEnabled(int ulStreamNum,   // stream number to test
                                [Out, MarshalAs(UnmanagedType.Bool)] out bool pbEnabled); // returned state
                
        // GetDiscID
        //
        // If pszwPath is specified as NULL, DVD Navigator will use the current path
        // that would be returned by GetDVDDirectory() at this point.
        //
        // Returns a 64-bit identification number for the specified DVD disc.
        [PreserveSig]
        int GetDiscID([In, MarshalAs(UnmanagedType.LPWStr)] string pszwPath, // root path (should we rather use const WCHAR*?)
                       out long pullDiscID);  // 64-bit unique id for the disc
                                
        // GetState
        //
        // The navigator will create a new state object and save the current location into it.
        // The state object can be used to restore the navigator the saved location at a later time.
        // A new IDvdState object is created (with a single AddRef) and returned in *pStateData.
        // The object must be Released() when the application is finished with it.
        //
        [PreserveSig]
        int GetState([Out] out IDvdState pStateData); // returned object
                    
        //
        // GetMenuLanguages
        //
        // Navigator gets all of the menu languages for the VMGM and VTSM domains.
        //
        [PreserveSig]
        int GetMenuLanguages([Out, MarshalAs(UnmanagedType.LPArray)] int[] pLanguages, // data buffer (NULL returns #languages) 
                            int ulMaxLanguages,      // maxiumum number of languages to retrieve
                            out int pulActualLanguages); // actual number of languages retrieved
        
        //
        // GetButtonAtPosition
        //
        // This is typically called in response to a mouse move within the 
        // display window.
        // It returns the button located at the specified point within the display window.
        // If no button is present at that position, then VFW_E_DVD_NO_BUTTON is returned.
        // Button indices start at 1. 
        //
        // NOTE: DVD Buttons do not all necessarily have highlight rects,
        // button rects can overlap, and button rects do not always
        // correspond to the visual representation of DVD buttons.
        [PreserveSig]
        int GetButtonAtPosition    // typically called after a mouse move
            ([In] GDI.POINT point,
            out int pulButtonIndex);

        //
        // GetCmdFromEvent
        //
        // This method maps an EC_DVD_CMD_BEGIN/COMPLETE/CANCEL event's lParam1 into an AddRef'd
        // IDvdCmd pointer.  You must Release the returned pointer.  NULL is returned if the function
        // fails.
        //
        [PreserveSig]
        int GetCmdFromEvent([In] IntPtr lParam1, [Out] out IDvdCmd pCmdObj);

        // GetDefaultMenuLanguage
        // Returns the default language for menus.
        [PreserveSig]
        int GetDefaultMenuLanguage(out int pLanguage);

        // GetDefaultAudioLanguage 
        // Gets the default audio language.  
        // Languages are specified with Windows standard LCIDs.
        [PreserveSig]
        int GetDefaultAudioLanguage(out int pLanguage,	out DVD_AUDIO_LANG_EXT pAudioExtension);

        // GetDefaultSubpictureLanguage 
        // Gets the default subpicture language.  
        // Languages are specified with Windows standard LCIDs.
        [PreserveSig]
        int GetDefaultSubpictureLanguage(out int pLanguage, out DVD_SUBPICTURE_LANG_EXT  pSubpictureExtension);

        //
        // GetDecoderCaps:
        // Retrieves the DVD decoder's details about max data rate for video, audio
        // and subpicture (going backward and forward) as well as support for various
        // types of audio (AC3, MPEG2, DTS, SDDS, LPCM).
        //
        [PreserveSig]
        int GetDecoderCaps(ref DVD_DECODER_CAPS pCaps);

        //
        // GetButtonRect:
        // Retrieves the coordinates for a given button number
        //
        [PreserveSig]
        int GetButtonRect(int ulButton, out GDI.RECT pRect);

        // IsSubpictureStreamEnabled
        //
        // Determines if the specified subpicture stream is enabled/disabled in the current PGC.
        //
        // ulStreamNum - Subpicture stream number to test
        // pbEnabled   - where to place the result
        [PreserveSig]
        int IsSubpictureStreamEnabled(int ulStreamNum,   // stream number to test
                [Out, MarshalAs(UnmanagedType.Bool)] out bool pbEnabled); // returned state
    }

    [ComVisible(false)]
    public enum AM_LINE21_CCLEVEL 
    {  // should we use TC1, TC2 in stead?
        AM_L21_CCLEVEL_TC2 = 0
    }

    [ComVisible(false)]
    public enum AM_LINE21_CCSERVICE 
    {
        AM_L21_CCSERVICE_None = 0,
        AM_L21_CCSERVICE_Caption1,
        AM_L21_CCSERVICE_Caption2,
        AM_L21_CCSERVICE_Text1,
        AM_L21_CCSERVICE_Text2,
        AM_L21_CCSERVICE_XDS,
        AM_L21_CCSERVICE_DefChannel = 10,
        AM_L21_CCSERVICE_Invalid
    }

    [ComVisible(false)]
    public enum AM_LINE21_CCSTATE 
    {
        AM_L21_CCSTATE_Off = 0,
        AM_L21_CCSTATE_On
    }

    [ComVisible(false)]
    public enum AM_LINE21_CCSTYLE 
    {
        AM_L21_CCSTYLE_None = 0,
        AM_L21_CCSTYLE_PopOn,
        AM_L21_CCSTYLE_PaintOn,
        AM_L21_CCSTYLE_RollUp
    }

    [ComVisible(false)]
    public enum AM_LINE21_DRAWBGMODE 
    {
        AM_L21_DRAWBGMODE_Opaque,
        AM_L21_DRAWBGMODE_Transparent
    }
    
    [ComVisible(true), ComImport,
    GuidAttribute("6E8D4A21-310C-11d0-B79A-00AA003767A7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMLine21Decoder
    {
        //
        // Decoder options to be used by apps
        //

        // What is the decoder's level
        [PreserveSig]
        int GetDecoderLevel(out AM_LINE21_CCLEVEL lpLevel);  
        // supported level value is AM_L21Level_TC2 only
        // skipping the SetDecoderLevel( )

        // Which of the services is being currently used
        [PreserveSig]
        int GetCurrentService(out AM_LINE21_CCSERVICE lpService);  
        [PreserveSig]
        int SetCurrentService(AM_LINE21_CCSERVICE Service);  
        // supported service values are AM_L21Service_Caption1, 
        // AM_L21Service_Caption2, AM_L21Service_Text1, AM_L21Service_Text2, 
        // AM_L21Service_XDS, AM_L21Service_None)

        // Query/Set the service state (On/Off)
        // supported state values are AM_L21State_On and AM_L21State_Off
        [PreserveSig]
        int GetServiceState(out AM_LINE21_CCSTATE lpState);  
        [PreserveSig]
        int SetServiceState(AM_LINE21_CCSTATE State);  

        //
        // Output options to be used by downstream filters
        //

        // What size, bitdepth, etc. should the output video be
        [PreserveSig]
        int GetOutputFormat(out BITMAPINFOHEADER lpbmih);
        // GetOutputFormat() method, if successful, returns 
        // 1.  S_FALSE if no output format has so far been defined by downstream filters
        // 2.  S_OK if an output format has already been defined by downstream filters
        [PreserveSig]
        int SetOutputFormat(out BITMAPINFO lpbmi);

        // Specify physical color to be used in colorkeying the background 
        // for overlay mixing
        [PreserveSig]
        int GetBackgroundColor(out int pdwPhysColor);
        [PreserveSig]
        int SetBackgroundColor(int dwPhysColor);

        // Specify if whole output bitmap should be redrawn for each sample
        [PreserveSig]
        int GetRedrawAlways([Out, MarshalAs(UnmanagedType.Bool)] out bool lpbOption);
        [PreserveSig]
        int SetRedrawAlways([In, MarshalAs(UnmanagedType.Bool)] bool bOption);

        // Specify if the caption text background should be opaque/transparent
        [PreserveSig]
        int GetDrawBackgroundMode(out AM_LINE21_DRAWBGMODE lpMode);
        [PreserveSig]
        int SetDrawBackgroundMode(AM_LINE21_DRAWBGMODE Mode);
        // supported mode values are AM_L21_DrawBGMode_Opaque and
        // AM_L21_DrawBGMode_Transparent
    }
}
