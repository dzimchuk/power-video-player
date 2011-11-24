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
using Dzimchuk.Native;
using Dzimchuk.DirectShow;

namespace Dzimchuk.MediaEngine.Core.Description
{
    [Flags]
    public enum StreamInfoFlags
    {
        SI_RECT					= 1,		// rcSrc
        SI_VIDEOBITRATE			= 2,		// dwBitRate (but it's zero in the beginning)
        SI_FRAMERATE			= 4,		// AvgTimePerFrame
        SI_FOURCC				= 8,		// GetVideoSubType()
        SI_ASPECTRATIO			= 16,		// dwPictAspectRatioX and dwPictAspectRatioY
        SI_INTERLACEMODE		= 32,		// GetInterlaceMode()
        SI_WAVEFORMAT			= 64,		// GetWaveFormat()
        SI_SAMPLERATE			= 128,		// nSamplesPerSec
        SI_WAVECHANNELS			= 256,		// nChannels
        SI_BITSPERSAMPLE		= 512,		// wBitsPerSample (check for 0)
        SI_AUDIOBITRATE			= 1024,		// nAvgBytesPerSec (check for 0)
        SI_DVDFRAMERATE			= 2048,		// ulFrameRate
        SI_DVDFRAMEHEIGHT		= 4096,		// ulFrameHeight
        SI_DVDCOMPRESSION		= 8192,		// GetDVDCompressionType()
        SI_DVDAUDIOSTREAMNAME	= 16384,	// m_strDVDAudioStreamName
        SI_DVDAUDIOFORMAT		= 32768,	// GetDVDAudioFormat()
        SI_DVDFREQUENCY			= 65536,	// dwFrequency
        SI_DVDQUANTIZATION		= 131072	// Quantization
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class StreamInfo
    {
        public StreamInfoFlags Flags; // what data members contain valid data
        public Guid MajorType;
        public Guid FormatType;
        public Guid SubType;

        public GDI.RECT rcSrc;
        public int dwBitRate; // Approximate data rate of the video stream, in bits per second

        public long AvgTimePerFrame; // The desired average display time of the video frames, in 100-nanosecond units
        public int dwPictAspectRatioX; // The X dimension of picture aspect ratio. For example, 16 for a 16-inch x 9-inch display.
        public int dwPictAspectRatioY;
        public int dwInterlaceFlags;

        public short wFormatTag; // Waveform-audio format type

        public int nSamplesPerSec; // Sample rate, in samples per second (hertz). 
        public short nChannels;
        public short wBitsPerSample;
        public int nAvgBytesPerSec; // average data-transfer rate, in bytes per second

        public int ulFrameRate; // The frame rate in hertz (Hz), either 50 or 60
        public int ulFrameHeight; // The frame height in lines (525 for a frame rate of 60 Hz or 625 for 50 Hz)
        public DVD_VIDEO_COMPRESSION dvdCompression;

        public string strDVDAudioStreamName;
        public DVD_AUDIO_FORMAT AudioFormat;

        public int dwFrequency; // in hertz
        public byte Quantization; // 0 - the resolution is unknown
        
        public StreamInfo()
        {
        }

        public string GetMajorType()
        {
            string strMajorType;
            if ((Flags & StreamInfoFlags.SI_DVDCOMPRESSION) != 0)
                strMajorType=Resources.Resources.si_video;
            else if ((Flags & StreamInfoFlags.SI_DVDAUDIOFORMAT) != 0)
                strMajorType = Resources.Resources.si_audio;
            else if (MajorType==MediaType.Audio)
                strMajorType = Resources.Resources.si_audio;
            else if (MajorType==MediaType.AUXLine21Data)
                strMajorType = Resources.Resources.si_line21data;
            else if (MajorType==MediaType.Interleaved)
                strMajorType = Resources.Resources.si_interleaved_audio_and_video;
            else if (MajorType==MediaType.Midi)
                strMajorType = Resources.Resources.si_midi;
            else if (MajorType==MediaType.ScriptCommand)
                strMajorType = Resources.Resources.si_script_command;
            else if (MajorType==MediaType.Stream)
                strMajorType = Resources.Resources.si_byte_stream;
            else if (MajorType==MediaType.Text)
                strMajorType = Resources.Resources.si_text;
            else if (MajorType==MediaType.Timecode)
                strMajorType = Resources.Resources.si_timecode;
            else if (MajorType==MediaType.Video)
                strMajorType = Resources.Resources.si_video;
            else
                strMajorType = String.Empty;

            return strMajorType;
        }

        public string GetVideoSubType()
        {
            string strFormat = String.Empty;
            // check Packed YUV Formats
            if (SubType==MediaSubType.AYUV)
                strFormat="4:4:4 YUV format";
            else if (SubType==MediaSubType.UYVY)
                strFormat="UYVY (packed 4:2:2)";
            else if (SubType==MediaSubType.Y411)
                strFormat="Y41P (packed 4:1:1)";
            else if (SubType==MediaSubType.Y41P)
                strFormat="Y41P (packed 4:1:1)";
            else if (SubType==MediaSubType.Y211)
                strFormat="Y211";
            else if (SubType==MediaSubType.YUY2)
                strFormat="YUY2 (packed 4:2:2)";
            else if (SubType==MediaSubType.YVYU)
                strFormat="YVYU (packed 4:2:2)";
            else if (SubType==MediaSubType.YUYV)
                strFormat="YUYV (packed 4:2:2)";
            else if (SubType==MediaSubType.IF09) // now check Planar YUV Formats
                strFormat="Indeo YVU9";
            else if (SubType==MediaSubType.IYUV)
                strFormat="IYUV";
            else if (SubType==MediaSubType.YV12)
                strFormat="YV12";
            else if (SubType==MediaSubType.YVU9)
                strFormat="YVU9";
            else if (SubType==MediaSubType.RGB1) // let's check Uncompressed RGB Video Subtypes
                strFormat="RGB, 1 bit per pixel (bpp), palettized";
            else if (SubType==MediaSubType.RGB4)
                strFormat="RGB, 4 bpp, palettized";
            else if (SubType==MediaSubType.RGB8)
                strFormat="RGB, 8 bpp";
            else if (SubType==MediaSubType.RGB565)
                strFormat="RGB 565, 16 bpp";
            else if (SubType==MediaSubType.RGB555)
                strFormat="RGB 555, 16 bpp";
            else if (SubType==MediaSubType.RGB24)
                strFormat="RGB, 24 bpp";
            else if (SubType==MediaSubType.RGB32)
                strFormat="RGB, 32 bpp, no alpha channel";
            else if (SubType==MediaSubType.ARGB32)
                strFormat="RGB, 32 bpp, alpha channel";
            else if (SubType==MediaSubType.QTJpeg)
                strFormat="QuickTime JPEG compressed data";
            else if (SubType==MediaSubType.QTMovie)
                strFormat="Apple QuickTime® compression";
            else if (SubType==MediaSubType.QTRle)
                strFormat="QuickTime RLE compressed data";
            else if (SubType==MediaSubType.QTRpza)
                strFormat="QuickTime RPZA compressed data";
            else if (SubType==MediaSubType.QTSmc)
                strFormat="QuickTime SMC compressed data";
    
            if (strFormat.Length != 0)
                return strFormat;
            strFormat = MediaTypeManager.GetInstance().GetTypeName(MajorType, SubType);
            return strFormat != null ? strFormat : MediaTypeManager.GetInstance().GetFourCC(SubType);
        }

        public string GetInterlaceMode()
        {
            string strMode = String.Empty;
            if ((dwInterlaceFlags & DvdHlp.AMINTERLACE_IsInterlaced)==0) 
            {
                strMode="Progressive frames (video stream is not interlaced)";
                return strMode;
            }

            switch (dwInterlaceFlags & DvdHlp.AMINTERLACE_DisplayModeMask)
            {
                case DvdHlp.AMINTERLACE_DisplayModeBobOnly:
                {
                    // Bob display mode only.
                    // A technique for displaying interlaced video. 
                    // In bob mode, each field is displayed individually. 
                    // The gaps between scan lines are filled using interpolation. 
                    if ((dwInterlaceFlags & DvdHlp.AMINTERLACE_1FieldPerSample)!=0)
                    {
                        strMode="Non-interleaved bob";
                        // each media sample contains a single video field
                    }
                    else
                    {
                        strMode="Interleaved bob";
                        // Each media sample contains two video fields. 
                        // Flags on the media sample indicate which field to display first.
                    }
                    break;
                }
                case DvdHlp.AMINTERLACE_DisplayModeWeaveOnly:
                {
                    // Weave display mode only.
                    // A technique for displaying interlaced video.
                    // In weave mode, alternating fields are combined into a single image.
                    strMode="Weave";
                    break;
                }
                case DvdHlp.AMINTERLACE_DisplayModeBobOrWeave:
                {
                    // Either bob or weave mode.
                    strMode="Bob or weave";
                    // The video stream varies between progressive and interlaced content.  
                    // Each media sample contains either a progressive frame or two video fields. 
                    // Flags on the media sample indicate the correct way to display the contents. 
                    break;
                }
            }

            return strMode;
        }

        public string GetWaveFormat()
        {
/*	
Media subtypes are defined for each wFormatTag as follows: 

The Data 1 subfield of the Media Subtype is the same as the wFormatTag value. 
The Data 2 field is 0. 
The Data 3 field is 0x0010. 
The Data 4 field is 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71. 
Thus, for PCM audio the subtype GUID (defined in uuids.h as MEDIASUBTYPE_PCM) is: 

{00000001-0000-0010-8000-00AA00389B71}
*/
            string s, strFormat;
            s = wFormatTag != 0 ? String.Format("0x{0:X4} ", wFormatTag) : String.Empty;
            strFormat=null;
            switch (wFormatTag)
            {
                case DsHlp.WAVE_FORMAT_PCM:
                    strFormat="(PCM audio)";
                    break;
                case DsHlp.WAVE_FORMAT_IEEE_FLOAT:
                    strFormat="(IEEE FLOAT)";
                    break;
                case DsHlp.WAVE_FORMAT_DRM:
                    strFormat="(DRM Audio)";
                    break;
                case DsHlp.WAVE_FORMAT_MSNAUDIO:
                    strFormat="(MSNAUDIO)";
                    break;
                case DsHlp.WAVE_FORMAT_MPEG:
                    strFormat="(MPEG1 Audio Payload)";
                    break;
                case DsHlp.WAVE_FORMAT_MPEGLAYER3:
                    strFormat="(ISO/MPEG Layer3)";
                    break;
                case DsHlp.WAVE_FORMAT_DOLBY_AC3_SPDIF:
                    strFormat="(Dolby AC3 over SPDIF)";
                    break;
                case DsHlp.WAVE_FORMAT_MSAUDIO1:
                    strFormat="(MSAUDIO1)";
                    break;
                case 0x0161:
                    strFormat="(WMA)";
                    break;
                case DsHlp.WAVE_FORMAT_RAW_SPORT:
                    strFormat="(Dolby AC3 over SPDIF)";
                    break;
                case DsHlp.WAVE_FORMAT_ESST_AC3:
                    strFormat="(Dolby AC3 over SPDIF)";
                    break;
                case DsHlp.WAVE_FORMAT_VOXWARE:
                case DsHlp.WAVE_FORMAT_VOXWARE_BYTE_ALIGNED:
                case DsHlp.WAVE_FORMAT_VOXWARE_AC8:
                case DsHlp.WAVE_FORMAT_VOXWARE_AC10:
                case DsHlp.WAVE_FORMAT_VOXWARE_AC16:
                case DsHlp.WAVE_FORMAT_VOXWARE_AC20:
                case DsHlp.WAVE_FORMAT_VOXWARE_RT24:
                case DsHlp.WAVE_FORMAT_VOXWARE_RT29:
                case DsHlp.WAVE_FORMAT_VOXWARE_RT29HW:
                case DsHlp.WAVE_FORMAT_VOXWARE_VR12:
                case DsHlp.WAVE_FORMAT_VOXWARE_VR18:
                case DsHlp.WAVE_FORMAT_VOXWARE_TQ40:
                case DsHlp.WAVE_FORMAT_VOXWARE_TQ60:
                    strFormat="(Voxware)";
                    break;
                case DsHlp.WAVE_FORMAT_SOFTSOUND:
                    strFormat="(Softsound)";
                    break;
                case DsHlp.WAVE_FORMAT_AAC:
                    strFormat = "(AAC)";
                    break;
                default:
                    strFormat = MediaTypeManager.GetInstance().GetTypeName(MajorType, SubType);
                    break;
            }
            if (strFormat != null)
                s+=strFormat;
            return s;
        }

        public string GetDVDCompressionType()
        {
            string str;
            if (dvdCompression==DVD_VIDEO_COMPRESSION.DVD_VideoCompression_MPEG1)
                str="MPEG-1";
            else if (dvdCompression==DVD_VIDEO_COMPRESSION.DVD_VideoCompression_MPEG2)
                str="MPEG-2";
            else
                str = Resources.Resources.si_unknown_compression_type;
            return str;
        }

        public string GetDVDAudioFormat()
        {
            string str;
            switch(AudioFormat)
            {
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_AC3:
                    str="Dolby AC-3";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_MPEG1:
                    str="MPEG-1";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_MPEG1_DRC:
                    str="MPEG-1 with dynamic range control";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_MPEG2:
                    str="MPEG-2";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_MPEG2_DRC:
                    str="MPEG-2 with dynamic range control";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_LPCM:
                    str="Linear Pulse Code Modulated (LPCM)";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_DTS:
                    str="Digital Theater Systems (DTS)";
                    break;
                case DVD_AUDIO_FORMAT.DVD_AudioFormat_SDDS:
                    str="Sony Dynamic Digital Sound (SDDS)";
                    break;
                default:
                    str="unrecognized";
                    break;
            }
            return str;
        }
    }
}
