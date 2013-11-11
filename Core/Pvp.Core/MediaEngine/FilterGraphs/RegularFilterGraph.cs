using System;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.Renderers;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine.FilterGraphs
{
    internal class RegularFilterGraph : FilterGraphBase
    {
        private readonly SourceType _sourceType;
        private readonly Guid _recommendedSourceFilterId;

        private ISourceFilterHandler _sourceFilterHandler;

        public RegularFilterGraph(SourceType sourceType, Guid recommendedSourceFilterId)
        {
            _recommendedSourceFilterId = recommendedSourceFilterId;
            _sourceType = sourceType;

            _aspectRatio = 1.0;
            _rate = 1.00;
        }

        public override void BuildUp(FilterGraphBuilderParameters parameters)
        {
            // Create filter graph manager
            InitializeGraphBuilder(() =>
                                       {
                                           object comobj = null;
                                           try
                                           {
                                               var type = Type.GetTypeFromCLSID(Clsid.FilterGraph, true);
                                               comobj = Activator.CreateInstance(type);
                                               var graphBuilder = (IGraphBuilder)comobj;
                                               comobj = null; // important! (see the finally block)

                                               return graphBuilder;
                                           }
                                           catch (Exception e)
                                           {
                                               throw new FilterGraphBuilderException(GraphBuilderError.FilterGraphManager, e);
                                           }
                                           finally
                                           {
                                               if (comobj != null)
                                               {
                                                   Marshal.FinalReleaseComObject(comobj);
                                               }
                                           }
                                       });
            

            // Adding a source filter for a specific video file
            _sourceFilterHandler = SourceFilterHandlerFactory.AddSourceFilter(GraphBuilder, parameters.Source, _recommendedSourceFilterId);

            // QUERY the filter graph interfaces
            InitializeMediaEventEx(parameters.MediaWindowHandle);
            InitializeFilterGraph2();
            InitializeMediaControl();
            InitializeMediaSeeking();
            InitializeBasicAudio();

            // create a renderer
            ThrowExceptionForHRPointer errorFunc = delegate(int hrCode, GraphBuilderError error)
                                                       {
                                                           hrCode.ThrowExceptionForHR(error);
                                                       };

            Renderer = RendererBase.AddRenderer(GraphBuilder, parameters.PreferredVideoRenderer, errorFunc, parameters.MediaWindowHandle);
            
            DoBuildGraph();

            SeekingCapabilities caps = SeekingCapabilities.CanGetDuration;
            int hr = MediaSeeking.CheckCapabilities(ref caps);
            if (hr == DsHlp.S_OK)
            {
                _isGraphSeekable = true;

                long rtDuration;
                MediaSeeking.GetDuration(out rtDuration);

                _duration = rtDuration;
            }

            // MEDIA SIZE
            int height, arWidth;
            int width, arHeight;

            Renderer.GetNativeVideoSize(out width, out height, out arWidth, out arHeight);

            double w = arWidth;
            double h = arHeight;
            _aspectRatio = w / h;

            _sourceRect = new GDI.RECT { left = 0, top = 0, right = width, bottom = height };
            
//            if (FindSplitter(pFilterGraph))
//                ReportUnrenderedPins(pFilterGraph); // then we can raise OnFailedStreamsAvailable
            GatherMediaInfo(parameters.Source);
        }

        public override SourceType SourceType
        {
            get { return _sourceType; }
        }

        private bool _isGraphSeekable;
        public override bool IsGraphSeekable
        {
            get { return _isGraphSeekable; }
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

        public override int AudioStreamsCount
        {
            get { return _sourceFilterHandler.AudioStreamsCount; }
        }

        public override int CurrentAudioStream
        {
            get { return _sourceFilterHandler.CurrentAudioStream; }
            set { _sourceFilterHandler.CurrentAudioStream = value; }
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

        public override bool SetVolume(int volume)
        {
            return _sourceFilterHandler.SetVolume(volume);
        }

        public override bool GetVolume(out int volume)
        {
            return _sourceFilterHandler.GetVolume(out volume);
        }

        public override string GetAudioStreamName(int nStream)
        {
            return String.Format(Resources.Resources.mw_stream_format, nStream + 1);
        }

        public override long GetCurrentPosition()
        {
            var result = 0L;

            if (IsGraphSeekable)
            {
                MediaSeeking.GetCurrentPosition(out result);
            }

            return result;
        }

        public override void SetCurrentPosition(long time)
        {
            if (IsGraphSeekable)
            {
                var state = GraphState;
                PauseGraph();
                long pStop = 0;
                MediaSeeking.SetPositions(ref time, SeekingFlags.AbsolutePositioning, ref pStop, SeekingFlags.NoPositioning);

                switch (state)
                {
                    case GraphState.Running:
                        ResumeGraph();
                        break;
                    case GraphState.Stopped:
                        MediaControl.Stop();
                        GraphState = GraphState.Stopped;
                        break;
                }
            }
        }

        public override void SetRate(double rate)
        {
            if (IsGraphSeekable)
            {
                var hr = MediaSeeking.SetRate(rate);
                if (hr == DsHlp.S_OK)
                {
                    _rate = rate;
                }
            }
        }

        protected override void CloseInterfaces()
        {
            base.CloseInterfaces();

            if (_sourceFilterHandler != null)
            {
                _sourceFilterHandler.Dispose();
            }
        }

        private void DoBuildGraph()
        {
            _sourceFilterHandler.RenderVideo(GraphBuilder, Renderer);
            _sourceFilterHandler.RenderAudio(GraphBuilder);
        }

//        public IEnumerable<StreamInfo> ReportUnrenderedPins()
//        {
//            int hr;
//            IntPtr ptr;
//            IEnumMediaTypes pEnumTypes;
//            int cFetched;
//            IPin pPin;
//
//            StreamInfo pStreamInfo;
//            int nPinsToSkip = 0;
//
//            IList<StreamInfo> streams = new List<StreamInfo>();
//
//            while ((pPin = DsUtils.GetPin(_splitterFilter, PinDirection.Output, false, nPinsToSkip)) != null)
//            {
//                nPinsToSkip++;
//                pStreamInfo = new StreamInfo();
//                hr = pPin.EnumMediaTypes(out pEnumTypes);
//                if (hr == DsHlp.S_OK)
//                {
//                    if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
//                    {
//                        AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
//                        GatherStreamInfo(pGraph, pStreamInfo, ref mt);
//
//                        DsUtils.FreeFormatBlock(ptr);
//                        Marshal.FreeCoTaskMem(ptr);
//                    }
//                    Marshal.ReleaseComObject(pEnumTypes);
//                }
//                Marshal.ReleaseComObject(pPin);
//                streams.Add(pStreamInfo);
//            }
//
//            return streams;
//        }

        #region Gathering the info about the media file
        private void GatherMediaInfo(string source)
        {
            AddToMediaInfo(source);

            if (SourceType == SourceType.Asf)
            {
                AddToMediaInfo(MediaSubType.Asf);
            }
            else
            {
                _sourceFilterHandler.GetMainStreamSubtype(mt => AddToMediaInfo(mt.subType));
            }

            _sourceFilterHandler.GetStreamsMediaTypes(mt =>
                                                          {
                                                              var streamInfo = new StreamInfo();
                                                              GatherStreamInfo(streamInfo, mt);
                                                              AddToMediaInfo(streamInfo);
                                                          });
        }

        private int GetVideoDimension(int value1, int value2)
        {
            int value = value1 != 0 ? value1 : value2;
            if (value < 0)
                value *= -1;
            return value;
        }

        private void GatherStreamInfo(StreamInfo pStreamInfo, AMMediaType pmt)
        {
            pStreamInfo.MajorType = pmt.majorType;
            pStreamInfo.SubType = pmt.subType;
            pStreamInfo.FormatType = pmt.formatType;

            if (pmt.formatType == FormatType.VideoInfo)
            {
                // Check the buffer size.
                if (pmt.formatSize >= Marshal.SizeOf(typeof(VIDEOINFOHEADER)))
                {
                    VIDEOINFOHEADER pVih = (VIDEOINFOHEADER)Marshal.PtrToStructure(pmt.formatPtr, typeof(VIDEOINFOHEADER));
                    pStreamInfo.dwBitRate = pVih.dwBitRate;
                    pStreamInfo.AvgTimePerFrame = pVih.AvgTimePerFrame;
                    pStreamInfo.Flags = StreamInfoFlags.SI_VIDEOBITRATE | StreamInfoFlags.SI_FRAMERATE;

                    pStreamInfo.rcSrc.right = GetVideoDimension(SourceRect.right, pVih.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(SourceRect.bottom, pVih.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = SourceRect.right;
                    pStreamInfo.rcSrc.bottom = SourceRect.bottom;
                }

                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
            }
            else if (pmt.formatType == FormatType.VideoInfo2)
            {
                // Check the buffer size.
                if (pmt.formatSize >= Marshal.SizeOf(typeof(VIDEOINFOHEADER2)))
                {
                    VIDEOINFOHEADER2 pVih2 = (VIDEOINFOHEADER2)Marshal.PtrToStructure(pmt.formatPtr, typeof(VIDEOINFOHEADER2));
                    pStreamInfo.dwBitRate = pVih2.dwBitRate;
                    pStreamInfo.AvgTimePerFrame = pVih2.AvgTimePerFrame;
                    pStreamInfo.dwPictAspectRatioX = pVih2.dwPictAspectRatioX;
                    pStreamInfo.dwPictAspectRatioY = pVih2.dwPictAspectRatioY;
                    pStreamInfo.dwInterlaceFlags = pVih2.dwInterlaceFlags;
                    pStreamInfo.Flags = StreamInfoFlags.SI_VIDEOBITRATE |
                        StreamInfoFlags.SI_FRAMERATE | StreamInfoFlags.SI_ASPECTRATIO |
                        StreamInfoFlags.SI_INTERLACEMODE;

                    pStreamInfo.rcSrc.right = GetVideoDimension(SourceRect.right, pVih2.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(SourceRect.bottom, pVih2.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = SourceRect.right;
                    pStreamInfo.rcSrc.bottom = SourceRect.bottom;
                }

                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
            }
            else if (pmt.formatType == FormatType.WaveEx)
            {
                // Check the buffer size.
                if (pmt.formatSize >= /*Marshal.SizeOf(typeof(WAVEFORMATEX))*/ 18)
                {
                    WAVEFORMATEX pWfx = (WAVEFORMATEX)Marshal.PtrToStructure(pmt.formatPtr, typeof(WAVEFORMATEX));
                    pStreamInfo.wFormatTag = pWfx.wFormatTag;
                    pStreamInfo.nSamplesPerSec = pWfx.nSamplesPerSec;
                    pStreamInfo.nChannels = pWfx.nChannels;
                    pStreamInfo.wBitsPerSample = pWfx.wBitsPerSample;
                    pStreamInfo.nAvgBytesPerSec = pWfx.nAvgBytesPerSec;
                    pStreamInfo.Flags = StreamInfoFlags.SI_WAVEFORMAT |
                        StreamInfoFlags.SI_SAMPLERATE | StreamInfoFlags.SI_WAVECHANNELS |
                        StreamInfoFlags.SI_BITSPERSAMPLE | StreamInfoFlags.SI_AUDIOBITRATE;
                }
            }
            else if (pmt.formatType == FormatType.MpegVideo)
            {
                // Check the buffer size.
                if (pmt.formatSize >= Marshal.SizeOf(typeof(MPEG1VIDEOINFO)))
                {
                    MPEG1VIDEOINFO pM1vi = (MPEG1VIDEOINFO)Marshal.PtrToStructure(pmt.formatPtr, typeof(MPEG1VIDEOINFO));
                    pStreamInfo.dwBitRate = pM1vi.hdr.dwBitRate;
                    pStreamInfo.AvgTimePerFrame = pM1vi.hdr.AvgTimePerFrame;
                    pStreamInfo.Flags = StreamInfoFlags.SI_VIDEOBITRATE | StreamInfoFlags.SI_FRAMERATE;

                    pStreamInfo.rcSrc.right = GetVideoDimension(SourceRect.right, pM1vi.hdr.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(SourceRect.bottom, pM1vi.hdr.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = SourceRect.right;
                    pStreamInfo.rcSrc.bottom = SourceRect.bottom;
                }

                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
            }
            else if (pmt.formatType == FormatType.Mpeg2Video)
            {
                // Check the buffer size.
                if (pmt.formatSize >= Marshal.SizeOf(typeof(MPEG2VIDEOINFO)))
                {
                    MPEG2VIDEOINFO pM2vi = (MPEG2VIDEOINFO)Marshal.PtrToStructure(pmt.formatPtr, typeof(MPEG2VIDEOINFO));
                    pStreamInfo.dwBitRate = pM2vi.hdr.dwBitRate;
                    pStreamInfo.AvgTimePerFrame = pM2vi.hdr.AvgTimePerFrame;
                    pStreamInfo.dwPictAspectRatioX = pM2vi.hdr.dwPictAspectRatioX;
                    pStreamInfo.dwPictAspectRatioY = pM2vi.hdr.dwPictAspectRatioY;
                    pStreamInfo.dwInterlaceFlags = pM2vi.hdr.dwInterlaceFlags;
                    pStreamInfo.Flags = StreamInfoFlags.SI_VIDEOBITRATE | StreamInfoFlags.SI_FRAMERATE |
                        StreamInfoFlags.SI_ASPECTRATIO | StreamInfoFlags.SI_INTERLACEMODE;

                    pStreamInfo.rcSrc.right = GetVideoDimension(SourceRect.right, pM2vi.hdr.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(SourceRect.bottom, pM2vi.hdr.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = SourceRect.right;
                    pStreamInfo.rcSrc.bottom = SourceRect.bottom;
                }

                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
            }
        }
        #endregion
    }
}