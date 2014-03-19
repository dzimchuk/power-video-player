using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.Renderers;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine.FilterGraphs
{
    internal class DvdFilterGraph : FilterGraphBase, IDvdFilterGraph
    {
        [DllImport("quartz.dll", CharSet = CharSet.Auto)]
        private static extern uint AMGetErrorText(int hr, StringBuilder pBuffer, int maxLen);

        // DVD interfaces
        private IDvdGraphBuilder _pDvdGraphBuilder;
        private IDvdInfo2 _pDvdInfo2;
        private IDvdControl2 _pDvdControl2;
        private IAMLine21Decoder _pAmLine21Decoder;

        // Audio streams stuff
        private int _audioStreamsCount;
        private int _currentAudioStream;

        private bool _dvdAudioRendered = true;
        private bool _dvdSubpictureRendered = true;

        // DVD related variables
        private int _ulNumTitles;
        private readonly List<int> _arrayNumChapters; // number of chapters in each title
        private readonly List<string> _arrayAudioStream;
        private readonly List<string> _arrayMenuLang;
        private readonly List<int> _arrayMenuLangLcid;
        private readonly List<string> _arraySubpictureStream;
        private bool _bPanscanPermitted;
        private bool _bLetterboxPermitted;
        private bool _bLine21Field1InGOP;
        private bool _bLine21Field2InGOP;
        private int _ulAnglesAvailable = 1;
        private int _ulCurrentAngle = 1;
        
        private bool _bShowMenuCalledFromTitle;
        private VALID_UOP_FLAG _uops;
        private DVD_DOMAIN _curDomain = DVD_DOMAIN.DVD_DOMAIN_Stop;
        private int _ulCurChapter;            // track the current chapter number
        private int _ulCurTitle;              // track the current title number
        private DVD_HMSF_TIMECODE _curTime;   // track the current playback time
        private bool _bMenuOn;                // we are in a menu
        private bool _bStillOn;               // used to track if there is a still frame on or not

        public DvdFilterGraph()
        {
            _arrayNumChapters = new List<int>();
            _arrayAudioStream = new List<string>();
            _arrayMenuLang = new List<string>();
            _arrayMenuLangLcid = new List<int>();
            _arraySubpictureStream = new List<string>();

            _aspectRatio = 1.0;
            _rate = 1.00;
        }

        protected override void CloseInterfaces()
        {
            base.CloseInterfaces();

            if (_pDvdGraphBuilder != null)
            {
                while (Marshal.ReleaseComObject(_pDvdGraphBuilder) > 0) { }
                _pDvdGraphBuilder = null;
            }

            if (_pAmLine21Decoder != null)
            {
                while (Marshal.ReleaseComObject(_pAmLine21Decoder) > 0) { }
                _pAmLine21Decoder = null;
            }

            if (_pDvdControl2 != null)
            {
                Marshal.ReleaseComObject(_pDvdControl2);
                _pDvdControl2 = null;
            }

            if (_pDvdInfo2 != null)
            {
                Marshal.ReleaseComObject(_pDvdInfo2);
                _pDvdInfo2 = null;
            }
        }

        public event EventHandler ModifyMenu;
        public event EventHandler DiscEjected;
        public event EventHandler InitSize;
        public event EventHandler<UserDecisionEventArgs> DvdParentalChange;

        protected virtual void OnModifyMenu()
        {
            EventArgs.Empty.Raise(this, ref ModifyMenu);
        }

        protected virtual void OnDiscEjected()
        {
            EventArgs.Empty.Raise(this, ref DiscEjected);
        }

        protected virtual void OnInitSize()
        {
            EventArgs.Empty.Raise(this, ref InitSize);
        }

        protected virtual void OnDvdParentalChange(UserDecisionEventArgs args)
        {
            args.Raise(this, ref DvdParentalChange);
        }

        public override SourceType SourceType
        {
            get { return SourceType.Dvd; }
        }

        public override void BuildUp(FilterGraphBuilderParameters parameters)
        {
            // Create DVD Graph Builder
            object comobj = null;
            try
            {
                var type = Type.GetTypeFromCLSID(Clsid.DvdGraphBuilder, true);
                comobj = Activator.CreateInstance(type);
                _pDvdGraphBuilder = (IDvdGraphBuilder)comobj;
                comobj = null; // important! (see the finally block)
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.DvdGraphBuilder, e);
            }
            finally
            {
                if (comobj != null)
                {
                    Marshal.FinalReleaseComObject(comobj);
                }
            }

            InitializeGraphBuilder(() =>
                                       {
                                           IGraphBuilder graphBuilder;
                                           var hresult = _pDvdGraphBuilder.GetFiltergraph(out graphBuilder);
                                           hresult.ThrowExceptionForHR(GraphBuilderError.DvdGraphBuilder);
                                           return graphBuilder;
                                       });

            // It is important to QI for IMediaEventEx *before* building the graph so that
            // the app can catch all of the DVD Navigator's initialization events.  Once
            // an app QI's for IMediaEventEx, DirectShow will start queuing up events and 
            // the app will receive them when it sets the notify window. If the app does not
            // QI for IMediaEventEx before building the graph, these events will just be lost.
            InitializeMediaEventEx(parameters.MediaWindowHandle);

            // request desired renderer (may return null)            
            Renderer = RendererBase.AddRenderer(_pDvdGraphBuilder,
                                                GraphBuilder,
                                                parameters.PreferredVideoRenderer,
                                                parameters.MediaWindowHandle);

            // Build the Graph
            string str;
            AM_DVD_RENDERSTATUS buildStatus;
            var hr = _pDvdGraphBuilder.RenderDvdVideoVolume(parameters.DiscPath, parameters.Flags, out buildStatus);
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
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_VIDEO) != 0)
                {
                    FixUpVideoStream(ref buildStatus);
                }

                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_SUBPIC) != 0)
                {
                    FixUpSubpictureStream(ref buildStatus);
                }

                StringBuilder strError = new StringBuilder();
                bool bOk = GetStatusText(ref buildStatus, strError);
                str = strError.Length == 0 ? Resources.Resources.dvd_unknown_error : strError.ToString();
                if (!bOk)
                {
                    throw new FilterGraphBuilderException(str);
                }

                if (strError.Length != 0)
                {
                    if (!parameters.OnPartialSuccessCallback(str + "\n" + Resources.Resources.dvd_question_continue))
                    {
                        throw new AbortException();
                    }
                }
            }

            // The graph was successfully rendered in some form if we get this far
            // We will now instantiate all of the necessary interfaces
            if (Renderer == null)
                Renderer = RendererBase.GetExistingRenderer(GraphBuilder, parameters.MediaWindowHandle);

            object o;
            var guid = typeof(IDvdInfo2).GUID;
            hr = _pDvdGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.FAILED(hr))
            {
                Guid IID_IDvdInfo = new Guid("A70EFE60-E2A3-11d0-A9BE-00AA0061BE93");
                hr = _pDvdGraphBuilder.GetDvdInterface(ref IID_IDvdInfo, out o);
                if (DsHlp.SUCCEEDED(hr))
                    str = Resources.Resources.dvd_incompatible_dshow;
                else
                    str = Resources.Resources.error_cant_retrieve_all_interfaces;
                throw new FilterGraphBuilderException(str);
            }

            _pDvdInfo2 = (IDvdInfo2)o;

            guid = typeof(IDvdControl2).GUID;
            hr = _pDvdGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.FAILED(hr))
            {
                Guid IID_IDvdControl = new Guid("A70EFE61-E2A3-11d0-A9BE-00AA0061BE93");
                hr = _pDvdGraphBuilder.GetDvdInterface(ref IID_IDvdControl, out o);
                if (DsHlp.SUCCEEDED(hr))
                    str = Resources.Resources.dvd_incompatible_dshow;
                else
                    str = Resources.Resources.error_cant_retrieve_all_interfaces;
                throw new FilterGraphBuilderException(str);
            }

            _pDvdControl2 = (IDvdControl2)o;

            // this one may or may not be present
            guid = typeof(IAMLine21Decoder).GUID;
            hr = _pDvdGraphBuilder.GetDvdInterface(ref guid, out o);
            if (DsHlp.SUCCEEDED(hr))
            {
                _pAmLine21Decoder = (IAMLine21Decoder)o;
            }

            InitializeMediaControl();
            InitializeBasicAudio();

            // Set DVD Navigator options
            SetDvdPlaybackOptions();

            int width, arWidth;
            int height, arHeight;
            Renderer.GetNativeVideoSize(out width, out height, out arWidth, out arHeight);
            double w = arWidth;
            double h = arHeight;
            _aspectRatio = w / h;
            if (height == 0 && width == 0)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.NoVideoDimension);
            }

            _sourceRect = new GDI.RECT { left = 0, top = 0, right = width, bottom = height };

            if (!ReadDvdInformation(parameters.DiscPath))
            {
                throw new FilterGraphBuilderException(GraphBuilderError.CantPlayDisc);
            }
        }

        private void FixUpVideoStream(ref AM_DVD_RENDERSTATUS buildStatus)
        {
            var outputPin = DsUtils.GetPinByMediaType(GraphBuilder, PinDirection.Output, false, MediaType.Video);
            if (outputPin == null)
            {
                return;
            }

            var ok = true;

            if (Renderer != null)
            {
                IPin pVideoRendererInputPin = Renderer.GetInputPin();
                if (pVideoRendererInputPin != null)
                {
                    var hr = GraphBuilder.Connect(outputPin, pVideoRendererInputPin);
                    Marshal.ReleaseComObject(pVideoRendererInputPin);

                    if (DsHlp.FAILED(hr))
                    {
                        hr = GraphBuilder.Render(outputPin);
                        if (DsHlp.FAILED(hr))
                        {
                            ok = false;
                        }
                    }
                }
            }
            else
            {
                var hr = GraphBuilder.Render(outputPin);
                if (DsHlp.FAILED(hr))
                {
                    ok = false;
                }
            }

            Marshal.ReleaseComObject(outputPin);

            if (ok)
            {
                buildStatus.dwFailedStreamsFlag = buildStatus.dwFailedStreamsFlag ^ AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_VIDEO;
                buildStatus.iNumStreamsFailed = buildStatus.iNumStreamsFailed - 1;
            }
        }

        private void FixUpSubpictureStream(ref AM_DVD_RENDERSTATUS buildStatus)
        {
            var outputPin = DsUtils.GetPinBySubType(GraphBuilder, PinDirection.Output, false, MediaSubType.DVD_SUBPICTURE);
            if (outputPin == null)
            {
                return;
            }

            var ok = true;

            var hr = GraphBuilder.Render(outputPin);
            if (DsHlp.FAILED(hr))
            {
                ok = false;
            }

            Marshal.ReleaseComObject(outputPin);

            if (ok)
            {
                buildStatus.dwFailedStreamsFlag = buildStatus.dwFailedStreamsFlag ^ AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_SUBPIC;
                buildStatus.iNumStreamsFailed = buildStatus.iNumStreamsFailed - 1;
            }
        }

        //This method parses AM_DVD_RENDERSTATUS and returns a text description 
        //of the error 
        // return value:
        // true - can continue
        // false - stop
        private bool GetStatusText(ref AM_DVD_RENDERSTATUS buildStatus, StringBuilder strError)
        {
            var bRet = true;
            const string newLine = "\n";
            const string streamFormat = "    - {0}\n";
            if (buildStatus.iNumStreamsFailed > 0)
            {
                strError.AppendFormat(Resources.Resources.dvd_failed_streams_format,
                    buildStatus.iNumStreamsFailed, buildStatus.iNumStreams).Append(newLine);

                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_VIDEO) != 0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_video_stream);
                    bRet = false;
                }
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_AUDIO) != 0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_audio_stream);
                    _dvdAudioRendered = false;
                }
                if ((buildStatus.dwFailedStreamsFlag & AM_DVD_STREAM_FLAGS.AM_DVD_STREAM_SUBPIC) != 0)
                {
                    strError.AppendFormat(streamFormat, Resources.Resources.dvd_subpicture_stream);
                    _dvdSubpictureRendered = false;
                }
            }

            if (DsHlp.FAILED(buildStatus.hrVPEStatus))
            {
                try
                {
                    var buffer = new StringBuilder(200);
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

        private int SetDvdPlaybackOptions()
        {
            int hr;

            if (_pAmLine21Decoder != null)
            {
                // Disable Line21 (closed captioning) by default
                hr = _pAmLine21Decoder.SetServiceState(AM_LINE21_CCSTATE.AM_L21_CCSTATE_Off);
                if (DsHlp.FAILED(hr))
                    return hr;
            }

            // Don't reset DVD on stop.  This prevents the DVD from entering
            // DOMAIN_Stop when we stop playback or during resolution modes changes
            hr = _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
            if (DsHlp.FAILED(hr))
                return hr;

            // Ignore parental control for this application
            // If this is TRUE, then the nav will send an event and wait for you
            // to respond with AcceptParentalLevelChangeNotification()
            //
            hr = _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_NotifyParentalLevelChange, false);
            if (DsHlp.FAILED(hr))
                return hr;

            // Use HMSF timecode format (instead of binary coded decimal)
            hr = _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_HMSF_TimeCodeEvents, true);
            if (DsHlp.FAILED(hr))
                return hr;

            return hr;
        }

        private bool ReadDvdInformation(string lpSource)
        {
            AddToMediaInfo(lpSource);
            AddToMediaInfo(MediaSubType.DVD);

            // Read the number of available titles on this disc
            DVD_DISC_SIDE discSide;
            int ulNumVolumes, ulCurrentVolume, ulNumTitles;
            _pDvdInfo2.GetDVDVolumeInfo(out ulNumVolumes, out ulCurrentVolume,
                                        out discSide, out ulNumTitles);

            _ulNumTitles = ulNumTitles;

            for (int i = 1; i <= ulNumTitles; i++)
            {
                int ulNumOfChapters;
                _pDvdInfo2.GetNumberOfChapters(i, out ulNumOfChapters);
                _arrayNumChapters.Add(ulNumOfChapters);
            }

            return true;
        }

        private void GetTitleInfo()
        {
            DVD_HMSF_TIMECODE totalTime;
            int ulTimeCodeFlags;
            int hr = _pDvdInfo2.GetTotalTitleTime(out totalTime, out ulTimeCodeFlags);
            if (hr == DsHlp.S_OK)
            {
                _duration = totalTime.bHours * 3600 + totalTime.bMinutes * 60 + totalTime.bSeconds;
                _duration *= CoreDefinitions.ONE_SECOND;
                _isGraphSeekable = true;
            }
            else if (hr == DsHlp.VFW_S_DVD_NON_ONE_SEQUENTIAL)
            {
                // Nonsequential video title
                _duration = 0;
            }
        }

        private bool GetAudioInfo()
        {
            int ulStreamsAvailable, ulCurrentStream;

            // Read the number of audio streams available
            int hr = _pDvdInfo2.GetCurrentAudio(out ulStreamsAvailable, out ulCurrentStream);
            if (DsHlp.SUCCEEDED(hr))
            {
                // Add an entry for each available audio stream
                for (int i = 0; i < ulStreamsAvailable; i++)
                {
                    int language;
                    hr = _pDvdInfo2.GetAudioLanguage(i, out language);
                    if (DsHlp.FAILED(hr))
                    {
                        _arrayAudioStream.Add("Unknown");
                        continue; // GetAudioLanguage Failed for language i
                    }

                    // Skip this entry if there is no language ID
                    if (language == 0)
                    {
                        _arrayAudioStream.Add("Unknown");
                        continue;
                    }

                    var ci = new CultureInfo(language);
                    _arrayAudioStream.Add(ci.EnglishName);

                    bool bEnabled;
                    hr = _pDvdInfo2.IsAudioStreamEnabled(i, out bEnabled);
                    if (hr == DsHlp.S_OK && bEnabled)
                    {
                        StreamInfo pStreamInfo = new StreamInfo();
                        GetAudioAttributes(pStreamInfo, i, _arrayAudioStream[i]);
                        AddToMediaInfo(pStreamInfo);
                    }
                }

                _audioStreamsCount = _arrayAudioStream.Count;
                _currentAudioStream = ulCurrentStream;
                return true;
            }

            return false;
        }

        private bool GetVideoInfo()
        {
            DVD_VideoAttributes atrVideo;
            int hr;

            hr = _pDvdInfo2.GetCurrentVideoAttributes(out atrVideo);
            if (DsHlp.FAILED(hr))
                return false;

            // TRUE means the picture can be shown as pan-scan if the display aspect ratio is 4 x 3
            _bPanscanPermitted = atrVideo.fPanscanPermitted; // 16:9 is cropped to display as 4:3
            // TRUE means the picture can be shown as letterbox if the display aspect ratio is 4 x 3
            _bLetterboxPermitted = atrVideo.fLetterboxPermitted;

            StreamInfo pStreamInfo = new StreamInfo();
            pStreamInfo.dwPictAspectRatioX = atrVideo.ulAspectX;
            pStreamInfo.dwPictAspectRatioY = atrVideo.ulAspectY;

            pStreamInfo.rcSrc.right = atrVideo.ulSourceResolutionX;
            pStreamInfo.rcSrc.bottom = atrVideo.ulSourceResolutionY;

            int width, arWidth;
            int height, arHeight;
            Renderer.GetNativeVideoSize(out width, out height, out arWidth, out arHeight);
            double w = arWidth;
            double h = arHeight;
            _aspectRatio = w / h;
            _sourceRect = new GDI.RECT { left = 0, top = 0, right = width, bottom = height };

            pStreamInfo.ulFrameRate = atrVideo.ulFrameRate;
            pStreamInfo.ulFrameHeight = atrVideo.ulFrameHeight;

            pStreamInfo.dvdCompression = atrVideo.Compression;

            // TRUE means there is user data in line 21, field 1
            _bLine21Field1InGOP = atrVideo.fLine21Field1InGOP;
            // TRUE means there is user data in line 21, field 2
            _bLine21Field2InGOP = atrVideo.fLine21Field2InGOP;

            pStreamInfo.Flags = StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_ASPECTRATIO |
                StreamInfoFlags.SI_DVDFRAMERATE | StreamInfoFlags.SI_DVDFRAMEHEIGHT |
                StreamInfoFlags.SI_DVDCOMPRESSION;
            AddToMediaInfo(pStreamInfo);

            return true;
        }

        private void GetAudioAttributes(StreamInfo pStreamInfo, int ulStream, string szStreamName)
        {
            pStreamInfo.strDVDAudioStreamName = szStreamName;

            int hr;

            DVD_AudioAttributes audioAtr;
            hr = _pDvdInfo2.GetAudioAttributes(ulStream, out audioAtr);

            if (DsHlp.FAILED(hr))
                return;

            pStreamInfo.AudioFormat = audioAtr.AudioFormat;
            pStreamInfo.dwFrequency = audioAtr.dwFrequency;
            pStreamInfo.Quantization = audioAtr.bQuantization;
            pStreamInfo.nChannels = audioAtr.bNumberOfChannels;

            pStreamInfo.Flags = StreamInfoFlags.SI_DVDAUDIOSTREAMNAME |
                StreamInfoFlags.SI_DVDAUDIOFORMAT | StreamInfoFlags.SI_DVDFREQUENCY |
                StreamInfoFlags.SI_DVDQUANTIZATION | StreamInfoFlags.SI_WAVECHANNELS;
        }

        private void GetSubpictureInfo()
        {
            int hr;
            int i;

            // Read the number of subpicture streams available
            int ulStreamsAvailable = 0, ulCurrentStream = 0;
            bool bIsDisabled; // TRUE means it is disabled

            hr = _pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, out ulCurrentStream,
                out bIsDisabled);
            if (DsHlp.SUCCEEDED(hr))
            {
                for (i = 0; i < ulStreamsAvailable; i++)
                {
                    int language;
                    hr = _pDvdInfo2.GetSubpictureLanguage(i, out language);
                    if (DsHlp.FAILED(hr))
                    {
                        _arraySubpictureStream.Add("Unknown");
                        continue; // GetAudioLanguage Failed for language i
                    }

                    // Skip this entry if there is no language ID
                    if (language == 0)
                    {
                        _arraySubpictureStream.Add("Unknown");
                        continue;
                    }

                    CultureInfo ci = new CultureInfo(language);
                    _arraySubpictureStream.Add(ci.EnglishName);
                }
            }
        }

        public void UpdateTitleInfo()
        {
            DVD_PLAYBACK_LOCATION2 loc;
            var hr = _pDvdInfo2.GetCurrentLocation(out loc);

            ClearTitleInfo(loc.TitleNum, loc.ChapterNum);

            // Get the current title info (duration, number of chapters...)
            GetTitleInfo();

            // Retrieve the video attributes of the current title
            GetVideoInfo();

            // Read the number of available audio streams
            GetAudioInfo();

            // Read the number of subpicture streams supported and configure
            // the subpicture stream menu (often subtitle languages)
            GetSubpictureInfo();
        }

        public void GetAngleInfo()
        {
            int hr;
            int ulAnglesAvailable = 0, ulCurrentAngle = 0;

            // Read the number of angles available
            hr = _pDvdInfo2.GetCurrentAngle(out ulAnglesAvailable, out ulCurrentAngle);
            if (DsHlp.SUCCEEDED(hr))
            {
                // An angle count of 1 means that the DVD is not in an 
                // angle block
                // NOTE: Since angles range from 1 to 9, start counting at 1 in your loops
                if (ulAnglesAvailable >= 2)
                {
                    _ulAnglesAvailable = ulAnglesAvailable;
                    _ulCurrentAngle = ulCurrentAngle;
                }
            }
        }

        public void ClearTitleInfo(int ulTitle, int ulChapter)
        {
            ClearStreamsInfo();

            _currentAudioStream = 0;
            _audioStreamsCount = 0;
            _arrayAudioStream.Clear();
            _arraySubpictureStream.Clear();

            _isGraphSeekable = false;
            _rate = 1.0;

            _bPanscanPermitted = false;
            _bLetterboxPermitted = false;

            _ulAnglesAvailable = 1;
            _ulCurrentAngle = 1;

           _ulCurTitle = ulTitle;
           _ulCurChapter = ulChapter;
           _ulCurrentAngle = 1;
            _curTime.bHours = 0;
            _curTime.bMinutes = 0;
            _curTime.bSeconds = 0;
            _duration = 0;

            _arrayMenuLang.Clear();
            _arrayMenuLangLcid.Clear();
        }

        public void ClearTitleInfo()
        {
            ClearTitleInfo(0, 0);
        }

        public void GetMenuLanguageInfo()
        {
            ClearTitleInfo();

            int hr;
            int i;

            // Read the number of available menu languages
            int ulLanguagesAvailable = 0;

            hr = _pDvdInfo2.GetMenuLanguages(null, 10, out ulLanguagesAvailable);
            if (DsHlp.FAILED(hr))
                return;

            // Allocate a language array large enough to hold the language list
            int[] pLanguageList = new int[ulLanguagesAvailable];

            // Now fill the language array with the menu languages
            hr = _pDvdInfo2.GetMenuLanguages(pLanguageList, ulLanguagesAvailable,
                    out ulLanguagesAvailable);
            if (DsHlp.SUCCEEDED(hr))
            {
                // Add an entry to the menu for each available menu language
                for (i = 0; i < ulLanguagesAvailable; i++)
                {

                    // Skip this entry if there is no language ID
                    if (pLanguageList[i] == 0)
                    {
                        _arrayMenuLang.Add("Unknown");
                        _arrayMenuLangLcid.Add(pLanguageList[i]);
                        continue;
                    }

                    CultureInfo ci = new CultureInfo(pLanguageList[i]);
                    _arrayMenuLang.Add(ci.EnglishName);
                    _arrayMenuLangLcid.Add(pLanguageList[i]);
                }
            }
        }

        private bool _isGraphSeekable;
        public override bool IsGraphSeekable
        {
            get
            {
                return _isGraphSeekable && (_uops & VALID_UOP_FLAG.UOP_FLAG_Play_Title_Or_AtTime) == 0;
            }
        }

        private long _duration;
        public override long Duration
        {
            get { return _duration; }
        }

        private double _rate;
        public override double Rate
        {
            get { return _rate; }
        }

        public override long GetCurrentPosition()
        {
            long currentTime = _curTime.bHours * 3600 + _curTime.bMinutes * 60 + _curTime.bSeconds;
            currentTime *= CoreDefinitions.ONE_SECOND;
            return currentTime;
        }

        public override void SetCurrentPosition(long time)
        {
            if (IsGraphSeekable)
            {
                DVD_PLAYBACK_LOCATION2 loc;
                var hr = _pDvdInfo2.GetCurrentLocation(out loc);
                if (hr == DsHlp.S_OK)
                {
                    long second = time / CoreDefinitions.ONE_SECOND;
                    long remain = second % 3600;
                    long h = second / 3600;
                    long minute = remain / 60;
                    second = remain % 60;

                    loc.TimeCode.bHours = (byte)h;
                    loc.TimeCode.bMinutes = (byte)minute;
                    loc.TimeCode.bSeconds = (byte)second;

                    double rate = Rate;
                    IDvdCmd pObj;
                    var state = GraphState;
                    if (state == GraphState.Paused)
                    {
                        ResumeGraph();
                    }

                    hr = _pDvdControl2.PlayAtTime(ref loc.TimeCode, DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);

                    if (DsHlp.SUCCEEDED(hr))
                    {
                        _curTime = loc.TimeCode;
                        if (pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }
                        if (rate != 1.0)
                            SetRate(rate);
                    }
                    
                    if (state == GraphState.Paused)
                    {
                        PauseGraph();
                    }
                }
            }
        }

        public override void SetRate(double rate)
        {
            if (IsGraphSeekable)
            {
                IDvdCmd pObj;
                var hr = _pDvdControl2.PlayForwards(rate, DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (pObj != null)
                    {
                        pObj.WaitForEnd();
                        Marshal.ReleaseComObject(pObj);
                    }

                    _rate = rate;
                }
            }
        }

        public override bool StopGraph()
        {
            _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);

            MediaControl.Stop();
            GraphState = GraphState.Stopped;

            _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);

            return true;
        }

        public override int AudioStreamsCount
        {
            get { return _audioStreamsCount; }
        }

        public override int CurrentAudioStream
        {
            get
            {
                int ulStreamsAvailable, ulCurrentStream;

                var hr = _pDvdInfo2.GetCurrentAudio(out ulStreamsAvailable, out ulCurrentStream);
                if (DsHlp.SUCCEEDED(hr))
                {
                    // Update the current audio language selection
                    _currentAudioStream = ulCurrentStream;
                }

                return _currentAudioStream;
            }
            set
            {
                // Set the audio stream to the requested value
                // Note that this does not affect the subpicture data (subtitles)
                IDvdCmd pObj;
                var hr = _pDvdControl2.SelectAudioStream(value, DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (pObj != null)
                    {
                        pObj.WaitForEnd();
                        Marshal.ReleaseComObject(pObj);
                    }
                    _currentAudioStream = value;
                }
            }
        }

        private GDI.RECT _sourceRect;
        public override GDI.RECT SourceRect
        {
            get { return _sourceRect; }
        }

        private double _aspectRatio;
        public override double AspectRatio
        {
            get { return _aspectRatio; }
        }

        public override string GetAudioStreamName(int nStream)
        {
            if (_arrayAudioStream == null || _arrayAudioStream.Count == 0)
                return string.Empty;

            return _arrayAudioStream[nStream];
        }

        public override bool SetVolume(int volume)
        {
            return BasicAudio.put_Volume(volume) == DsHlp.S_OK;
        }

        public override bool GetVolume(out int volume)
        {
            return BasicAudio.get_Volume(out volume) == DsHlp.S_OK;
        }

        protected override void OnExternalStreamSelection()
        {
        }

        public int AnglesAvailable
        {
            get { return _ulAnglesAvailable; }
        }

        public int CurrentAngle
        {
            get { return _ulCurrentAngle; }
            set
            {
                if (_ulAnglesAvailable < 2 || value > _ulAnglesAvailable)
                    return;

                // Set the angle to the requested value.
                IDvdCmd pObj;
                var hr = _pDvdControl2.SelectAngle(value, DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (pObj != null)
                    {
                        pObj.WaitForEnd();
                        Marshal.ReleaseComObject(pObj);
                    }
                    _ulCurrentAngle = value;
                }
            }
        }

        public int CurrentChapter
        {
            get { return _ulCurChapter; }
        }

        public int CurrentSubpictureStream
        {
            get
            {
                int ulStreamsAvailable, ulCurrentStream;
                bool bIsDisabled; // TRUE means it is disabled

                var hr = _pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, out ulCurrentStream, out bIsDisabled);
                if (DsHlp.SUCCEEDED(hr))
                    return ulCurrentStream;

                return -1;
            }
            set
            {
                if (_arraySubpictureStream.Count == 0 || value > (_arraySubpictureStream.Count - 1))
                    return;

                IDvdCmd pObj;
                var hr = _pDvdControl2.SelectSubpictureStream(value, DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                if (DsHlp.SUCCEEDED(hr) && pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
            }
        }

        public int CurrentTitle
        {
            get { return _ulCurTitle; }
        }

        public bool IsMenuOn
        {
            get { return _bMenuOn; }
        }

        public int MenuLangCount
        {
            get { return _arrayMenuLang.Count; }
        }

        public int NumberOfSubpictureStreams
        {
            get { return _arraySubpictureStream.Count; }
        }

        public int NumberOfTitles
        {
            get { return _ulNumTitles; }
        }

        public VALID_UOP_FLAG UOPS
        {
            get { return _uops; }
        }

        public bool EnableSubpicture(bool bEnable)
        {
            IDvdCmd pObj;
            var hr = _pDvdControl2.SetSubpictureState(bEnable, DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
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
            return _pDvdInfo2.GetCurrentDomain(out pDomain) == DsHlp.S_OK;
        }

        public string GetMenuLangName(int nLang)
        {
            var str = Resources.Resources.error;

            if (_arrayMenuLang.Count == 0)
                return str;

            if (nLang > (_arrayMenuLang.Count - 1))
                return str;

            return _arrayMenuLang[nLang];
        }

        public int GetNumChapters(int ulTitle)
        {
            var ulcount = _arrayNumChapters.Count;
            if (ulcount < ulTitle)
                return 0;

            return _arrayNumChapters[ulTitle - 1];
        }

        public string GetSubpictureStreamName(int nStream)
        {
            var str = Resources.Resources.error;

            if (_arraySubpictureStream.Count == 0)
                return str;

            if (nStream > (_arraySubpictureStream.Count - 1))
                return str;

            return _arraySubpictureStream[nStream];
        }

        public bool GoTo(int ulTitle, int ulChapter)
        {
            if (ulTitle <= _ulNumTitles && ulChapter <= GetNumChapters(ulTitle))
            {
                IDvdCmd pObj;
                var hr = _pDvdControl2.PlayChapterInTitle(ulTitle, ulChapter,
                    DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (pObj != null)
                    {
                        pObj.WaitForEnd();
                        Marshal.ReleaseComObject(pObj);
                    }

                    if (_curDomain == DVD_DOMAIN.DVD_DOMAIN_Title && _ulCurTitle != ulTitle)
                    {
                        UpdateTitleInfo();
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsAudioStreamEnabled(int ulStreamNum)
        {
            bool bEnabled;
            var hr = _pDvdInfo2.IsAudioStreamEnabled(ulStreamNum, out bEnabled);
            return (hr == DsHlp.S_OK) && bEnabled;
        }

        public bool IsResumeDvdEnabled()
        {
            DVD_DOMAIN domain = _curDomain;
            return ((domain == DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu) || (domain == DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu))
                && (UOPS & VALID_UOP_FLAG.UOP_FLAG_Resume) == 0 && _bShowMenuCalledFromTitle;
        }

        public bool IsSubpictureEnabled()
        {
            int ulStreamsAvailable, ulCurrentStream;
            bool bIsDisabled; // TRUE means it is disabled

            var hr = _pDvdInfo2.GetCurrentSubpicture(out ulStreamsAvailable, out ulCurrentStream, out bIsDisabled);
            if (DsHlp.SUCCEEDED(hr))
                return !bIsDisabled;

            return false;
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            bool bEnabled;
            var hr = _pDvdInfo2.IsSubpictureStreamEnabled(ulStreamNum, out bEnabled);
            return (hr == DsHlp.S_OK) && bEnabled;
        }

        // The Resume method leaves a menu and resumes playback
        public bool ResumeDvd()
        {
            IDvdCmd pObj;
            var hr = _pDvdControl2.Resume(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
            if (DsHlp.SUCCEEDED(hr))
            {
                if (pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
                _bMenuOn = false;
                return true;
            }

            return false;
        }

        public void ReturnFromSubmenu()
        {
            IDvdCmd pObj;
            var hr = _pDvdControl2.ReturnFromSubmenu(DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
            if (DsHlp.SUCCEEDED(hr) && pObj != null)
            {
                pObj.WaitForEnd();
                Marshal.ReleaseComObject(pObj);
            }
        }

        public void SetMenuLang(int nLang)
        {
            if (_arrayMenuLangLcid.Count == 0)
                return;

            if (nLang > (_arrayMenuLangLcid.Count - 1))
                return;

            _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, true);

            // Changing menu language is only valid in the DVD_DOMAIN_Stop domain
            var hr = _pDvdControl2.Stop();
            if (DsHlp.SUCCEEDED(hr))
            {
                // Change the default menu language
                _pDvdControl2.SelectDefaultMenuLanguage(_arrayMenuLangLcid[nLang]);

                // Display the root menu
                ShowMenu(DVD_MENU_ID.DVD_MENU_Title);
            }

            // Turn off ResetOnStop option 
            _pDvdControl2.SetOption(DVD_OPTION_FLAG.DVD_ResetOnStop, false);
        }

        public void ShowMenu(DVD_MENU_ID menuId)
        {
            DVD_DOMAIN domain = _curDomain;
            IDvdCmd pObj;
            var hr = _pDvdControl2.ShowMenu(menuId, DVD_CMD_FLAGS.DVD_CMD_FLAG_Flush, out pObj);
            if (DsHlp.SUCCEEDED(hr))
            {
                if (pObj != null)
                {
                    pObj.WaitForEnd();
                    Marshal.ReleaseComObject(pObj);
                }
                _bMenuOn = true;
                if (!_bShowMenuCalledFromTitle)
                {
                    _bShowMenuCalledFromTitle = domain == DVD_DOMAIN.DVD_DOMAIN_Title;
                }
            }
        }

        public void ActivateSelectedDvdMenuButton()
        {
            if (IsMenuOn)
                _pDvdControl2.ActivateButton();
        }

        public void SelectDvdMenuButton(DVD_RELATIVE_BUTTON relativeButton)
        {
            if (IsMenuOn)
                _pDvdControl2.SelectRelativeButton(relativeButton);
        }

        public void ActivateDvdMenuButtonAtPosition(GDI.POINT point)
        {
            if (IsMenuOn)
                _pDvdControl2.ActivateAtPosition(point);
        }

        public void SelectDvdMenuButtonAtPosition(GDI.POINT point)
        {
            if (IsMenuOn)
                _pDvdControl2.SelectAtPosition(point);
        }

        protected override bool HandleGraphEvent(int evCode, int lParam1, int lParam2)
        {
            var handled = base.HandleGraphEvent(evCode, lParam1, lParam2);

            if (!handled)
            {
                switch (evCode)
                {
                    ////// DVD cases ///////
                    case (int)DsEvCode.DvdCurrentHmsfTime:
                        var guid = new Guid(lParam1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        var abyte = guid.ToByteArray();
                        _curTime.bHours = abyte[0];
                        _curTime.bMinutes = abyte[1];
                        _curTime.bSeconds = abyte[2];
                        _curTime.bFrames = abyte[3];

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdChaptStart:
                        _ulCurChapter = lParam1;

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdAngleChange:
                        // lParam1 is the number of available angles (1 means no multiangle support)
                        // lParam2 is the current angle, Angle numbers range from 1 to 9
                        _ulAnglesAvailable = lParam1;
                        _ulCurrentAngle = lParam2;

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdAnglesAvail:
                        // Read the number of available angles
                        GetAngleInfo();
                        OnModifyMenu();

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdNoFpPgc: // disc doesn't have a First Play Program Chain
                        IDvdCmd pObj;
                        var hr = _pDvdControl2.PlayTitle(1, DVD_CMD_FLAGS.DVD_CMD_FLAG_None, out pObj);
                        if (DsHlp.SUCCEEDED(hr) && pObj != null)
                        {
                            pObj.WaitForEnd();
                            Marshal.ReleaseComObject(pObj);
                        }

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdDomChange:
                        switch (lParam1)
                        {
                            case (int)DVD_DOMAIN.DVD_DOMAIN_FirstPlay:  // = 1 (Performing default initialization of a DVD disc)
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_Stop:       // = 5
                                ClearTitleInfo();
                                _bMenuOn = false;

                                bool bEjected;
                                HandleDiscEject(out bEjected);
                                if (bEjected)
                                {
                                    OnDiscEjected();
                                }
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_VideoManagerMenu:  // = 2
                            case (int)DVD_DOMAIN.DVD_DOMAIN_VideoTitleSetMenu: // = 3
                                // Inform the app to update the menu option to show "Resume" now
                                _bMenuOn = true;  // now menu is "On"
                                GetMenuLanguageInfo();
                                break;
                            case (int)DVD_DOMAIN.DVD_DOMAIN_Title:      // = 4
                                // Inform the app to update the menu option to show "Menu" again
                                _bMenuOn = false; // we are no longer in a menu
                                _bShowMenuCalledFromTitle = false;
                                UpdateTitleInfo();
                                OnInitSize(); // video size and aspect ratio might have changed when entering new title
                                break;
                        } // end of domain change switch
                        _curDomain = (DVD_DOMAIN)lParam1;
                        OnModifyMenu();

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdValidUopsChange:
                        _uops = (VALID_UOP_FLAG)lParam1;

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdPlaybStopped:
                        //	StopGraph();

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdParentalLChange:
                        if (DvdParentalChange != null)
                        {
                            var args = new UserDecisionEventArgs(String.Format(Resources.Resources.accept_parental_level_format, lParam1));
                            OnDvdParentalChange(args);
                            _pDvdControl2.AcceptParentalLevelChange(args.Accept);
                        }
                        else
                        {
                            _pDvdControl2.AcceptParentalLevelChange(false);
                        }

                        handled = true;
                        break;
                    case (int)DsEvCode.DvdError:
                        handled = true;
                        switch (lParam1)
                        {
                            case (int)DVD_ERROR.DVD_ERROR_Unexpected: // Playback is stopped.
                                OnGraphError(Resources.Resources.mw_dvd_unexpected_error);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_CopyProtectFail:
                                OnGraphError(Resources.Resources.mw_dvd_copyprotect_failed);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_InvalidDVD1_0Disc:
                                OnGraphError(Resources.Resources.mw_dvd_invalid_disc);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_InvalidDiscRegion:
                                OnGraphError(Resources.Resources.mw_dvd_invalid_region);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                //	ChangeDvdRegion(); // details in the dvdcore.cpp
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_LowParentalLevel:
                                OnGraphError(Resources.Resources.mw_dvd_low_parental_level);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_MacrovisionFail:
                                OnGraphError(Resources.Resources.mw_dvd_macrovision_error);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_IncompatibleSystemAndDecoderRegions:
                                OnGraphError(Resources.Resources.mw_dvd_system_decoder_regions);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            case (int)DVD_ERROR.DVD_ERROR_IncompatibleDiscAndDecoderRegions:
                                OnGraphError(Resources.Resources.mw_dvd_disc_decoder_regions);
                                MediaControl.Stop();
                                GraphState = GraphState.Stopped;
                                break;
                            default:
                                handled = false;
                                break;
                        }  // end of switch (lParam1)
                        break;
                    // Next is warning
                    case (int)DsEvCode.DvdWarning:
                        switch (lParam1)
                        {
                            case (int)DVD_WARNING.DVD_WARNING_InvalidDVD1_0Disc:
                                //		OnGraphError("DVD Warning: Current disc is not v1.0 spec compliant");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_FormatNotSupported:
                                //		OnGraphError("DVD Warning: The decoder does not support the new format.");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_IllegalNavCommand:
                                //		OnGraphError("DVD Warning: An illegal navigation command was encountered.");
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Open:
                                OnGraphError(Resources.Resources.mw_dvd_warning_cant_open_file);
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Seek:
                                OnGraphError(Resources.Resources.mw_dvd_warning_cant_seek);
                                break;
                            case (int)DVD_WARNING.DVD_WARNING_Read:
                                OnGraphError(Resources.Resources.mw_dvd_warning_cant_read);
                                break;
                        }
                        handled = true;
                        break;
                    case (int)DsEvCode.DvdButtonChange:
                        break;
                    case (int)DsEvCode.DvdStillOn:
                        if (lParam1 != 0) // if there is a still without buttons, we can call StillOff
                        {
                            _bStillOn = true;
                        }
                        handled = true;
                        break;
                    case (int)DsEvCode.DvdStillOff:
                        _bStillOn = false; // we are no longer in a still
                        handled = true;
                        break;
                }
            }

            return handled;
        }

        private void HandleDiscEject(out bool bEjected)
        {
            bEjected = false;

            var ptr = Marshal.AllocCoTaskMem(Storage.MAX_PATH * 2);
            int ulActualSize;
            var hr = _pDvdInfo2.GetDVDDirectory(ptr, Storage.MAX_PATH, out ulActualSize);
            if (hr == DsHlp.S_OK)
            {
                string path = Marshal.PtrToStringUni(ptr, ulActualSize);
                if (path.Length >= 3)
                {
                    path = path.Substring(0, 3);
                    uint maximumComponentLength, fileSystemFlags, volumeSerialNumber;
                    var nMode = NoCat.SetErrorMode(NoCat.SEM_FAILCRITICALERRORS);
                    if (Storage.GetVolumeInformation(path, null, 0, out volumeSerialNumber,
                                                     out maximumComponentLength, out fileSystemFlags, null, 0) == 0)
                    {
                        bEjected = true;
                    }
                    NoCat.SetErrorMode(nMode);
                }
            }
            Marshal.FreeCoTaskMem(ptr);
        }
    }
}