using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterGraphs;

namespace Pvp.Core.MediaEngine.SourceFilterHandlers
{
    internal class RegularSourceFilterHandler : ISourceFilterHandler
    {
        private readonly IBaseFilter _sourceFilter;
        private IBaseFilter _splitterFilter;
        private IPin _sourceOutPin;
        private bool _disposed;

        // DirectSound Interfaces
        private readonly List<IBaseFilter> _directSoundBaseFilters;
        private readonly List<IBasicAudio> _basicAudioInterfaces;

        // Audio streams stuff
        private int _audioStreamsCount;
        private int _currentAudioStream;

        public RegularSourceFilterHandler(IBaseFilter sourceFilter)
        {
            _sourceFilter = sourceFilter;

            _directSoundBaseFilters = new List<IBaseFilter>();
            _basicAudioInterfaces = new List<IBasicAudio>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _basicAudioInterfaces.Clear();
            IEnumerator ie = _directSoundBaseFilters.GetEnumerator();
            while (ie.MoveNext())
            {
                var pBaseFilter = (IBaseFilter)ie.Current;
                while (Marshal.ReleaseComObject(pBaseFilter) > 0) { }
            }
            _directSoundBaseFilters.Clear();

            if (_sourceOutPin != null)
            {
                Marshal.FinalReleaseComObject(_sourceOutPin);
            }

            if (_sourceFilter != null)
            {
                if (_sourceFilter == _splitterFilter)
                {
                    _splitterFilter = null;
                }
                Marshal.FinalReleaseComObject(_sourceFilter);
            }

            if (_splitterFilter != null)
            {
                Marshal.FinalReleaseComObject(_splitterFilter);
            }

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
            IPin pPin = null;
            IPin pInputPin = null;

            IBasicAudio pBA;
            IBaseFilter pBaseFilter;

            int hr;
            int nSkip = 0;
            if (FindSplitter(pGraphBuilder))
            {
                while ((pPin = DsUtils.GetPin(_splitterFilter, PinDirection.Output, false, nSkip)) != null)
                {
                    if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                    {
                        // this unconnected pin supports audio type!
                        // let's render it!
                        if (BuildSoundRenderer(pGraphBuilder))
                        {
                            pInputPin = DsUtils.GetPin(_directSoundBaseFilters.Last(), PinDirection.Input);
                            hr = DsHlp.S_FALSE;
                            hr = pGraphBuilder.Connect(pPin, pInputPin);
                            Marshal.ReleaseComObject(pInputPin);
                            if (hr == DsHlp.S_OK || hr == DsHlp.VFW_S_PARTIAL_RENDER)
                            {
                                if (_directSoundBaseFilters.Count == 8)
                                {
                                    Marshal.ReleaseComObject(pPin);
                                    break; // out of while cycle
                                }

                            }
                            else
                            {
                                pBaseFilter = _directSoundBaseFilters.Last();
                                pGraphBuilder.RemoveFilter(pBaseFilter);
                                Marshal.ReleaseComObject(pBaseFilter);

                                _basicAudioInterfaces.RemoveAt(_basicAudioInterfaces.Count - 1);
                                _directSoundBaseFilters.RemoveAt(_directSoundBaseFilters.Count - 1);

                                nSkip++;
                            }
                        }
                        else
                        {
                            // could not create/add DirectSound filter
                            Marshal.ReleaseComObject(pPin);
                            break; // out of while cycle
                        }
                    }
                    else
                        nSkip++;
                    Marshal.ReleaseComObject(pPin);
                } // end of while
            }

            _currentAudioStream = 0;
            _audioStreamsCount = _basicAudioInterfaces.Count;
            const int lVolume = -10000;
            for (var i = 1; i < _audioStreamsCount; i++)
            {
                _basicAudioInterfaces[i].put_Volume(lVolume);
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

        private bool BuildSoundRenderer(IGraphBuilder pGraphBuilder)
        {
            var pDSBaseFilter = DsUtils.GetFilter(Clsid.DSoundRender, false);
            if (pDSBaseFilter == null)
            {
                TraceSink.GetTraceSink().TraceWarning("Could not instantiate DirectSound Filter.");
                return false;
            }

            // add the DirectSound filter to the graph
            var hr = pGraphBuilder.AddFilter(pDSBaseFilter, "DirectSound Filter");
            if (DsHlp.FAILED(hr))
            {
                while (Marshal.ReleaseComObject(pDSBaseFilter) > 0) { }
                TraceSink.GetTraceSink().TraceWarning("Could not add DirectSound Filter to the filter graph.");
                return false;
            }

            IBasicAudio pBA = pDSBaseFilter as IBasicAudio;
            if (pBA == null)
            {
                while (Marshal.ReleaseComObject(pDSBaseFilter) > 0) { }
                TraceSink.GetTraceSink().TraceWarning("Could not get IBasicAudio interface.");
                return false;
            }

            _basicAudioInterfaces.Add(pBA);
            _directSoundBaseFilters.Add(pDSBaseFilter);
            return true;
        }

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

            var nPinsToSkip = 0;
            IPin pPin;
            while ((pPin = DsUtils.GetPin(_splitterFilter, PinDirection.Output, true, nPinsToSkip)) != null)
            {
                nPinsToSkip++;

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

        public int AudioStreamsCount
        {
            get { return _audioStreamsCount; }
        }

        public int CurrentAudioStream
        {
            get { return _currentAudioStream; }
            set
            {
                if (_basicAudioInterfaces.Count == 0)
                    return;

                int lVolume;
                GetVolume(out lVolume);

                const int lMute = -10000;
                for (int i = 0; i < _audioStreamsCount; i++)
                {
                    var pBA = _basicAudioInterfaces[i];
                    pBA.put_Volume(lMute);
                }

                _currentAudioStream = value;
                SetVolume(lVolume);
            }
        }

        public bool SetVolume(int volume)
        {
            if (_basicAudioInterfaces.Count == 0)
                return false;

            var pBA = _basicAudioInterfaces[_currentAudioStream];
            return pBA.put_Volume(volume) == DsHlp.S_OK;
        }

        public bool GetVolume(out int volume)
        {
            volume = 0;

            if (_basicAudioInterfaces.Count == 0)
                return false;

            var pBA = _basicAudioInterfaces[_currentAudioStream];
            return pBA.get_Volume(out volume) == DsHlp.S_OK;
        }
    }
}