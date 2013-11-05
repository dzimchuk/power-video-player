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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.FilterGraphs;
using Pvp.Core.MediaEngine.Internal;
using Pvp.Core.MediaEngine.Renderers;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    internal class MediaEngine : IMediaEngine
    {
        private IFilterGraph _filterGraph;

        private bool _autoPlay;
        private Renderer _preferredRenderer;
        private bool _repeat;

        private IMediaWindow _mediaWindow;
        private readonly IMediaWindowHost _mediaWindowHost;

        private readonly SynchronizationContext _synchronizationContext;
           
        /// <summary>
        /// Constructor.
        /// </summary>
        public MediaEngine(IMediaWindowHost mediaWindowHost)
        {
            _mediaWindowHost = mediaWindowHost;
            _synchronizationContext = SynchronizationContext.Current;
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
        #endregion

        #region Playback properties and methods
        
        public int AudioStreamsCount
        {
            get { return _filterGraph != null ? _filterGraph.AudioStreamsCount : 0; }
        }

        public int CurrentAudioStream
        {
            get
            {
                if (_filterGraph == null)
                    return -1;

                return _filterGraph.CurrentAudioStream;
            }
            set
            {
                if (_filterGraph == null)
                    return;
                
                var audioStreamsCount = _filterGraph.AudioStreamsCount;
                if (audioStreamsCount == 0 || value > (audioStreamsCount - 1))
                    return;

                _filterGraph.CurrentAudioStream = value;
            }
        }

        public int FilterCount
        {
            get { return _filterGraph != null ? _filterGraph.FilterCount : 0; }
        }

        public GraphState GraphState
        {
            get { return _filterGraph != null ? _filterGraph.GraphState : GraphState.Reset; }
        }

        public bool IsGraphSeekable
        {
            get { return _filterGraph != null && _filterGraph.IsGraphSeekable; }
        }
        
        public MediaInfo MediaInfo
        {
            get { return _filterGraph != null ? _filterGraph.MediaInfo : null; }
        }

        public SourceType SourceType
        {
            get { return _filterGraph != null ? _filterGraph.SourceType : SourceType.Unknown; }
        }

        public string GetAudioStreamName(int nStream)
        {
            var str = Resources.Resources.error;
            if (_filterGraph == null)
                return str;

            var audioStreamsCount = _filterGraph.AudioStreamsCount;
            if (audioStreamsCount == 0 || nStream > (audioStreamsCount - 1))
                return str;

            return _filterGraph.GetAudioStreamName(nStream);
        }

        public long GetCurrentPosition()
        {
            return _filterGraph != null ? _filterGraph.GetCurrentPosition() : 0L;
        }

        public void SetCurrentPosition(long time)
        {
            if (_filterGraph == null)
                return;

            _filterGraph.SetCurrentPosition(time);
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
            return (_filterGraph != null && _filterGraph.IsGraphSeekable) ? _filterGraph.Duration : 0;
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
            return _filterGraph != null ? _filterGraph.GetFilterName(nFilterNum) : string.Empty;
        }

        public double GetRate()
        {
            return _filterGraph != null ? _filterGraph.Rate : 1.0;
        }

        public void SetRate(double dRate)
        {
            if (_filterGraph == null)
                return;

            _filterGraph.SetRate(dRate);
        }

        public double Rate
        {
            get { return GetRate(); }
            set { SetRate(value); }
        }

        public bool GetVolume(out int volume)
        {
            volume = 0;
            return _filterGraph != null && _filterGraph.GetVolume(out volume);
        }

        public bool SetVolume(int volume)
        {
            _volume = CalculateVolumeValue(volume);
            IsMuted = volume == -10000;
            return true;
        }

        private bool SetVolumeInternal(int volume)
        {
            return _filterGraph != null && _filterGraph.SetVolume(volume);
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
                    if (_filterGraph.Renderer is VideoRenderer)
                    {
                        if (currentState != GraphState.Paused)
                            PauseGraph();
                    }

                    if (_filterGraph.Renderer.GetCurrentImage(out header, out dibFull, out dibDataOnly))
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
        /// Render new media.
        /// </summary>
        /// <param name="source">Filename.</param>
        /// <param name="mediaSourceType">One of the MediaSourceType.</param>
        /// <returns></returns>
        public bool BuildGraph(string source, MediaSourceType mediaSourceType)
        {
            ResetGraph();

            _mediaWindow = _mediaWindowHost.GetMediaWindow();
            _mediaWindow.MessageReceived += _mediaWindow_MessageReceived;

            _filterGraph = FilterGraphBuilder.BuildFilterGraph(source,
                                                               mediaSourceType,
                                                               _mediaWindow.Handle,
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
                    _filterGraph.Renderer is IVMRWindowless ? ((IVMRWindowless)_filterGraph.Renderer).VMRWindowlessControl : null,
                    _filterGraph.Renderer is IVMR9Windowless ? ((IVMR9Windowless)_filterGraph.Renderer).VMRWindowlessControl : null,
                    _filterGraph.Renderer is IEVR ? ((IEVR)_filterGraph.Renderer).MFVideoDisplayControl : null);

                _filterGraph.GraphError += (sender, args) => ReportError(args);
                _filterGraph.PlayBackComplete += (sender, arges) =>
                {
                    if (_repeat)
                    {
                        if (_filterGraph.IsGraphSeekable)
                            SetCurrentPosition(0);
                        else // gonna have to think it over!
                            SetCurrentPosition(0);
                    }
                    else
                    {
                        StopGraph();
                    }
                };
                _filterGraph.ErrorAbort += (sender, args) =>
                {
                    ResetGraph();
                    ReportError(String.Format("{0} {1}", Resources.Resources.mw_error_occured,
                        Resources.Resources.mw_play_aborted));
                };
                _filterGraph.FailedStreamsAvailable += OnFailedStreamsAvailableInternal;

                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                if (dvdFilterGraph != null)
                {
                    dvdFilterGraph.ModifyMenu += (sender, args) => OnModifyMenu();
                    dvdFilterGraph.DiscEjected += (sender, args) => ResetGraph();
                    dvdFilterGraph.InitSize += (sender, args) => OnInitSize(false, true);
                    dvdFilterGraph.DvdParentalChange += (sender, args) => args.Raise(this, ref DvdParentalChange);
                }

                OnInitSize(true, true);
                IsMuted = IsMuted; // this resets the volume

                return !_autoPlay || ResumeGraph();
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

            return _filterGraph.PauseGraph();
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

            var result = _filterGraph.ResumeGraph();
            if (!result)
            {
                ResetGraph();
                ReportError(Resources.Resources.mw_play_aborted);
            }

            return result;
        }

        public bool StopGraph()
        {
            if (_filterGraph == null)
                return false;

            return _filterGraph.StopGraph();
        }
        #endregion

        #region DVD specific properties and methods
        public int AnglesAvailable
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 1 : dvdFilterGraph.AnglesAvailable;
            }
        }

        public int CurrentAngle
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 1 : dvdFilterGraph.CurrentAngle;
            }
            set
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;

                if (dvdFilterGraph == null)
                    return;

                dvdFilterGraph.CurrentAngle = value;
            }
        }

        public int CurrentChapter
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.CurrentChapter;
            }
        }

        public int CurrentSubpictureStream
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? -1 : dvdFilterGraph.CurrentSubpictureStream;
            }
            set
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                if (dvdFilterGraph == null)
                    return;

                dvdFilterGraph.CurrentSubpictureStream = value;
            }
        }

        public int CurrentTitle
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.CurrentTitle;
            }
        }

        public bool IsMenuOn 
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph != null && dvdFilterGraph.IsMenuOn;
            }
        }

        public int MenuLangCount
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.MenuLangCount;
            }
        }

        public int NumberOfSubpictureStreams
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.NumberOfSubpictureStreams;
            }
        }

        public int NumberOfTitles
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.NumberOfTitles;
            }
        }

        public VALID_UOP_FLAG UOPS
        {
            get
            {
                var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
                return dvdFilterGraph == null ? 0 : dvdFilterGraph.UOPS;
            }
        }

        public bool EnableSubpicture(bool bEnable)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.EnableSubpicture(bEnable);
        }

        public bool GetCurrentDomain(out DVD_DOMAIN pDomain)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
    
            pDomain = 0;
            return dvdFilterGraph != null && dvdFilterGraph.GetCurrentDomain(out pDomain);
        }

        public string GetMenuLangName(int nLang)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null ? dvdFilterGraph.GetMenuLangName(nLang) : Resources.Resources.error;
        }

        public int GetNumChapters(int ulTitle)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph == null ? 0 : dvdFilterGraph.GetNumChapters(ulTitle);
        }

        public string GetSubpictureStreamName(int nStream)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null ? dvdFilterGraph.GetSubpictureStreamName(nStream) : Resources.Resources.error;
        }

        public void GoTo(int ulTitle, int ulChapter)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null && dvdFilterGraph.GoTo(ulTitle, ulChapter))
            {
                OnInitSize(false, true); // video size and aspect ratio might have changed when entering new title
                OnModifyMenu(); // signal that a (context) menu may need to be updated
            }
        }

        public bool IsAudioStreamEnabled(int ulStreamNum)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.IsAudioStreamEnabled(ulStreamNum);
        }

        public bool IsResumeDvdEnabled()
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.IsResumeDvdEnabled();
        }

        public bool IsSubpictureEnabled()
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.IsSubpictureEnabled();
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.IsSubpictureStreamEnabled(ulStreamNum);
        }

        // The Resume method leaves a menu and resumes playback
        public bool ResumeDvd()
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            return dvdFilterGraph != null && dvdFilterGraph.ResumeDvd();
        }

        public void ReturnFromSubmenu()
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null)
            {
                dvdFilterGraph.ReturnFromSubmenu();
            }
        }

        public void SetMenuLang(int nLang)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null)
            {
                dvdFilterGraph.SetMenuLang(nLang);
            }
        }

        public void ShowMenu(DVD_MENU_ID menuId)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null)
            {
                dvdFilterGraph.ShowMenu(menuId);
            }
        }

        public void ActivateSelectedDvdMenuButton()
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null && dvdFilterGraph.IsMenuOn)
            {
                dvdFilterGraph.ActivateSelectedDvdMenuButton();
            }
        }

        public void SelectDvdMenuButtonUp()
        {
            if (IsMenuOn)
                SelectDvdMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Upper);
        }

        public void SelectDvdMenuButtonDown()
        {
            if (IsMenuOn)
                SelectDvdMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Lower);
        }

        public void SelectDvdMenuButtonLeft()
        {
            if (IsMenuOn)
                SelectDvdMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Left);
        }

        public void SelectDvdMenuButtonRight()
        {
            if (IsMenuOn)
                SelectDvdMenuButton(DVD_RELATIVE_BUTTON.DVD_Relative_Right);
        }

        private void SelectDvdMenuButton(DVD_RELATIVE_BUTTON relativeButton)
        {
            ((IDvdFilterGraph)_filterGraph).SelectDvdMenuButton(relativeButton);
        }

        public void ActivateDvdMenuButtonAtPosition(GDI.POINT point)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null && dvdFilterGraph.IsMenuOn)
                dvdFilterGraph.ActivateDvdMenuButtonAtPosition(point);
        }

        public void SelectDvdMenuButtonAtPosition(GDI.POINT point)
        {
            var dvdFilterGraph = _filterGraph as IDvdFilterGraph;
            if (dvdFilterGraph != null && dvdFilterGraph.IsMenuOn)
                dvdFilterGraph.SelectDvdMenuButtonAtPosition(point);
        }

        #endregion

        #region Others
        public bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay)
        {
            return _filterGraph != null && _filterGraph.DisplayFilterPropPage(hParent, strFilter, bDisplay);
        }

        public bool DisplayFilterPropPage(IntPtr hParent, int nFilterNum, bool bDisplay)
        {
            return DisplayFilterPropPage(hParent, GetFilterName(nFilterNum), bDisplay);
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
            args.Raise(this, ref ErrorOccured);
        }

        protected virtual void OnModifyMenu()
        {
            EventArgs.Empty.Raise(this, ref ModifyMenu);
        }

        private void OnFailedStreamsAvailableInternal(IList<StreamInfo> streams)
        {
            // Raising it on another thread is needed so that BuildGraph could continue.
            ThreadPool.QueueUserWorkItem(delegate(object state) { OnFailedStreamsAvailable((IList<StreamInfo>)state); }, streams);
        }

        protected virtual void OnFailedStreamsAvailable(IList<StreamInfo> streams)
        {
            if (FailedStreamsAvailable != null && _synchronizationContext != null)
            {
                _synchronizationContext.Post(state =>
                                                 {
                                                     var handler = Interlocked.CompareExchange(ref FailedStreamsAvailable, null, null);
                                                     if (handler != null)
                                                     {
                                                         handler((IList<StreamInfo>)state);
                                                     }
                                                 }, streams);
            }
        }

        protected virtual void OnPartialSuccess(UserDecisionEventArgs args)
        {
            args.Raise(this, ref PartialSuccess);
        }
        
        #endregion

        protected virtual void OnInitSize(bool initial, bool suggestInvalidate)
        {
            if (_filterGraph != null)
            {
                var rect = _filterGraph.SourceRect;
                var size = new GDI.SIZE { cx = rect.right - rect.left, cy = rect.bottom - rect.top };
                var args = new CoreInitSizeEventArgs(size, _filterGraph.AspectRatio, initial, suggestInvalidate);
                args.Raise(this, ref InitSize);
            }
        }

        #region HandleGraphEvent
        private void HandleGraphEvent()
        {
            if (_filterGraph != null)
            {
                _filterGraph.HandleGraphEvent();
            }
        }
        #endregion

        #region MediaWindow 'hook'
        
        private void _mediaWindow_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            switch (e.Msg)
            {
                default:
                    if (e.Msg == FilterGraphBase.UWM_GRAPH_NOTIFY)
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
        
        public void SetVideoPosition(GDI.RECT rcDest)
        {
            if (_filterGraph != null)
            {
                _filterGraph.Renderer.SetVideoPosition(_filterGraph.SourceRect, rcDest);
            }
        }
    }
}
