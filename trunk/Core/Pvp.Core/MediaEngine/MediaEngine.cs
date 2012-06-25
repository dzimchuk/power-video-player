﻿/* ****************************************************************************
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.GraphBuilders;
using Pvp.Core.MediaEngine.Render;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    internal class MediaEngine : IMediaEngine
    {
        private RegularFilterGraphBuilder _regularBuilder;
        private DVDFilterGraphBuilder _dvdBuilder;
        private FilterGraph _filterGraph;

        private bool _autoPlay;
        private Renderer _preferredRenderer;
        private bool _repeat;

        private IMediaWindow _mediaWindow;
        private readonly IMediaWindowHost _mediaWindowHost;
           
        /// <summary>
        /// Constructor.
        /// </summary>
        public MediaEngine(IMediaWindowHost mediaWindowHost)
        {
            _mediaWindowHost = mediaWindowHost;

            _regularBuilder = RegularFilterGraphBuilder.GetGraphBuilder();
            _dvdBuilder = DVDFilterGraphBuilder.GetGraphBuilder();

            _regularBuilder.FailedStreamsAvailable += new FailedStreamsHandler(OnFailedStreamsAvailableInternal);
            _dvdBuilder.FailedStreamsAvailable += new FailedStreamsHandler(OnFailedStreamsAvailableInternal);
        }
        
        #region Events

        public event FailedStreamsHandler FailedStreamsAvailable;
        public event EventHandler<ErrorOccuredEventArgs> ErrorOccured;
        public event EventHandler ModifyMenu;
        public event EventHandler<CoreInitSizeEventArgs> InitSize;
        public event EventHandler<UserDecisionEventArgs> DvdParentalChange;
        public event EventHandler<UserDecisionEventArgs> PartialSuccess;
        public event EventHandler UpdateSuggested;

        #endregion

        #region General properties (preferences)
        public bool AutoPlay
        {
            get { return _autoPlay; }
            set { _autoPlay = value; }
        }

        public bool Repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }

        public Renderer PreferredVideoRenderer
        {
            get { return _preferredRenderer; }
            set { _preferredRenderer = value; }
        }

        public bool UsePreferredFilters
        {
            get { return _regularBuilder.UsePreferredFilters; }
            set { _regularBuilder.UsePreferredFilters = value; }
        }

        public bool UsePreferredFilters4DVD
        {
            get { return _dvdBuilder.UsePreferredFilters; }
            set { _dvdBuilder.UsePreferredFilters = value; }
        }
        #endregion

        #region Playback properties and methods
        
        public int AudioStreams
        {
            get { return _filterGraph != null ? _filterGraph.nAudioStreams : 0; }
        }

        public int CurrentAudioStream
        {
            get
            {
                if (_filterGraph == null)
                    return -1;

                if (_filterGraph.SourceType == SourceType.DVD)
                {
                    int ulStreamsAvailable = 0, ulCurrentStream = 0;

                    int hr = _filterGraph.pDvdInfo2.GetCurrentAudio(out ulStreamsAvailable,
                        out ulCurrentStream);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        // Update the current audio language selection
                        _filterGraph.nCurrentAudioStream = ulCurrentStream;
                    }
                }

                return _filterGraph.nCurrentAudioStream;
            }
            set
            {
                if (_filterGraph == null)
                    return;
                else if (_filterGraph.nAudioStreams == 0)
                    return;
                else if (value > (_filterGraph.nAudioStreams - 1))
                    return;
                else if (_filterGraph.SourceType != SourceType.DVD)
                {
                    if (_filterGraph.arrayBasicAudio.Count == 0)
                        return;

                    int lVolume;
                    GetVolume(out lVolume);

                    int lMute = -10000;
                    IBasicAudio pBA;
                    for (int i = 0; i < _filterGraph.nAudioStreams; i++)
                    {
                        pBA = (IBasicAudio)_filterGraph.arrayBasicAudio[i];
                        pBA.put_Volume(lMute);
                    }

                    _filterGraph.nCurrentAudioStream = value;
                    SetVolume(lVolume);
                }
                else
                {
                    int hr;

                    // Set the audio stream to the requested value
                    // Note that this does not affect the subpicture data (subtitles)
                    IDvdCmd pObj;
                    hr = _filterGraph.pDvdControl2.SelectAudioStream(value,
                        DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        if (pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }
                        _filterGraph.nCurrentAudioStream = value;
                    }
                }
            }
        }

        public int FilterCount
        {
            get { return _filterGraph != null ? _filterGraph.aFilters.Count : 0; }
        }

        public GraphState GraphState
        {
            get { return _filterGraph != null ? _filterGraph.GraphState : GraphState.Reset; }
        }

        public bool IsGraphSeekable
        {
            get
            {
                if (_filterGraph != null)
                {
                    if (_filterGraph.bSeekable &&
                        (_filterGraph.UOPS & VALID_UOP_FLAG.UOP_FLAG_Play_Title_Or_AtTime) == 0)
                        return true;

                    return false;
                }
                else
                    return false;
            }
        }
        
        public MediaInfo MediaInfo
        {
            get { return _filterGraph != null ? _filterGraph.info : null; }
        }

        public SourceType SourceType
        {
            get { return _filterGraph != null ? _filterGraph.SourceType : SourceType.Unknown; }
        }

        public string GetAudioStreamName(int nStream)
        {
            string str = Resources.Resources.error;
            if (_filterGraph == null)
                return str;

            if (_filterGraph.nAudioStreams == 0)
                return str;

            if (nStream > (_filterGraph.nAudioStreams - 1))
                return str;

            if (_filterGraph.SourceType != SourceType.DVD)
            {
                str = String.Format(Resources.Resources.mw_stream_format, nStream + 1);
            }
            else
            {
                if (_filterGraph.arrayAudioStream.Count == 0)
                    return str;

                str = (string)_filterGraph.arrayAudioStream[nStream];
            }

            return str;
        }

        public long GetCurrentPosition()
        {
            if (_filterGraph == null)
                return 0;
            if (_filterGraph.bSeekable)
            {
                if (_filterGraph.SourceType != SourceType.DVD)
                {
                    _filterGraph.pMediaSeeking.GetCurrentPosition(out _filterGraph.rtCurrentTime);
                }
                else
                {
                    _filterGraph.rtCurrentTime = _filterGraph.CurTime.bHours * 3600 +
                        _filterGraph.CurTime.bMinutes * 60 + _filterGraph.CurTime.bSeconds;
                    _filterGraph.rtCurrentTime *= CoreDefinitions.ONE_SECOND;
                }

                return _filterGraph.rtCurrentTime;
            }

            return 0;
        }

        public TimeSpan CurrentPosition
        {
            get
            {
                var time = GetCurrentPosition();
                var totalSeconds = time / CoreDefinitions.ONE_SECOND;
                var remain = totalSeconds % 3600;
                var h = totalSeconds / 3600;
                var minute = remain / 60;
                var second = remain % 60;

                return new TimeSpan((int)h, (int)minute, (int)second);
            }
            set
            {
                SetCurrentPosition((value.Hours * 3600 + value.Minutes * 60 + value.Seconds) * CoreDefinitions.ONE_SECOND);
            }
        }

        public long GetDuration()
        {
            if (_filterGraph == null)
                return 0;
            if (_filterGraph.bSeekable)
                return _filterGraph.rtDuration;
            return 0;
        }

        public TimeSpan Duration
        {
            get
            {
                var duration = GetDuration();
                var totalSeconds = duration / CoreDefinitions.ONE_SECOND;
                var remain = totalSeconds % 3600;
                var h = totalSeconds / 3600;
                var minute = remain / 60;
                var second = remain % 60;

                return new TimeSpan((int)h, (int)minute, (int)second);
            }
        }

        public string GetFilterName(int nFilterNum)
        {
            if (_filterGraph != null && _filterGraph.aFilters.Count > nFilterNum)
                return (string)_filterGraph.aFilters[nFilterNum];
            else
                return String.Empty;
        }

        public double GetRate()
        {
            return _filterGraph != null ? _filterGraph.dRate : 1.0;
        }

        public bool GetVolume(out int volume)
        {
            volume = 0;
            if (_filterGraph == null)
                return false;
            if (_filterGraph.SourceType == SourceType.DVD)
                return _filterGraph.pBasicAudio.get_Volume(out volume) == DsHlp.S_OK;
            if (_filterGraph.arrayBasicAudio.Count == 0)
                return false;
            IBasicAudio pBA = (IBasicAudio)_filterGraph.arrayBasicAudio[_filterGraph.nCurrentAudioStream];
            return pBA.get_Volume(out volume) == DsHlp.S_OK;
        }

        public void SetCurrentPosition(long time)
        {
            if (_filterGraph == null)
                return;
            if (IsGraphSeekable)
            {
                int hr;
                _filterGraph.rtCurrentTime = time;
                if (_filterGraph.SourceType != SourceType.DVD)
                {
                    GraphState state = _filterGraph.GraphState;
                    PauseGraph();
                    long pStop = 0;
                    hr = _filterGraph.pMediaSeeking.SetPositions(ref _filterGraph.rtCurrentTime,
                        SeekingFlags.AbsolutePositioning,
                        ref pStop, SeekingFlags.NoPositioning);

                    switch (state)
                    {
                        case GraphState.Running:
                            ResumeGraph();
                            break;
                        case GraphState.Stopped:
                            _filterGraph.pMediaControl.Stop();
                            _filterGraph.GraphState = GraphState.Stopped;
                            break;
                    }

                }
                else
                {
                    DVD_PLAYBACK_LOCATION2 loc;
                    hr = _filterGraph.pDvdInfo2.GetCurrentLocation(out loc);
                    if (hr == DsHlp.S_OK)
                    {
                        long second;
                        long minute;
                        long h;
                        long remain;

                        second = time / CoreDefinitions.ONE_SECOND;
                        remain = second % 3600;
                        h = second / 3600;
                        minute = remain / 60;
                        second = remain % 60;

                        loc.TimeCode.bHours = (byte)h;
                        loc.TimeCode.bMinutes = (byte)minute;
                        loc.TimeCode.bSeconds = (byte)second;

                        double rate = GetRate();
                        IDvdCmd pObj;
                        GraphState state = _filterGraph.GraphState;
                        if (state == GraphState.Paused)
                            ResumeGraph();
                        hr = _filterGraph.pDvdControl2.PlayAtTime(ref loc.TimeCode,
                            DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);

                        if (DsHlp.SUCCEEDED(hr))
                        {
                            _filterGraph.CurTime = loc.TimeCode;
                            if (pObj != null)
                            {
                                pObj.WaitForEnd();
                                Marshal.ReleaseComObject(pObj);
                            }
                            if (rate != 1.0)
                                SetRate(rate);
                        }
                        if (state == GraphState.Paused)
                            PauseGraph();
                    }
                }
            }
        }

        public void SetRate(double dRate)
        {
            if (_filterGraph == null)
                return;
            int hr;
            if (_filterGraph.bSeekable)
            {
                if (_filterGraph.SourceType != SourceType.DVD)
                {
                    hr = _filterGraph.pMediaSeeking.SetRate(dRate);
                    if (hr == DsHlp.S_OK)
                        _filterGraph.dRate = dRate;
                }
                else
                {
                    // set rate for DVD
                    IDvdCmd pObj;
                    hr = _filterGraph.pDvdControl2.PlayForwards(dRate,
                        DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        if (pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }
                        _filterGraph.dRate = dRate;
                    }
                }

            }
        }

        public bool SetVolume(int volume)
        {
            _volume = CalculateVolumeValue(volume);
            IsMuted = volume == -10000;
            return true;
        }

        private bool SetVolumeInternal(int volume)
        {
            if (_filterGraph == null)
                return false;
            if (_filterGraph.SourceType == SourceType.DVD)
                return _filterGraph.pBasicAudio.put_Volume(volume) == DsHlp.S_OK;
            if (_filterGraph.arrayBasicAudio.Count == 0)
                return false;
            IBasicAudio pBA = (IBasicAudio)_filterGraph.arrayBasicAudio[_filterGraph.nCurrentAudioStream];
            return pBA.put_Volume(volume) == DsHlp.S_OK;
        }

        public double Rate
        {
            get { return GetRate(); }
            set { SetRate(value); }
        }

        private const int VOLUME_RANGE = -5000;
        private int CalculateVolumeValue(double volume)
        {
            return (int)(VOLUME_RANGE * (1.0 - volume));
        }

        private double CalculateVolumeValue(int volume)
        {
            if (volume > 0)
                throw new ArgumentException("Volume value cannot be greater than 0.");

            if (volume < VOLUME_RANGE)
                return 0.0;

            return 1.0 - volume / VOLUME_RANGE;
        }

        private double _volume = 0.5;
        public double Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentException("Volume should be between 0 and 1.");
                
                if (!IsMuted)
                {
                    SetVolumeInternal(CalculateVolumeValue(value));
                }

                _volume = value;
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }
            set
            {
                if (value)
                    SetVolumeInternal(-10000);
                else
                    SetVolumeInternal(CalculateVolumeValue(_volume));

                _isMuted = value;
            }
        }

        public void GetCurrentImage(IImageCreator imageCreator)
        {
            if (_filterGraph != null)
            {
                GraphState currentState = GraphState;
                
                IntPtr dibFull = IntPtr.Zero;
                IntPtr dibDataOnly;
                BITMAPINFOHEADER header;
                try
                {
                    if (_filterGraph.pRenderer is VideoRenderer)
                    {
                        if (currentState != GraphState.Paused)
                            PauseGraph();
                    }

                    if (_filterGraph.pRenderer.GetCurrentImage(out header, out dibFull, out dibDataOnly))
                        imageCreator.CreateImage(ref header, dibDataOnly);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(dibFull);
                    if (GraphState != currentState)
                    {
                        switch (currentState)
                        {
                            case GraphState.Running:
                                ResumeGraph();
                                break;
                            case GraphState.Stopped:
                                StopGraph();
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Playback control methods
        /// <summary>
        /// Render a new video.
        /// </summary>
        /// <param name="mediaWindow">An instance of the media window.
        /// </param>
        /// <param name="source">Filename.</param>
        /// <param name="mediaSourceType">One of the MediaSourceType.</param>
        /// <returns></returns>
        public bool BuildGraph(string source, MediaSourceType mediaSourceType)
        {
            return BuildGraph(source, mediaSourceType, null);
        }

        /// <summary>
        /// Render a new video using a custom renderer.
        /// </summary>
        /// <param name="mediaWindow">An instance of the media window.
        /// </param>
        /// <param name="source">Filename.</param>
        /// <param name="mediaSourceType">One of the MediaSourceType.</param>
        /// <param name="renderer">A custom renderer.</param>
        /// <returns></returns>
        public bool BuildGraph(string source, MediaSourceType mediaSourceType, RendererBase renderer)
        {
            ResetGraph();

            _mediaWindow = _mediaWindowHost.GetMediaWindow();
            _mediaWindow.MessageReceived += new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);

            _filterGraph = FilterGraphBuilder.BuildFilterGraph(source,
                                                               mediaSourceType,
                                                               _mediaWindow.Handle,
                                                               renderer,
                                                               _preferredRenderer,
                                                               ReportError,
                                                               delegate(string message)
                                                               {
                                                                   UserDecisionEventArgs args = new UserDecisionEventArgs(message);
                                                                   OnPartialSuccess(args);
                                                                   return args.Accept;
                                                               });
            if (_filterGraph != null)
            {
                _mediaWindow.SetRendererInterfaces(
                    _filterGraph.pRenderer is IVMRWindowless ? ((IVMRWindowless)_filterGraph.pRenderer).VMRWindowlessControl : null,
                    _filterGraph.pRenderer is IVMR9Windowless ? ((IVMR9Windowless)_filterGraph.pRenderer).VMRWindowlessControl : null,
                    _filterGraph.pRenderer is IEnhancedVideoRenderer ? ((IEnhancedVideoRenderer)_filterGraph.pRenderer).MFVideoDisplayControl : null);
                OnInitSize(true, true);
                IsMuted = IsMuted; // this resets the volume
                if (_autoPlay)
                    return ResumeGraph();
                else
                    _filterGraph.GraphState = GraphState.Stopped;
                return true;
            }
            else
            {
                CleanUpMediaWindow();
                return false;
            }
        }

        public bool PauseGraph()
        {
            if (_filterGraph == null)
                return false;
            int hr = _filterGraph.pMediaControl.Pause();
            if (hr == DsHlp.S_OK)
            {
                _filterGraph.GraphState = GraphState.Paused;
                return true;
            }
            else if (hr == DsHlp.S_FALSE)
            {
                if (UpdateGraphState())
                    return _filterGraph.GraphState == GraphState.Paused;

                _filterGraph.pMediaControl.Stop();
            }

            _filterGraph.GraphState = GraphState.Stopped; // Pause() failed
            return false;
        }

        public void ResetGraph()
        {
            if (_filterGraph != null)
            {
                _mediaWindow.SetRendererInterfaces(null, null, null);
                _filterGraph.Dispose();
                _filterGraph = null;

                CleanUpMediaWindow();
            }
        }

        public bool ResumeGraph()
        {
            if (_filterGraph == null)
                return false;
            if (DsHlp.SUCCEEDED(_filterGraph.pMediaControl.Run()))
            {
                _filterGraph.GraphState = GraphState.Running;
                return true; // ok, we're running
            }
            else if (UpdateGraphState())
            {
                _filterGraph.GraphState = GraphState.Running;
                return true; // ok, we're running
            }
            else
            {
                ResetGraph();
                ReportError(Resources.Resources.mw_play_aborted);
                return false;
            }
        }

        public bool StopGraph()
        {
            if (_filterGraph == null)
                return false;
            if (_filterGraph.SourceType != SourceType.DVD)
            {
                PauseGraph();
                SetCurrentPosition(0);
                _filterGraph.pMediaControl.Stop();
                _filterGraph.GraphState = GraphState.Stopped;
            }
            else
            {
                _filterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);

                _filterGraph.pMediaControl.Stop();
                _filterGraph.GraphState = GraphState.Stopped;

                _filterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
            }

            return true;
        }
        #endregion

        #region DVD specific properties and methods
        public int AnglesAvailable
        {
            get { return _filterGraph == null ? 1 : _filterGraph.ulAnglesAvailable; }
        }

        public int CurrentAngle
        {
            get { return _filterGraph == null ? 1 : _filterGraph.ulCurrentAngle; }
            set
            {
                if (_filterGraph == null)
                    return;
                else if (_filterGraph.pDvdControl2 == null)
                    return;
                else if (_filterGraph.ulAnglesAvailable < 2)
                    return;
                else if (value > _filterGraph.ulAnglesAvailable)
                    return;
                else
                {
                    int hr;

                    // Set the angle to the requested value.
                    IDvdCmd pObj;
                    hr = _filterGraph.pDvdControl2.SelectAngle(value,
                        DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        if (pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }
                        _filterGraph.ulCurrentAngle = value;
                    }
                }
            }
        }

        public int CurrentChapter
        {
            get { return _filterGraph == null ? 0 : _filterGraph.ulCurChapter; }
        }

        public int CurrentSubpictureStream
        {
            get
            {
                if (_filterGraph == null)
                    return -1;

                if (_filterGraph.pDvdInfo2 == null)
                    return -1;

                int ulStreamsAvailable = 0, ulCurrentStream = 0;
                bool bIsDisabled; // TRUE means it is disabled

                int hr = _filterGraph.pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable,
                    out ulCurrentStream, out bIsDisabled);
                if (DsHlp.SUCCEEDED(hr))
                    return ulCurrentStream;

                return -1;
            }
            set
            {
                if (_filterGraph == null)
                    return;

                if (_filterGraph.arraySubpictureStream.Count == 0)
                    return;

                if (value > (_filterGraph.arraySubpictureStream.Count - 1))
                    return;

                if (_filterGraph.pDvdControl2 == null)
                    return;

                IDvdCmd pObj;
                int hr = _filterGraph.pDvdControl2.SelectSubpictureStream(value,
                    DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                if (DsHlp.SUCCEEDED(hr) && pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
            }
        }

        public int CurrentTitle
        {
            get { return _filterGraph == null ? 0 : _filterGraph.ulCurTitle; }
        }

        public bool IsMenuOn 
        {
            get { return _filterGraph != null && _filterGraph.bMenuOn; }
        }

        public int MenuLangCount
        {
            get { return _filterGraph == null ? 0 : _filterGraph.arrayMenuLang.Count; }
        }

        public int NumberOfSubpictureStreams
        {
            get { return _filterGraph == null ? 0 : (int)_filterGraph.arraySubpictureStream.Count; }
        }

        public int NumberOfTitles
        {
            get { return _filterGraph == null ? 0 : _filterGraph.ulNumTitles; }
        }

        public VALID_UOP_FLAG UOPS
        {
            get { return _filterGraph == null ? 0 : _filterGraph.UOPS; }
        }

        public bool EnableSubpicture(bool bEnable)
        {
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdControl2 == null)
                return false;

            IDvdCmd pObj;
            int hr = _filterGraph.pDvdControl2.SetSubpictureState(bEnable,
                DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
            if (DsHlp.SUCCEEDED(hr) && pObj != null)
            {
                pObj.WaitForEnd();
                Marshal.ReleaseComObject(pObj);
                return true;
            }
            return false;
        }

        public bool GetCurrentDomain(out DVD_DOMAIN pDomain)
        {
            //	DVD_DOMAIN_FirstPlay
            //  DVD_DOMAIN_VideoManagerMenu 
            //  DVD_DOMAIN_VideoTitleSetMenu  
            //  DVD_DOMAIN_Title         
            //  DVD_DOMAIN_Stop     
            pDomain = 0;
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdInfo2 == null)
                return false;

            int hr = _filterGraph.pDvdInfo2.GetCurrentDomain(out pDomain);
            return hr == DsHlp.S_OK;
        }

        public string GetMenuLangName(int nLang)
        {
            string str = Resources.Resources.error;
            if (_filterGraph == null)
                return str;

            if (_filterGraph.arrayMenuLang.Count == 0)
                return str;

            if (nLang > (_filterGraph.arrayMenuLang.Count - 1))
                return str;

            return (string)_filterGraph.arrayMenuLang[nLang];
        }

        public int GetNumChapters(int ulTitle)
        {
            if (_filterGraph == null)
                return 0;

            int ulcount = _filterGraph.arrayNumChapters.Count;
            if (ulcount < ulTitle)
                return 0;

            return (int)_filterGraph.arrayNumChapters[ulTitle - 1];
        }

        public string GetSubpictureStreamName(int nStream)
        {
            string str = Resources.Resources.error;
            if (_filterGraph == null)
                return str;

            if (_filterGraph.arraySubpictureStream.Count == 0)
                return str;

            if (nStream > (_filterGraph.arraySubpictureStream.Count - 1))
                return str;

            return (string)_filterGraph.arraySubpictureStream[nStream];
        }

        public void GoTo(int ulTitle, int ulChapter)
        {
            if (_filterGraph != null && _filterGraph.pDvdControl2 != null &&
                ulTitle <= _filterGraph.ulNumTitles && ulChapter <= GetNumChapters(ulTitle))
            {
                IDvdCmd pObj;
                int hr = _filterGraph.pDvdControl2.PlayChapterInTitle(ulTitle, ulChapter,
                    DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (pObj != null)
                    {
                        pObj.WaitForEnd();
                        Marshal.ReleaseComObject(pObj);
                    }
                    if (_filterGraph.CurDomain == DVD_DOMAIN.DVD_DOMAIN_Title &&
                        _filterGraph.ulCurTitle != ulTitle)
                    {
                        _dvdBuilder.UpdateTitleInfo(_filterGraph);
                        OnInitSize(false, true); // video size and aspect ratio might have changed when entering new title
                        OnModifyMenu(); // signal that a (context) menu may need to be updated
                    }

                }
            }
        }

        public bool IsAudioStreamEnabled(int ulStreamNum)
        {
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdInfo2 == null)
                return false;

            bool bEnabled;
            int hr = _filterGraph.pDvdInfo2.IsAudioStreamEnabled(ulStreamNum, out bEnabled);
            return (hr == DsHlp.S_OK) ? bEnabled : false;
        }

        public bool IsResumeDVDEnabled()
        {
            if (_filterGraph == null)
                return false;

            DVD_DOMAIN domain = _filterGraph.CurDomain;
            return ((domain == DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu) || (domain == DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu))
                && (_filterGraph.UOPS & VALID_UOP_FLAG.UOP_FLAG_Resume) == 0 &&
                _filterGraph.bShowMenuCalledFromTitle;
        }

        public bool IsSubpictureEnabled()
        {
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdInfo2 == null)
                return false;

            int ulStreamsAvailable = 0, ulCurrentStream = 0;
            bool bIsDisabled; // TRUE means it is disabled

            int hr = _filterGraph.pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable,
                out ulCurrentStream, out bIsDisabled);
            if (DsHlp.SUCCEEDED(hr))
                return !bIsDisabled;

            return false;
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdInfo2 == null)
                return false;

            bool bEnabled;
            int hr = _filterGraph.pDvdInfo2.IsSubpictureStreamEnabled(ulStreamNum, out bEnabled);
            return (hr == DsHlp.S_OK) ? bEnabled : false;
        }

        // The Resume method leaves a menu and resumes playback
        public bool ResumeDVD()
        {
            if (_filterGraph == null)
                return false;

            if (_filterGraph.pDvdControl2 == null)
                return false;

            IDvdCmd pObj;
            int hr = _filterGraph.pDvdControl2.Resume(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
            if (DsHlp.SUCCEEDED(hr))
            {
                if (pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
                _filterGraph.bMenuOn = false;
                return true;
            }

            return false;
        }

        public void ReturnFromSubmenu()
        {
            if (_filterGraph == null)
                return;

            if (_filterGraph.pDvdControl2 == null)
                return;

            IDvdCmd pObj;
            int hr = _filterGraph.pDvdControl2.ReturnFromSubmenu(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush,
                out pObj);
            if (DsHlp.SUCCEEDED(hr) && pObj != null)
            {
                pObj.WaitForEnd();
                Marshal.ReleaseComObject(pObj);
            }
        }

        public void SetMenuLang(int nLang)
        {
            if (_filterGraph == null)
                return;

            if (_filterGraph.arrayMenuLangLCID.Count == 0)
                return;

            if (nLang > (_filterGraph.arrayMenuLangLCID.Count - 1))
                return;

            int hr;
            hr = _filterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);

            // Changing menu language is only valid in the DVD_DOMAIN_Stop domain
            hr = _filterGraph.pDvdControl2.Stop();
            if (DsHlp.SUCCEEDED(hr))
            {
                // Change the default menu language
                hr = _filterGraph.pDvdControl2.SelectDefaultMenuLanguage((int)_filterGraph.arrayMenuLangLCID[nLang]);

                // Display the root menu
                ShowMenu(DVD_MENU_ID.DVD_MENU_Title);
            }

            // Turn off ResetOnStop option 
            hr = _filterGraph.pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
        }

        public void ShowMenu(DVD_MENU_ID menuID)
        {
            if (_filterGraph == null)
                return;

            DVD_DOMAIN domain = _filterGraph.CurDomain;
            IDvdCmd pObj;
            int hr = _filterGraph.pDvdControl2.ShowMenu(menuID,
                DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
            if (DsHlp.SUCCEEDED(hr))
            {
                if (pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
                _filterGraph.bMenuOn = true;
                if (!_filterGraph.bShowMenuCalledFromTitle)
                {
                    _filterGraph.bShowMenuCalledFromTitle = domain == DVD_DOMAIN.DVD_DOMAIN_Title;
                }
            }
        }

        public void ActivateSelectedDVDMenuButton()
        {
            if (IsMenuOn)
                _filterGraph.pDvdControl2.ActivateButton();
        }

        public void SelectDVDMenuButtonUp()
        {
            if (IsMenuOn)
                SelectDVDMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Upper);
        }

        public void SelectDVDMenuButtonDown()
        {
            if (IsMenuOn)
                SelectDVDMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Lower);
        }

        public void SelectDVDMenuButtonLeft()
        {
            if (IsMenuOn)
                SelectDVDMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Left);
        }

        public void SelectDVDMenuButtonRight()
        {
            if (IsMenuOn)
                SelectDVDMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Right);
        }

        private void SelectDVDMenuButton(DVD_RELATIVE_BUTTON relativeButton)
        {
            _filterGraph.pDvdControl2.SelectRelativeButton(relativeButton);
        }

        public void ActivateDVDMenuButtonAtPosition(GDI.POINT point)
        {
            if (IsMenuOn)
                _filterGraph.pDvdControl2.ActivateAtPosition(point);
        }

        public void SelectDVDMenuButtonAtPosition(GDI.POINT point)
        {
            if (IsMenuOn)
                _filterGraph.pDvdControl2.SelectAtPosition(point);
        }

        #endregion

        #region Others
        public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
        {
            if (_filterGraph == null)
                return false;

            IBaseFilter pFilter = null;
            _filterGraph.pGraphBuilder.FindFilterByName(strFilter, out pFilter);
            if (pFilter == null)
                return false;

            bool bRet = false;
            ISpecifyPropertyPages pProp = pFilter as ISpecifyPropertyPages;
            if (pProp != null)
            {
                bRet = true;
                if (bDisplay)
                {
                    // Show the page. 
                    CAUUID caGUID = new CAUUID();
                    pProp.GetPages(out caGUID);

                    object pFilterUnk = (object)pFilter;
                    DsUtils.OleCreatePropertyFrame(
                        hParent,                // Parent window
                        0, 0,                   // Reserved
                        strFilter,				// Caption for the dialog box
                        1,                      // Number of objects (just the filter)
                        ref pFilterUnk,			// Array of object pointers. 
                        caGUID.cElems,          // Number of property pages
                        caGUID.pElems,          // Array of property page CLSIDs
                        0,                      // Locale identifier
                        0, IntPtr.Zero          // Reserved
                        );

                    // Clean up.
                    Marshal.FreeCoTaskMem(caGUID.pElems);
                }

            //    Marshal.ReleaseComObject(pProp);
            }

        //    Marshal.ReleaseComObject(pFilter);
            return bRet;
        }

        public bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay)
        {
            return DisplayFilterPropPage(hParent, GetFilterName(nFilterNum), bDisplay);
        }

        public void OnCultureChanged()
        {
            FilterGraph.ReadErrorMessages();
        }
        #endregion

        #region Internal stuff
        
        private void CleanUpMediaWindow()
        {
            _mediaWindow.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(_mediaWindow_MessageReceived);
            _mediaWindow.Dispose();
            _mediaWindow = null;

            OnMediaWindowDisposed(EventArgs.Empty);
        }

        private void ReportError(string error)
        {
            OnErrorOccured(new ErrorOccuredEventArgs(error.Replace("\\n", "\n")));
        }
        
        protected virtual void OnErrorOccured(ErrorOccuredEventArgs args)
        {
            if (ErrorOccured != null)
                ErrorOccured(this, args);
        }

        protected virtual void OnModifyMenu()
        {
            if (ModifyMenu != null)
                ModifyMenu(this, EventArgs.Empty);
        }

        private void OnFailedStreamsAvailableInternal(IList<StreamInfo> streams)
        {
            ThreadPool.QueueUserWorkItem(delegate(object state) { OnFailedStreamsAvailable((IList<StreamInfo>)state); }, streams);
        }

        protected virtual void OnFailedStreamsAvailable(IList<StreamInfo> streams)
        {
            if (FailedStreamsAvailable != null)
                FailedStreamsAvailable(streams);
        }

        protected virtual void OnPartialSuccess(UserDecisionEventArgs args)
        {
            if (PartialSuccess != null)
                PartialSuccess(this, args);
        }

        //This function will return FALSE if the state is RESET or unidentified!!!!
        //The application must determine whether to stop the graph.
        private bool UpdateGraphState()
        {
            if (_filterGraph == null)
                return false;

            int hr;
            FilterState fs;
            hr = _filterGraph.pMediaControl.GetState(2000, out fs);
            if (hr == DsHlp.S_OK)
            {
                switch (fs)
                {
                    case FilterState.State_Stopped:
                        _filterGraph.GraphState = GraphState.Stopped;
                        break;
                    case FilterState.State_Paused:
                        _filterGraph.GraphState = GraphState.Paused;
                        break;
                    case FilterState.State_Running:
                        _filterGraph.GraphState = GraphState.Running;
                        break;
                }

                return true;
            }

            if (hr == DsHlp.VFW_S_CANT_CUE)
            {
                _filterGraph.GraphState = GraphState.Paused;
                return true;
            }
            else if (hr == DsHlp.VFW_S_STATE_INTERMEDIATE) //Don't know what the state is so just stay at the old one
                return true;
            else
                return false;
        }
        #endregion

        protected virtual void OnInitSize(bool initial, bool suggestInvalidate)
        {
            if (_filterGraph != null)
            {
                if (InitSize != null)
                {
                    GDI.SIZE size = new GDI.SIZE();
                    size.cx = _filterGraph.rcSrc.right - _filterGraph.rcSrc.left;
                    size.cy = _filterGraph.rcSrc.bottom - _filterGraph.rcSrc.top;
                    InitSize(this, new CoreInitSizeEventArgs(size, _filterGraph.dAspectRatio, initial, suggestInvalidate));
                }
            }
        }

        #region HandleGraphEvent
        private void HandleGraphEvent()
        {
            if (_filterGraph == null)
                return;

            int evCode, lParam1, lParam2;

            bool bEjected = false;
            while (DsHlp.SUCCEEDED(_filterGraph.pMediaEventEx.GetEvent(out evCode, out lParam1, out lParam2, 0)))
            {
                int hr;
                switch (evCode)
                {
                    case (int)DsEvCode.Complete:
                        if (_repeat)
                        {
                            if (_filterGraph.bSeekable)
                                SetCurrentPosition(0);
                            else // gonna have to think it over!
                                SetCurrentPosition(0);
                        }
                        else
                            StopGraph();
                        break;
                    case (int)DsEvCode.ErrorAbort:
                        ResetGraph();
                        ReportError(String.Format("{0} {1}", Resources.Resources.mw_error_occured,
                            Resources.Resources.mw_play_aborted));
                        break;
                    ////// DVD cases ///////
                    case (int)DsEvCode.DvdCurrentHmsfTime:
                        Guid guid = new Guid(lParam1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        byte[] abyte = guid.ToByteArray();
                        _filterGraph.CurTime.bHours = abyte[0];
                        _filterGraph.CurTime.bMinutes = abyte[1];
                        _filterGraph.CurTime.bSeconds = abyte[2];
                        _filterGraph.CurTime.bFrames = abyte[3];
                        break;
                    case (int)DsEvCode.DvdChaptStart:
                        _filterGraph.ulCurChapter = lParam1;
                        break;
                    case (int)DsEvCode.DvdAngleChange:
                        // lParam1 is the number of available angles (1 means no multiangle support)
                        // lParam2 is the current angle, Angle numbers range from 1 to 9
                        _filterGraph.ulAnglesAvailable = lParam1;
                        _filterGraph.ulCurrentAngle = lParam2;
                        break;
                    case (int)DsEvCode.DvdAnglesAvail:
                        // Read the number of available angles
                        _dvdBuilder.GetAngleInfo(_filterGraph);
                        OnModifyMenu();
                        break;
                    case (int)DsEvCode.DvdNoFpPgc: // disc doesn't have a First Play Program Chain
                        IDvdCmd pObj;
                        hr = _filterGraph.pDvdControl2.PlayTitle(1,
                            DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                        if (DsHlp.SUCCEEDED(hr) && pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }
                        break;
                    case (int)DsEvCode.DvdDomChange:
                        switch (lParam1)
                        {
                            case (int)DVD_DOMAIN.DVD_DOMAIN_FirstPlay:  // = 1 (Performing default initialization of a DVD disc)
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_Stop:       // = 5
                                _dvdBuilder.ClearTitleInfo(_filterGraph);
                                _filterGraph.bMenuOn = false;
                                HandleDiscEject(ref bEjected);
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu:  // = 2
                            case (int)DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu: // = 3
                                // Inform the app to update the menu option to show "Resume" now
                                _filterGraph.bMenuOn = true;  // now menu is "On"
                                _dvdBuilder.GetMenuLanguageInfo(_filterGraph);
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_Title:      // = 4
                                // Inform the app to update the menu option to show "Menu" again
                                _filterGraph.bMenuOn = false; // we are no longer in a menu
                                _filterGraph.bShowMenuCalledFromTitle = false;
                                _dvdBuilder.UpdateTitleInfo(_filterGraph);
                                OnInitSize(false, true); // video size and aspect ratio might have changed when entering new title
                                break;
                        } // end of domain change switch
                        _filterGraph.CurDomain = (DVD_DOMAIN)lParam1;
                        OnModifyMenu();
                        break;
                    case (int)DsEvCode.DvdValidUopsChange:
                        _filterGraph.UOPS = (VALID_UOP_FLAG)lParam1;
                        break;
                    case (int)DsEvCode.DvdPlaybStopped:
                        //	StopGraph();
                        break;
                    case (int)DsEvCode.DvdParentalLChange:
                        if (DvdParentalChange != null)
                        {
                            string str = String.Format(Resources.Resources.accept_parental_level_format, lParam1);
                            UserDecisionEventArgs args = new UserDecisionEventArgs(str);
                            DvdParentalChange(this, args);
                            _filterGraph.pDvdControl2.AcceptParentalLevelChange(args.Accept);
                        }
                        else
                            _filterGraph.pDvdControl2.AcceptParentalLevelChange(false);
                        break;
                    case (int)DsEvCode.DvdError:
                        switch (lParam1)
                        {
                            case (int)DVD_ERROR.DVD_ERROR_Unexpected: // Playback is stopped.
                                ReportError(Resources.Resources.mw_dvd_unexpected_error);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_CopyProtectFail:
                                ReportError(Resources.Resources.mw_dvd_copyprotect_failed);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_InvalidDVD1_0Disc:
                                ReportError(Resources.Resources.mw_dvd_invalid_disc);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_InvalidDiscRegion:
                                ReportError(Resources.Resources.mw_dvd_invalid_region);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                //	ChangeDvdRegion(); // details in the dvdcore.cpp
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_LowParentalLevel:
                                ReportError(Resources.Resources.mw_dvd_low_parental_level);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_MacrovisionFail:
                                ReportError(Resources.Resources.mw_dvd_macrovision_error);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_IncompatibleSystemAndDecoderRegions:
                                ReportError(Resources.Resources.mw_dvd_system_decoder_regions);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_IncompatibleDiscAndDecoderRegions:
                                ReportError(Resources.Resources.mw_dvd_disc_decoder_regions);
                                _filterGraph.pMediaControl.Stop();
                                _filterGraph.GraphState = GraphState.Stopped;
                                break;
                        }  // end of switch (lParam1)
                        break;
                    // Next is warning
                    case (int)DsEvCode.DvdWarning:
                        switch (lParam1)
                        {
                            case (int)DVD_WARNING.DVD_WARNING_InvalidDVD1_0Disc:
                                //		ReportError("DVD Warning: Current disc is not v1.0 spec compliant");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_FormatNotSupported:
                                //		ReportError("DVD Warning: The decoder does not support the new format.");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_IllegalNavCommand:
                                //		ReportError("DVD Warning: An illegal navigation command was encountered.");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Open:
                                ReportError(Resources.Resources.mw_dvd_warning_cant_open_file);
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Seek:
                                ReportError(Resources.Resources.mw_dvd_warning_cant_seek);
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Read:
                                ReportError(Resources.Resources.mw_dvd_warning_cant_read);
                                break;
                            default:
                                //		ReportError("DVD Warning: An unknown (%ld) warning received.");
                                break;
                        }
                        break;
                    case (int)DsEvCode.DvdButtonChange:
                        break;
                    case (int)DsEvCode.DvdStillOn:
                        if (lParam1 != 0) // if there is a still without buttons, we can call StillOff
                            _filterGraph.bStillOn = true;
                        break;
                    case (int)DsEvCode.DvdStillOff:
                        _filterGraph.bStillOn = false; // we are no longer in a still
                        break;
                } // end of switch(..)

                _filterGraph.pMediaEventEx.FreeEventParams(evCode, lParam1, lParam2);
            } // end of while(...)

            if (bEjected)
                ResetGraph();
        }

        void HandleDiscEject(ref bool bEjected)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Storage.MAX_PATH * 2);
            int ulActualSize;
            int hr = _filterGraph.pDvdInfo2.GetDVDDirectory(ptr, Storage.MAX_PATH, out ulActualSize);
            if (hr == DsHlp.S_OK)
            {
                string path = Marshal.PtrToStringUni(ptr, ulActualSize);
                if (path.Length >= 3)
                {
                    path = path.Substring(0, 3);
                    uint MaximumComponentLength, FileSystemFlags, VolumeSerialNumber;
                    int nMode = NoCat.SetErrorMode(NoCat.SEM_FAILCRITICALERRORS);
                    if (Storage.GetVolumeInformation(path, null, 0, out VolumeSerialNumber,
                        out MaximumComponentLength, out FileSystemFlags, null, 0) == 0)
                        bEjected = true;
                    NoCat.SetErrorMode(nMode);
                }
            }
            Marshal.FreeCoTaskMem(ptr);
        }
        #endregion

        #region MediaWindow 'hook'
        
        private void _mediaWindow_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            switch (e.Msg)
            {
                default:
                    if (e.Msg == (uint)FilterGraph.UWM_GRAPH_NOTIFY)
                    {
                        HandleGraphEvent();
                        if (UpdateSuggested != null)
                            UpdateSuggested(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        #endregion

        public event EventHandler MediaWindowDisposed;

        protected virtual void OnMediaWindowDisposed(EventArgs args)
        {
            if (MediaWindowDisposed != null)
                MediaWindowDisposed(this, args);
        }
        
        public void SetVideoPosition(ref GDI.RECT rcDest)
        {
            if (_filterGraph != null)
            {
                _filterGraph.pRenderer.SetVideoPosition(ref _filterGraph.rcSrc, ref rcDest);
            }
        }
    }
}