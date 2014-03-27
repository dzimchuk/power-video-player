using System;
using System.Linq;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterGraphs;
using Pvp.Core.MediaEngine.StreamHandlers;

namespace Pvp.Core.MediaEngine.SourceFilterHandlers
{
    internal class RegularSourceFilterHandler : ISourceFilterHandler
    {
        private readonly IBaseFilter _sourceFilter;
        private IBaseFilter _splitterFilter;
        private IPin _sourceOutPin;
        private bool _disposed;

        private IAudioStreamHandler _audioStreamHandler;

        public RegularSourceFilterHandler(IBaseFilter sourceFilter)
        {
            _sourceFilter = sourceFilter;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_sourceOutPin != null)
            {
                Marshal.FinalReleaseComObject(_sourceOutPin);
            }

            if (_audioStreamHandler != null)
            {
                _audioStreamHandler.Dispose();
            }

            if (_splitterFilter != _sourceFilter)
            {
                Marshal.FinalReleaseComObject(_splitterFilter);
            } 

            Marshal.FinalReleaseComObject(_sourceFilter);

            _disposed = true;
        }

        public void RenderVideo(IGraphBuilder pGraphBuilder, IRenderer renderer)
        {
            InsureSourceOutPin();

            var pVideoRendererInputPin = renderer.GetInputPin();
            if (pVideoRendererInputPin != null)
            {
                var hr = pGraphBuilder.Connect(_sourceOutPin, pVideoRendererInputPin);
                Marshal.ReleaseComObject(pVideoRendererInputPin);

                // that's it, if hr > 0 (partial success) the video stream is already rendered but there are unrendered (audio) streams

                hr.ThrowExceptionForHR(GraphBuilderError.CantRenderFile);
            }
            else
            {
                throw new FilterGraphBuilderException(GraphBuilderError.CantRenderFile);
            }
        }

        public void RenderAudio(IGraphBuilder pGraphBuilder)
        {
            if (FindSplitter(pGraphBuilder))
            {
                _audioStreamHandler = AudioStreamHandlerFactory.GetHandler(_splitterFilter);
                _audioStreamHandler.RenderAudio(pGraphBuilder, _splitterFilter);
            }
        }

        //        protected void GetFilter(Guid majortype, Guid subtype, out IBaseFilter filter)
        //        {
        //            filter = null;
        //            Guid guidFilter = Guid.Empty;
        //            using (var manager = new MediaTypeManager())
        //            {
        //                guidFilter = manager.GetTypeClsid(majortype, subtype);
        //            }
        //            if (guidFilter != Guid.Empty)
        //                GetFilter(guidFilter, out filter);
        //        }

        private void InsureSourceOutPin()
        {
            _sourceOutPin = DsUtils.GetPin(_sourceFilter, PinDirection.Output, new[] { MediaType.Stream, MediaType.Video });
            if (_sourceOutPin == null)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.CantPlayFile);
            }
        }

        // this function should be called AFTER the video stream has been rendered
        // but before rendering the audio streams
        // however, it will try to find the splitter even if video wasn't rendered
        private bool FindSplitter(IGraphBuilder pGraphBuilder)
        {
            if (_splitterFilter != null)
            {
                DsUtils.RemoveRedundantFilters(_sourceFilter, pGraphBuilder);
                return true;
            }

            IEnumFilters pEnumFilters = null;
            IBaseFilter pFilter = null;
            int cFetched;
            bool bSplitterFound = false;

            int hr = pGraphBuilder.EnumFilters(out pEnumFilters);
            if (DsHlp.FAILED(hr)) return false;

            IPin pPin;
            int nFilters = 0;
            bool bCanRelease;
            while ((pEnumFilters.Next(1, out pFilter, out cFetched) == DsHlp.S_OK))
            {
                nFilters++;
                bCanRelease = true;
                pPin = DsUtils.GetPin(pFilter, PinDirection.Output, false, 0);
                if (pPin != null)
                {
                    if (!bSplitterFound)
                    {
                        if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                        {
                            //this unconnected pin supports audio type!
                            bSplitterFound = true;
                            bCanRelease = false;
                            _splitterFilter = pFilter;
                        }
                    }
                    Marshal.ReleaseComObject(pPin);
                }

                //let's have a look at another filter
                if (bCanRelease)
                    Marshal.ReleaseComObject(pFilter);

                if (bSplitterFound)
                    break;
            }

            Marshal.ReleaseComObject(pEnumFilters);

            if (!bSplitterFound)
            {
                if (nFilters > 3)
                {
                    pPin = DsUtils.GetPin(_sourceFilter, PinDirection.Output, true, 0);
                    if (pPin != null)
                    {
                        IPin pInputPin;
                        hr = pPin.ConnectedTo(out pInputPin);
                        if (hr == DsHlp.S_OK)
                        {
                            PinInfo info = new PinInfo();
                            pInputPin.QueryPinInfo(out info);
                            if (hr == DsHlp.S_OK)
                            {
                                _splitterFilter = info.pFilter;
                                bSplitterFound = true;
                            }
                            Marshal.ReleaseComObject(pInputPin);
                        }
                        Marshal.ReleaseComObject(pPin);
                    }
                }
                else
                {
                    _splitterFilter = _sourceFilter;
                    bSplitterFound = true;
                }
            }

            StripSplitter(pGraphBuilder);
            return bSplitterFound;
        }

        // disconnect all connected audio out pins and remove unused filters
        private void StripSplitter(IGraphBuilder pGraphBuilder)
        {
            if (_splitterFilter != null)
            {
                IPin pPin = null;
                int nSkip = 0;

                while ((pPin = DsUtils.GetPin(_splitterFilter, PinDirection.Output, true, nSkip)) != null)
                {
                    if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                    {
                        // this connected pin supports audio type!                    
                        DsUtils.Disconnect(pGraphBuilder, pPin);
                    }
                    else
                        nSkip++;
                    Marshal.ReleaseComObject(pPin);
                } // end of while

                DsUtils.RemoveRedundantFilters(_sourceFilter, pGraphBuilder);
            }
        }

        public void GetMainStreamSubtype(Action<AMMediaType> inspect)
        {
            var pPin = DsUtils.GetPin(_sourceFilter, PinDirection.Output, true);
            if (pPin != null)
            {
                IEnumMediaTypes pEnumTypes;

                var hr = pPin.EnumMediaTypes(out pEnumTypes);
                if (hr == DsHlp.S_OK)
                {
                    IntPtr ptr;
                    int cFetched;

                    if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                    {
                        AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));

                        inspect(mt);
                        
                        DsUtils.FreeFormatBlock(ptr);
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    Marshal.ReleaseComObject(pEnumTypes);
                }
                Marshal.ReleaseComObject(pPin);
            }
        }

        public void GetStreamsMediaTypes(Action<AMMediaType> inspect)
        {
            if (_splitterFilter == null)
            {
                return;
            }

            _splitterFilter.EnumPins(PinDirection.Output, true, (pin, mediaType) =>
                                                                {
                                                                    if (mediaType.majorType == MediaType.Audio && _audioStreamHandler != null)
                                                                    {
                                                                        _audioStreamHandler.EnumMediaTypes(pin, mediaType, inspect);
                                                                    }
                                                                    else
                                                                    {
                                                                        inspect(mediaType);
                                                                    }
                                                                });
        }

        public int AudioStreamsCount
        {
            get { return _audioStreamHandler != null ? _audioStreamHandler.AudioStreamsCount : 0; }
        }

        public int CurrentAudioStream
        {
            get { return _audioStreamHandler != null ? _audioStreamHandler.CurrentAudioStream : 0; }
            set
            {
                if (_audioStreamHandler == null)
                    return;

                _audioStreamHandler.CurrentAudioStream = value;
            }
        }

        public bool SetVolume(int volume)
        {
            return _audioStreamHandler != null && _audioStreamHandler.SetVolume(volume);
        }

        public bool GetVolume(out int volume)
        {
            volume = 0;
            return _audioStreamHandler != null && _audioStreamHandler.GetVolume(out volume);
        }

        public string GetAudioStreamName(int nStream)
        {
            return _audioStreamHandler != null ? _audioStreamHandler.GetAudioStreamName(nStream) : string.Empty;
        }

        public void OnExternalStreamSelection()
        {
            if (_audioStreamHandler != null)
            {
                _audioStreamHandler.OnExternalStreamSelection();
            }
        }

        public int NumberOfSubpictureStreams { get; private set; }
        public int CurrentSubpictureStream { get; set; }
        public bool EnableSubpicture(bool bEnable)
        {
            throw new NotImplementedException();
        }

        public string GetSubpictureStreamName(int nStream)
        {
            throw new NotImplementedException();
        }

        public bool IsSubpictureEnabled()
        {
            throw new NotImplementedException();
        }

        public bool IsSubpictureStreamEnabled(int ulStreamNum)
        {
            throw new NotImplementedException();
        }
    }
}