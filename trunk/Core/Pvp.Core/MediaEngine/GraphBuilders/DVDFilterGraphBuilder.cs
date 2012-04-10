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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.Render;

namespace Pvp.Core.MediaEngine.GraphBuilders
{
    /// <summary>
    /// 
    /// </summary>
    internal class DVDFilterGraphBuilder : FilterGraphBuilder
    {
        private bool bUsePreferredFilters;
        private static DVDFilterGraphBuilder graphBuilder;
        
        [DllImport("quartz.dll", CharSet = CharSet.Auto)]
        private static extern uint AMGetErrorText(int hr, StringBuilder pBuffer, int MaxLen);

        private DVDFilterGraphBuilder()
        {
        }

        public static DVDFilterGraphBuilder GetGraphBuilder()
        {
            if (graphBuilder == null)
                graphBuilder = new DVDFilterGraphBuilder();
            return graphBuilder;
        }

        public bool UsePreferredFilters
        {
            get { return bUsePreferredFilters; }
            set { bUsePreferredFilters = value; }
        }

        protected override void BuildFilterGraph(ref object comobj,
                                                 ref FilterGraphBuilderParameters parameters)
        {
            // TODO: remove these redundant declarations
            FilterGraph pFilterGraph = parameters.pFilterGraph;
            IntPtr hMediaWindow = parameters.hMediaWindow;
            string DiscPath = parameters.DiscPath;
            AM_DVD_GRAPH_FLAGS dwFlags = parameters.dwFlags;
            Func<string, bool> onPartialSuccessCallback = parameters.OnPartialSuccessCallback;
            Renderer preferredVideoRenderer = parameters.PreferredVideoRenderer;
                        
            // Create the DVD Graph Builder
            try
            {
                Type _type = Type.GetTypeFromCLSID(Clsid.DvdGraphBuilder, true);
                comobj = Activator.CreateInstance(_type);
                pFilterGraph.pDVDGraphBuilder = (IDvdGraphBuilder)comobj;
                comobj = null; // important! (see the finally block)
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.DvdGraphBuilder, e);
            }

            int hr = pFilterGraph.pDVDGraphBuilder.GetFiltergraph(out pFilterGraph.pGraphBuilder);
            ThrowExceptionForHR(pFilterGraph, hr, Error.DvdGraphBuilder);

            // It is important to QI for IMediaEventEx *before* building the graph so that
            // the app can catch all of the DVD Navigator's initialization events.  Once
            // an app QI's for IMediaEventEx, DirectShow will start queuing up events and 
            // the app will receive them when it sets the notify window. If the app does not
            // QI for IMediaEventEx before building the graph, these events will just be lost.
            try
            {
                pFilterGraph.pMediaEventEx = (IMediaEventEx)pFilterGraph.pGraphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.NecessaryInterfaces, e);
            }
            // SET the graph state window callback
            pFilterGraph.pMediaEventEx.SetNotifyWindow(hMediaWindow, (int)FilterGraph.UWM_GRAPH_NOTIFY, IntPtr.Zero);

            // request desired renderer (may return null)            
            if (parameters.Renderer == null)
            {
                pFilterGraph.pRenderer = RendererBase.AddRenderer(pFilterGraph.pDVDGraphBuilder,
                                                                  pFilterGraph.pGraphBuilder,
                                                                  preferredVideoRenderer,
                                                                  hMediaWindow);
            }
            else
            {
                pFilterGraph.pRenderer = RendererBase.AddRenderer(pFilterGraph.pDVDGraphBuilder,
                                                                  pFilterGraph.pGraphBuilder,
                                                                  parameters.Renderer,
                                                                  hMediaWindow);
            }
            
            // Build the Graph
            string str;
            AM_DVD_RENDERSTATUS buildStatus;
            hr=pFilterGraph.pDVDGraphBuilder.RenderDvdVideoVolume(DiscPath, dwFlags, out buildStatus);
            if (DsHlp.FAILED(hr)) // total failure
            {
                // If there is no DVD decoder, give a user-friendly message
                if ((uint)hr == DsHlp.VFW_E_DVD_DECNOTENOUGH)
                    str = Resources.Resources.dvd_not_enough_decoders;
                else
                    str = Resources.Resources.dvd_cant_render_volume;
                throw new FilterGraphBuilderException(str);
            }

            if (DsHlp.S_FALSE == hr) // partial success
            {
                StringBuilder strError = new StringBuilder();
                bool bOk = GetStatusText(pFilterGraph, ref buildStatus, strError);
                str = strError.Length == 0 ? Resources.Resources.dvd_unknown_error : strError.ToString();
                if (!bOk)
                    throw new FilterGraphBuilderException(str);

                if (!onPartialSuccessCallback(str + "\n" + Resources.Resources.dvd_question_continue))
                {
                    throw new AbortException();
                }
            }

            // The graph was successfully rendered in some form if we get this far
            // We will now instantiate all of the necessary interfaces
            if (pFilterGraph.pRenderer == null)
                pFilterGraph.pRenderer = RendererBase.GetExistingRenderer(pFilterGraph.pGraphBuilder, hMediaWindow);

            object o;
            Type type = typeof(IDvdInfo2);
            Guid guid = type.GUID;
            hr = pFilterGraph.pDVDGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.FAILED(hr))
            {
                Guid IID_IDvdInfo = new Guid("A70EFE60-E2A3-11d0-A9BE-00AA0061BE93");
                hr = pFilterGraph.pDVDGraphBuilder.GetDvdInterface(ref IID_IDvdInfo, out o);
                if (DsHlp.SUCCEEDED(hr))
                    str = Resources.Resources.dvd_incompatible_dshow;
                else
                    str = Resources.Resources.error_cant_retrieve_all_interfaces;
                throw new FilterGraphBuilderException(str);
            }

            pFilterGraph.pDvdInfo2 = (IDvdInfo2)o;

            type = typeof(IDvdControl2);
            guid = type.GUID;
            hr = pFilterGraph.pDVDGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.FAILED(hr))
            {
                Guid IID_IDvdControl = new Guid("A70EFE61-E2A3-11d0-A9BE-00AA0061BE93");
                hr = pFilterGraph.pDVDGraphBuilder.GetDvdInterface(ref IID_IDvdControl, out o);
                if (DsHlp.SUCCEEDED(hr))
                    str = Resources.Resources.dvd_incompatible_dshow;
                else
                    str = Resources.Resources.error_cant_retrieve_all_interfaces;
                throw new FilterGraphBuilderException(str);
            }

            pFilterGraph.pDvdControl2 = (IDvdControl2)o;

            // this one may or may not be present
            type = typeof(IAMLine21Decoder);
            guid = type.GUID;
            hr = pFilterGraph.pDVDGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.SUCCEEDED(hr))
                pFilterGraph.pAMLine21Decoder = (IAMLine21Decoder)o;
            
            try
            {
                pFilterGraph.pMediaControl = (IMediaControl)pFilterGraph.pGraphBuilder;
                pFilterGraph.pBasicAudio = (IBasicAudio)pFilterGraph.pGraphBuilder;
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.NecessaryInterfaces, e);
            }
            
            // Set DVD Navigator options
            SetDVDPlaybackOptions(pFilterGraph);
    
            int Width=0;
            int Height=0;
            pFilterGraph.pRenderer.GetNativeVideoSize(out Width, out Height, out pFilterGraph.ARWidth, out pFilterGraph.ARHeight);
            double w = pFilterGraph.ARWidth;
            double h = pFilterGraph.ARHeight;
            pFilterGraph.dAspectRatio=w/h;
            if (Height==0 && Width==0) 
                throw new FilterGraphBuilderException(Error.NoVideoDimension);
    
            pFilterGraph.rcSrc.left = 0;
            pFilterGraph.rcSrc.top = 0;
            pFilterGraph.rcSrc.right = Width;
            pFilterGraph.rcSrc.bottom = Height;
    
            if (!ReadDVDInformation(pFilterGraph, DiscPath)) 
                throw new FilterGraphBuilderException(Error.CantPlayDisc);

            DsUtils.EnumFilters(pFilterGraph.pGraphBuilder, pFilterGraph.aFilters);
#if DEBUG
            pFilterGraph.bAddedToRot = DsUtils.AddToRot(pFilterGraph.pGraphBuilder, out pFilterGraph.dwRegister);
#endif
            pFilterGraph.SourceType = SourceType.DVD;
        }

        //This method parses AM_DVD_RENDERSTATUS and returns a text description 
        //of the error 
        // return value:
        // true - can continue
        // false - stop
        private bool GetStatusText(FilterGraph pGraph, ref AM_DVD_RENDERSTATUS buildStatus, StringBuilder strError)
        {
            bool bRet = true;
            string newLine = "\n";
            string streamFormat = "    - {0}\n";
            if (buildStatus.iNumStreamsFailed > 0)
            {
                strError.AppendFormat(Resources.Resources.dvd_failed_streams_format, 
                    buildStatus.iNumStreamsFailed, buildStatus.iNumStreams).Append(newLine);
                        
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_VIDEO)!=0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_video_stream);
                    bRet = false;
                }
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_AUDIO)!=0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_audio_stream);
                    pGraph.bDVDAudioRendered = false;
                }
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_SUBPIC)!=0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_subpicture_stream);
                    pGraph.bDVDSubpictureRendered = false;
                }
            }

            if (DsHlp.FAILED(buildStatus.hrVPEStatus))
            {
                try
                {
                    StringBuilder buffer = new StringBuilder(200);
                    AMGetErrorText(buildStatus.hrVPEStatus, buffer, buffer.Capacity);
                    strError.Append(buffer.ToString());
                }
                catch
                {
                }
            }
    
            if (buildStatus.bDvdVolInvalid)
            {
                strError.Append(Resources.Resources.dvd_invalid_volume).Append(newLine);
                bRet = false;
            }
            else if (buildStatus.bDvdVolUnknown)
            {
                strError.Append(Resources.Resources.dvd_unknown_volume).Append(newLine);
                bRet = false;
            }
        
            if (buildStatus.bNoLine21In)
                strError.Append(Resources.Resources.dvd_no_line21in).Append(newLine);
        
            if (buildStatus.bNoLine21Out)
                strError.Append(Resources.Resources.dvd_no_line21out).Append(newLine);
        
            return bRet;
        }

        private int SetDVDPlaybackOptions(FilterGraph pGraph)
        {
            int hr;

            if (pGraph.pAMLine21Decoder != null)
            {
                // Disable Line21 (closed captioning) by default
                hr = pGraph.pAMLine21Decoder.SetServiceState(AM_LINE21_CCSTATE.AM_L21_CCSTATE_Off); 
                if (DsHlp.FAILED(hr))
                    return hr;
            }

            // Don't reset DVD on stop.  This prevents the DVD from entering
            // DOMAIN_Stop when we stop playback or during resolution modes changes
            hr = pGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false); 
            if (DsHlp.FAILED(hr))
                return hr;

            // Ignore parental control for this application
            // If this is TRUE, then the nav will send an event and wait for you
            // to respond with AcceptParentalLevelChangeNotification()
            //
            hr = pGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_NotifyParentalLevelChange, false);
            if (DsHlp.FAILED(hr))
                return hr;

            // Use HMSF timecode format (instead of binary coded decimal)
            hr = pGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_HMSF_TimeCodeEvents, true); 
            if (DsHlp.FAILED(hr))
                return hr;

            return hr;
        }

        private bool ReadDVDInformation(FilterGraph pGraph, string lpSource)
        {
            pGraph.info.source = lpSource;
            pGraph.info.StreamSubType=MediaSubType.DVD;
    
            // Read the number of available titles on this disc
            DVD_DISC_SIDE DiscSide;
            int ulNumVolumes=0, ulCurrentVolume=0, ulNumTitles=0;
            int hr = pGraph.pDvdInfo2.GetDVDVolumeInfo(out ulNumVolumes, out ulCurrentVolume,
                out DiscSide, out ulNumTitles);
        
            pGraph.ulNumTitles = ulNumTitles;

            int ulNumOfChapters;
            for (int i=1; i<=ulNumTitles; i++)
            {
                pGraph.pDvdInfo2.GetNumberOfChapters(i, out ulNumOfChapters);
                pGraph.arrayNumChapters.Add(ulNumOfChapters);
            }

            return true;
        }

        private void GetTitleInfo(FilterGraph pGraph)
        {
            int hr;
   
            DVD_HMSF_TIMECODE TotalTime;
            int ulTimeCodeFlags;
            hr = pGraph.pDvdInfo2.GetTotalTitleTime(out TotalTime, out ulTimeCodeFlags);
            if (hr==DsHlp.S_OK) 
            {
                pGraph.rtDuration=TotalTime.bHours*3600 + TotalTime.bMinutes*60 + TotalTime.bSeconds;
                pGraph.rtDuration *= CoreDefinitions.ONE_SECOND;
                pGraph.bSeekable=true;
            }
            else if (hr==DsHlp.VFW_S_DVD_NON_ONE_SEQUENTIAL) 
            {
                // Nonsequential video title
                pGraph.rtDuration=0;
                //		AfxMessageBox("non one seq");
            }
        }

        private bool GetAudioInfo(FilterGraph pGraph)
        {
            int hr;
            int ulStreamsAvailable=0, ulCurrentStream=0;

            // Read the number of audio streams available
            hr = pGraph.pDvdInfo2.GetCurrentAudio(out ulStreamsAvailable, out ulCurrentStream);  
            if (DsHlp.SUCCEEDED(hr))
            {
                // Add an entry for each available audio stream
                bool bEnabled;
                for (int i=0; i < ulStreamsAvailable; i++)
                {
                    int Language;
                    hr = pGraph.pDvdInfo2.GetAudioLanguage(i, out Language);
                    if (DsHlp.FAILED(hr))
                    {
                        pGraph.arrayAudioStream.Add("Unknown");
                        continue; // GetAudioLanguage Failed for language i
                    }
            
                    // Skip this entry if there is no language ID
                    if (Language == 0) 
                    {
                        pGraph.arrayAudioStream.Add("Unknown");
                        continue;
                    }
            
                    CultureInfo ci = new CultureInfo(Language);
                    pGraph.arrayAudioStream.Add(ci.EnglishName);
                            
                    bEnabled=false;
                    hr=pGraph.pDvdInfo2.IsAudioStreamEnabled(i, out bEnabled);
                    if (hr==DsHlp.S_OK && bEnabled)
                    {
                        StreamInfo pStreamInfo = new StreamInfo();
                        GetAudioAttributes(pGraph, pStreamInfo, i, (string)pGraph.arrayAudioStream[i]);
                        pGraph.info.streams.Add(pStreamInfo);
                    }
                }

                pGraph.nAudioStreams=pGraph.arrayAudioStream.Count;
                pGraph.nCurrentAudioStream=ulCurrentStream;
                return true;
            }

            return false;
        }

        private bool GetVideoInfo(FilterGraph pGraph)
        {
            DVD_VideoAttributes atrVideo;
            int hr;

            hr = pGraph.pDvdInfo2.GetCurrentVideoAttributes(out atrVideo);
            if (DsHlp.FAILED(hr))
                return false;
            
            // TRUE means the picture can be shown as pan-scan if the display aspect ratio is 4 x 3
            pGraph.bPanscanPermitted=atrVideo.fPanscanPermitted; // 16:9 is cropped to display as 4:3
            // TRUE means the picture can be shown as letterbox if the display aspect ratio is 4 x 3
            pGraph.bLetterboxPermitted=atrVideo.fLetterboxPermitted;

            StreamInfo pStreamInfo = new StreamInfo();
            pStreamInfo.dwPictAspectRatioX=atrVideo.ulAspectX;
            pStreamInfo.dwPictAspectRatioY=atrVideo.ulAspectY;

            pStreamInfo.rcSrc.right=atrVideo.ulSourceResolutionX;
            pStreamInfo.rcSrc.bottom=atrVideo.ulSourceResolutionY;

            int Width=0;
            int Height=0;
            pGraph.pRenderer.GetNativeVideoSize(out Width, out Height, out pGraph.ARWidth, out pGraph.ARHeight);
            double w = pGraph.ARWidth;
            double h = pGraph.ARHeight;
            pGraph.dAspectRatio=w/h;
            pGraph.rcSrc.left=pGraph.rcSrc.top=0;
            pGraph.rcSrc.right = Width;
            pGraph.rcSrc.bottom = Height;

            pStreamInfo.ulFrameRate=atrVideo.ulFrameRate;
            pStreamInfo.ulFrameHeight=atrVideo.ulFrameHeight;

            pStreamInfo.dvdCompression=atrVideo.Compression;

            // TRUE means there is user data in line 21, field 1
            pGraph.bLine21Field1InGOP=atrVideo.fLine21Field1InGOP;
            // TRUE means there is user data in line 21, field 2
            pGraph.bLine21Field2InGOP=atrVideo.fLine21Field2InGOP;

            pStreamInfo.Flags=StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_ASPECTRATIO | 
                StreamInfoFlags.SI_DVDFRAMERATE | StreamInfoFlags.SI_DVDFRAMEHEIGHT | 
                StreamInfoFlags.SI_DVDCOMPRESSION;
            pGraph.info.streams.Add(pStreamInfo);

            return true;
        }

        private void GetAudioAttributes(FilterGraph pGraph, StreamInfo pStreamInfo, int ulStream, string szStreamName)
        {
            pStreamInfo.strDVDAudioStreamName=szStreamName;
    
            int hr;

            DVD_AudioAttributes audioAtr;
            hr = pGraph.pDvdInfo2.GetAudioAttributes(ulStream, out audioAtr);

            if (DsHlp.FAILED(hr))
                return;
   
            pStreamInfo.AudioFormat=audioAtr.AudioFormat;
            pStreamInfo.dwFrequency=audioAtr.dwFrequency;
            pStreamInfo.Quantization=audioAtr.bQuantization;
            pStreamInfo.nChannels=audioAtr.bNumberOfChannels;

            pStreamInfo.Flags=StreamInfoFlags.SI_DVDAUDIOSTREAMNAME | 
                StreamInfoFlags.SI_DVDAUDIOFORMAT | StreamInfoFlags.SI_DVDFREQUENCY | 
                StreamInfoFlags.SI_DVDQUANTIZATION | StreamInfoFlags.SI_WAVECHANNELS;
        }

        private void GetSubpictureInfo(FilterGraph pGraph)
        {
            int hr;
            int i;

            // Read the number of subpicture streams available
            int ulStreamsAvailable=0, ulCurrentStream=0;
            bool bIsDisabled; // TRUE means it is disabled

            hr = pGraph.pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, out ulCurrentStream, 
                out bIsDisabled);
            if (DsHlp.SUCCEEDED(hr))
            {
                for (i=0; i < ulStreamsAvailable; i++)
                {
                    int Language;
                    hr = pGraph.pDvdInfo2.GetSubpictureLanguage(i, out Language);
                    if (DsHlp.FAILED(hr))
                    {
                        pGraph.arraySubpictureStream.Add("Unknown");
                        continue; // GetAudioLanguage Failed for language i
                    }
            
                    // Skip this entry if there is no language ID
                    if (Language == 0) 
                    {
                        pGraph.arraySubpictureStream.Add("Unknown");
                        continue;
                    }
            
                    CultureInfo ci = new CultureInfo(Language);
                    pGraph.arraySubpictureStream.Add(ci.EnglishName);
                }
            }
        }

        public void UpdateTitleInfo(FilterGraph pGraph)
        {
            DVD_PLAYBACK_LOCATION2 loc;
            int hr = pGraph.pDvdInfo2.GetCurrentLocation(out loc);
    
            ClearTitleInfo(pGraph, loc.TitleNum, loc.ChapterNum);
    
            // Get the current title info (duration, number of chapters...)
            GetTitleInfo(pGraph);

            // Retrieve the video attributes of the current title
            GetVideoInfo(pGraph);
    
            // Read the number of available audio streams
            GetAudioInfo(pGraph);

            // Read the number of subpicture streams supported and configure
            // the subpicture stream menu (often subtitle languages)
            GetSubpictureInfo(pGraph);
        }

        public void GetAngleInfo(FilterGraph pGraph)
        {
            int hr;
            int ulAnglesAvailable=0, ulCurrentAngle=0;

            // Read the number of angles available
            hr = pGraph.pDvdInfo2.GetCurrentAngle(out ulAnglesAvailable, out ulCurrentAngle); 
            if (DsHlp.SUCCEEDED(hr))
            {
                // An angle count of 1 means that the DVD is not in an 
                // angle block
                // NOTE: Since angles range from 1 to 9, start counting at 1 in your loops
                if (ulAnglesAvailable >= 2)
                {
                    pGraph.ulAnglesAvailable=ulAnglesAvailable;
                    pGraph.ulCurrentAngle=ulCurrentAngle;
                }
            }
        }

        public void ClearTitleInfo(FilterGraph pGraph, int ulTitle, int ulChapter)
        {
            pGraph.info.streams.Clear();

            pGraph.nCurrentAudioStream=0;
            pGraph.nAudioStreams=0;
            pGraph.arrayAudioStream.Clear();
            pGraph.arraySubpictureStream.Clear();

            pGraph.bSeekable=false;
            pGraph.dRate=1.0;
    
            pGraph.bPanscanPermitted=false;
            pGraph.bLetterboxPermitted=false;
        
            pGraph.ulAnglesAvailable=1;
            pGraph.ulCurrentAngle=1;
    
            pGraph.ulCurTitle=ulTitle;
            pGraph.ulCurChapter=ulChapter;
            pGraph.ulCurrentAngle=1;
            pGraph.CurTime.bHours=0;
            pGraph.CurTime.bMinutes=0;
            pGraph.CurTime.bSeconds=0;
            pGraph.rtDuration=0;

            pGraph.arrayMenuLang.Clear();
            pGraph.arrayMenuLangLCID.Clear();
        }

        public void ClearTitleInfo(FilterGraph pGraph)
        {
            ClearTitleInfo(pGraph, 0, 0);
        }

        public void GetMenuLanguageInfo(FilterGraph pGraph)
        {
            ClearTitleInfo(pGraph);
    
            int hr;
            int i;

            // Read the number of available menu languages
            int ulLanguagesAvailable=0;

            hr = pGraph.pDvdInfo2.GetMenuLanguages(null, 10, out ulLanguagesAvailable);
            if (DsHlp.FAILED(hr))
                return;

            // Allocate a language array large enough to hold the language list
            int[] pLanguageList = new int[ulLanguagesAvailable];

            // Now fill the language array with the menu languages
            hr = pGraph.pDvdInfo2.GetMenuLanguages(pLanguageList, ulLanguagesAvailable, 
                    out ulLanguagesAvailable);
            if (DsHlp.SUCCEEDED(hr))
            {
                // Add an entry to the menu for each available menu language
                for (i=0; i < ulLanguagesAvailable; i++)
                {
           
                    // Skip this entry if there is no language ID
                    if (pLanguageList[i] == 0) 
                    {
                        pGraph.arrayMenuLang.Add("Unknown");
                        pGraph.arrayMenuLangLCID.Add(pLanguageList[i]);
                        continue;
                    }

                    CultureInfo ci = new CultureInfo(pLanguageList[i]);
                    pGraph.arrayMenuLang.Add(ci.EnglishName);
                    pGraph.arrayMenuLangLCID.Add(pLanguageList[i]);					           
                }
            }
        }
    }
}
