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
using Dzimchuk.DirectShow;
using Dzimchuk.MediaEngine.Core.Render;
using Dzimchuk.MediaEngine.Core.Description;

namespace Dzimchuk.MediaEngine.Core.GraphBuilders
{
    /// <summary>
    /// 
    /// </summary>
    internal class RegularFilterGraphBuilder : FilterGraphBuilder
    {
        private bool bUsePreferredFilters;
        private static RegularFilterGraphBuilder graphBuilder;
            
        private RegularFilterGraphBuilder()
        {
        }

        public static RegularFilterGraphBuilder GetGraphBuilder()
        {
            if (graphBuilder == null)
                graphBuilder = new RegularFilterGraphBuilder();
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
            string source = parameters.source;
            IntPtr hMediaWindow = parameters.hMediaWindow;
            Renderer PreferredVideoRenderer = parameters.PreferredVideoRenderer;
            
            // Create the filter graph manager
            try
            {
                Type type = Type.GetTypeFromCLSID(Clsid.FilterGraph, true);
                comobj = Activator.CreateInstance(type);
                pFilterGraph.pGraphBuilder = (IGraphBuilder)comobj;
                comobj = null; // important! (see the finally block)
            }
            catch (Exception e)
            {
                throw new FilterGraphBuilderException(Error.FilterGraphManager, e);
            }

            // Adding a source filter for a specific video file
            AddSourceFilter(pFilterGraph, source);
                        
            // QUERY the filter graph interfaces
            try
            {
                pFilterGraph.pFilterGraph2 = (IFilterGraph2)pFilterGraph.pGraphBuilder;
                pFilterGraph.pMediaControl = (IMediaControl)pFilterGraph.pGraphBuilder;
                pFilterGraph.pMediaEventEx = (IMediaEventEx)pFilterGraph.pGraphBuilder;
                pFilterGraph.pMediaSeeking = (IMediaSeeking)pFilterGraph.pGraphBuilder;
                pFilterGraph.pBasicAudio = (IBasicAudio)pFilterGraph.pGraphBuilder;
            }
            catch(Exception e)
            {
                throw new FilterGraphBuilderException(Error.NecessaryInterfaces, e);
            }

            // SET the graph state window callback
            pFilterGraph.pMediaEventEx.SetNotifyWindow(hMediaWindow, (int)FilterGraph.UWM_GRAPH_NOTIFY, IntPtr.Zero);

            // create a renderer
            ThrowExceptionForHRPointer errorFunc = delegate(int hrCode, Error error)
            {
                ThrowExceptionForHR(pFilterGraph, hrCode, error);
            };
            
            pFilterGraph.pRenderer = RendererBase.AddRenderer(pFilterGraph.pGraphBuilder, PreferredVideoRenderer, errorFunc, hMediaWindow);
                        
            /*if (bUsePreferredFilters)
                ManualBuildGraph(pFilterGraph);
            else
                AutoBuildGraph(pFilterGraph, hMediaWindow);*/
            DoBuildGraph(pFilterGraph, hMediaWindow);
            
            SeekingCapabilities caps = SeekingCapabilities.CanGetDuration;
            int hr = pFilterGraph.pMediaSeeking.CheckCapabilities(ref caps);
            if (hr == DsHlp.S_OK)
            {
                pFilterGraph.bSeekable = true;
                pFilterGraph.pMediaSeeking.GetDuration(out pFilterGraph.rtDuration);
            }

            // MEDIA SIZE
            int Height=0;
            int Width=0;
            double w;
            double h;

            pFilterGraph.pRenderer.GetNativeVideoSize(out Width, out Height, out pFilterGraph.ARWidth, out pFilterGraph.ARHeight);
            
            if (Height == 0 || Width == 0)
            {
                // this is where we failed to render video and instead of saying we can't get the dimensions we should tell a user
                // what kind of pin we failed to connect
                //
                // we can call FindSplitter here because:
                // _ if there are audio streams _
                // a) if audio streams have been rendered _and_ there is a video stream FindSplitter will return a filter that the source filter is connected to
                // b) if audio streams have been rendered and there is no video stream FindSplitter is likely to return the source filter as a splitter
                // in cases a) and b) the splitter is probably added by pGraphBuilder.Render(...)
                // 
                // c) if no audio stream was rendered (due to some error) but there is an unconnected audio pin FindSplitter will return the filter
                //
                // _ if there are no audio streams _
                // d) if there is no audio stream and the video stream was not rendered FindSplitter will return the source filter as a splitter
                //
                // Splitter is very likely to be already found by the way

                w = h = 1;
                // throw new FilterGraphBuilderException(Error.NoVideoDimension);
                // unrendered pins are reported below (to include audio ones)
            }

            w=pFilterGraph.ARWidth;
            h=pFilterGraph.ARHeight;
            pFilterGraph.dAspectRatio = w/h;

            pFilterGraph.rcSrc.left = pFilterGraph.rcSrc.top = 0;
            pFilterGraph.rcSrc.right = Width;
            pFilterGraph.rcSrc.bottom = Height;

            DsUtils.EnumFilters(pFilterGraph.pGraphBuilder, pFilterGraph.aFilters);
#if DEBUG
            pFilterGraph.bAddedToRot = DsUtils.AddToRot(pFilterGraph.pGraphBuilder, out pFilterGraph.dwRegister);
#endif

            if (FindSplitter(pFilterGraph))
                ReportUnrenderedPins(pFilterGraph);
            GatherMediaInfo(pFilterGraph, source);
        }

        private void AddSourceFilter(FilterGraph pFilterGraph, string source)
        {
            int hr;
            if (pFilterGraph.RecommnedSourceFilterId != Guid.Empty)
            {
                pFilterGraph.error = Error.SourceFilter;
                IBaseFilter pBaseFilter;
                GetFilter(pFilterGraph.RecommnedSourceFilterId, out pBaseFilter);
                if (pBaseFilter != null)
                {
                    hr = pFilterGraph.pGraphBuilder.AddFilter(pBaseFilter, source);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        IFileSourceFilter pFileSourceFilter = null;
                        try
                        {
                            pFileSourceFilter = (IFileSourceFilter)pBaseFilter;
                        }
                        catch
                        {
                        }

                        if (pFileSourceFilter != null)
                        {
                            hr = pFileSourceFilter.Load(source, IntPtr.Zero);
                            if (DsHlp.SUCCEEDED(hr))
                            {
                                pFilterGraph.pSource = pBaseFilter; // success
                                return;
                            }
                            else
                            {
                                Marshal.FinalReleaseComObject(pBaseFilter);
                                // TODO trace something here
                            }
                        }
                        else
                        {
                            Marshal.FinalReleaseComObject(pBaseFilter);
                            // TODO trace something here
                        }
                    }
                    else
                    {
                        Marshal.FinalReleaseComObject(pBaseFilter);
                        // TODO trace something here
                    }
                }
            }
            
            // last resort
            hr = pFilterGraph.pGraphBuilder.AddSourceFilter(source, null, out pFilterGraph.pSource);
            ThrowExceptionForHR(pFilterGraph, hr, Error.SourceFilter);
        }

        protected virtual void DoBuildGraph(FilterGraph pFilterGraph, IntPtr hMediaWindow)
        {
            int hr;
            GetSourceOutPin(pFilterGraph);

            IPin pVideoRendererInputPin = pFilterGraph.pRenderer.GetInputPin();
            if (pVideoRendererInputPin != null)
            {
                hr = pFilterGraph.pGraphBuilder.Connect(pFilterGraph.pSourceOutPin, pVideoRendererInputPin);
                Marshal.ReleaseComObject(pVideoRendererInputPin);
                                
                // that's it, if hr > 0 (partial success) the video stream is already rendered but there are unrendered (audio) streams
                
                if (DsHlp.FAILED(hr))
                {
                    // if Connect failed (hr < 0) we either can't render video (no decoder) or there is no video (audio file) 
                    
                    // we will try to find the splitter and render audio streams
                    // first let's try to add a splitter
                    hr = pFilterGraph.pGraphBuilder.Render(pFilterGraph.pSourceOutPin);
                    // RenderAudioStreams, FindSplitter and StripSplitter will take care about the rest
                }
            }
            else
            {
                // we shouldn't ever enter this path
                hr = FallbackRender(pFilterGraph, pFilterGraph.pSourceOutPin, hMediaWindow);
                ThrowExceptionForHR(pFilterGraph, hr, Error.CantRenderFile);
            }

            RenderAudioStreams(pFilterGraph); // we should render audio streams ourselves because we want full control 
                                              // AND we need to make sure FindSplitter and StripSplitter were called so that we won't screw up the graph later
        }

        private int FallbackRender(FilterGraph pFilterGraph, IPin outPin, IntPtr hMediaWindow)
        {
            pFilterGraph.pRenderer.RemoveFromGraph();
            int hr = pFilterGraph.pGraphBuilder.Render(outPin);
            pFilterGraph.pRenderer = RendererBase.GetExistingRenderer(pFilterGraph.pGraphBuilder, hMediaWindow);
            return hr;
        }
        
        // sets either stream or video out pin
        private void GetSourceOutPin(FilterGraph pFilterGraph)
        {         
            IPin pPin = null;
            int nSkip = 0;

            while ((pPin = DsUtils.GetPin(pFilterGraph.pSource, PinDirection.Output, false, nSkip)) != null)
            {
                if ((DsUtils.IsMediaTypeSupported(pPin, MediaType.Stream) == 0) ||
                    (DsUtils.IsMediaTypeSupported(pPin, MediaType.Video) == 0))
                {
                    pFilterGraph.pSourceOutPin = pPin;
                    break;
                }
                else
                    nSkip++;
                Marshal.ReleaseComObject(pPin);
            }

            if (pFilterGraph.pSourceOutPin == null)
                throw new FilterGraphBuilderException(Error.CantPlayFile);
        }

        // returns a splitter's out video pin (disconnects it if necessary)
        // returns null if not found
        private IPin GetSplitterVideoOutPin(FilterGraph pGraph)
        {
            if (FindSplitter(pGraph))
            {
                IPin pSplitterVideoOutPin = null;
                int nSkip = 0;

                // try unconnected pins first
                while ((pSplitterVideoOutPin =
                    DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, false, nSkip)) != null)
                {
                    if (DsUtils.IsMediaTypeSupported(pSplitterVideoOutPin, MediaType.Video) == 0)
                    {
                        break;
                    }
                    else
                        nSkip++;
                    Marshal.ReleaseComObject(pSplitterVideoOutPin);
                } // end of while

                if (pSplitterVideoOutPin == null) 
                {
                    // let's try connected pins
                    nSkip = 0;
                    while ((pSplitterVideoOutPin =
                        DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, true, nSkip)) != null)
                    {
                        if (DsUtils.IsMediaTypeSupported(pSplitterVideoOutPin, MediaType.Video) == 0)
                        {
                            DsUtils.Disconnect(pGraph.pGraphBuilder, pSplitterVideoOutPin);
                            break;
                        }
                        else
                            nSkip++;
                        Marshal.ReleaseComObject(pSplitterVideoOutPin);
                    } // end of while
                }
                
                return pSplitterVideoOutPin;
            }
            else
                throw new FilterGraphBuilderException(Error.CantRenderFile);
        }
        
        #region Manual graph
        private void ManualBuildGraph(FilterGraph pGraph)
        {
            
        }

        private void RenderFileStream(FilterGraph pGraph, IPin pPin, ref AMMediaType mediaType)
        {

        }

        private void RenderVideoStream(FilterGraph pGraph, IPin pPin, ref AMMediaType mediaType)
        {

        }

        private void RenderAudioStream(FilterGraph pGraph, IPin pPin, ref AMMediaType mediaType)
        {
            if (BuildSoundRenderer(pGraph))
            {
                IBaseFilter pBaseFilter;
                int hr;
                GetFilter(mediaType.majorType, mediaType.subType, out pBaseFilter);
                if (pBaseFilter != null)
                {
                    hr = AddFilterToGraph(pGraph, pBaseFilter);
                    if (DsHlp.SUCCEEDED(hr))
                    {
                        
                    }
                    else
                    {
                        Marshal.ReleaseComObject(pBaseFilter);
                        if (!ConnectPinToSoundRenderer(pGraph, pPin))
                            RemoveSoundRenderer(pGraph);
                    }
                }
                else
                {

                }
            }
        }

        private int AddFilterToGraph(FilterGraph pGraph, IBaseFilter pFilter)
        {
            string name = null;
            FilterInfo fInfo = new FilterInfo();
            int hr = pFilter.QueryFilterInfo(out fInfo);
            if (DsHlp.SUCCEEDED(hr))
            {
                name = fInfo.achName;
                if (fInfo.pGraph != null)
                    Marshal.ReleaseComObject(fInfo.pGraph);
            }		
            return pGraph.pGraphBuilder.AddFilter(pFilter, name);
        }

        private bool ConnectPinToSoundRenderer(FilterGraph pGraph, IPin pPin)
        {
            IPin pInputPin=DsUtils.GetPin((IBaseFilter) pGraph.arrayDSBaseFilter[pGraph.arrayDSBaseFilter.Count-1],
                PinDirection.Input);
            int hr=pGraph.pGraphBuilder.Connect(pPin, pInputPin);
            Marshal.ReleaseComObject(pInputPin);
            return hr==DsHlp.S_OK || hr==DsHlp.VFW_S_PARTIAL_RENDER;
        }

        private void RemoveSoundRenderer(FilterGraph pGraph)
        {
            IBaseFilter pBaseFilter = (IBaseFilter) pGraph.arrayDSBaseFilter[pGraph.arrayDSBaseFilter.Count-1];
            pGraph.pGraphBuilder.RemoveFilter(pBaseFilter);
            Marshal.ReleaseComObject(pBaseFilter);
                                                                
            pGraph.arrayBasicAudio.RemoveAt(pGraph.arrayBasicAudio.Count-1);
            pGraph.arrayDSBaseFilter.RemoveAt(pGraph.arrayDSBaseFilter.Count-1);
        }
        #endregion

        #region Automatic Graph helpers
        private void RenderAudioStreams(FilterGraph pGraph)
        {
            IPin pPin = null;
            IPin pInputPin = null;
    
            IBasicAudio pBA;
            IBaseFilter pBaseFilter;

            int hr;
            int nSkip = 0;
            if (FindSplitter(pGraph))
            {
                while((pPin=DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, false, nSkip)) != null)
                {
                    if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                    {
                        // this unconnected pin supports audio type!
                        // let's render it!
                        if (BuildSoundRenderer(pGraph))
                        {
                            pInputPin=DsUtils.GetPin((IBaseFilter) pGraph.arrayDSBaseFilter[pGraph.arrayDSBaseFilter.Count-1],
                                PinDirection.Input);
                            hr=DsHlp.S_FALSE;
                            hr=pGraph.pGraphBuilder.Connect(pPin, pInputPin);
                            Marshal.ReleaseComObject(pInputPin);
                            if (hr==DsHlp.S_OK || hr==DsHlp.VFW_S_PARTIAL_RENDER)
                            {
                                if (pGraph.arrayDSBaseFilter.Count==8) 
                                {
                                    Marshal.ReleaseComObject(pPin);
                                    break; // out of while cicle
                                }

                            }
                            else
                            {
                                pBaseFilter = (IBaseFilter) pGraph.arrayDSBaseFilter[pGraph.arrayDSBaseFilter.Count-1];
                                pGraph.pGraphBuilder.RemoveFilter(pBaseFilter);
                                Marshal.ReleaseComObject(pBaseFilter);
                                                                
                                pGraph.arrayBasicAudio.RemoveAt(pGraph.arrayBasicAudio.Count-1);
                                pGraph.arrayDSBaseFilter.RemoveAt(pGraph.arrayDSBaseFilter.Count-1);

                                nSkip++;
                            }
                        }
                        else
                        {
                            // could not create/add DirectSound filter
                            Marshal.ReleaseComObject(pPin);
                            break; // out of while cicle
                        }
                    }
                    else
                        nSkip++;
                    Marshal.ReleaseComObject(pPin);
                } // end of while
            }
    
            pGraph.nCurrentAudioStream=0;
            pGraph.nAudioStreams = pGraph.arrayBasicAudio.Count;
            int lVolume = -10000;
            for (int i=1; i<pGraph.nAudioStreams; i++)
            {
                pBA = (IBasicAudio) pGraph.arrayBasicAudio[i];
                pBA.put_Volume(lVolume);
            }
        }

        // this function should be called AFTER the video stream has been rendered
        // but before rendering the audio streams
        // however, it will try to find the splitter even if video wasn't rendered
        private bool FindSplitter(FilterGraph pGraph)
        {
            if (pGraph.pSplitterFilter != null)
            {
                RemoveRedundantFilters(pGraph);
                return true;
            }
            
            IEnumFilters pEnumFilters = null;
            IBaseFilter pFilter = null;
            int cFetched;
            bool bSplitterFound=false;

            int hr = pGraph.pGraphBuilder.EnumFilters(out pEnumFilters);
            if (DsHlp.FAILED(hr)) return false;
    
            IPin pPin;
            int nFilters=0;
            bool bCanRelease;
            while((pEnumFilters.Next(1, out pFilter, out cFetched) == DsHlp.S_OK))
            {
                nFilters++;
                bCanRelease=true;
                pPin=DsUtils.GetPin(pFilter, PinDirection.Output, false, 0);
                if (pPin != null)
                {
                    if (!bSplitterFound)
                    {
                        if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                        {
                            //this unconnected pin supports audio type!
                            bSplitterFound=true;
                            bCanRelease=false;
                            pGraph.pSplitterFilter=pFilter;
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
                    pPin=DsUtils.GetPin(pGraph.pSource, PinDirection.Output, true, 0);
                    if (pPin != null)
                    {
                        IPin pInputPin;
                        hr=pPin.ConnectedTo(out pInputPin);
                        if (hr==DsHlp.S_OK)
                        {
                            PinInfo info = new PinInfo();
                            pInputPin.QueryPinInfo(out info);
                            if (hr==DsHlp.S_OK)
                            {
                                pGraph.pSplitterFilter=info.pFilter;
                                bSplitterFound=true;
                            }
                            Marshal.ReleaseComObject(pInputPin);
                        }
                        Marshal.ReleaseComObject(pPin);
                    }
                }
                else
                {
                    pGraph.pSplitterFilter=pGraph.pSource;
                    bSplitterFound=true;
                }
            }

            StripSplitter(pGraph);
            return bSplitterFound;
        }

        // disconnect all connected audio out pins and remove unused filters
        private void StripSplitter(FilterGraph pGraph)
        {
            if (pGraph.pSplitterFilter != null)
            {
                IPin pPin = null;
                int nSkip = 0;
                                
                while ((pPin = DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, true, nSkip)) != null)
                {
                    if (DsUtils.IsMediaTypeSupported(pPin, MediaType.Audio) == 0)
                    {
                        // this connected pin supports audio type!                    
                        DsUtils.Disconnect(pGraph.pGraphBuilder, pPin);
                    }
                    else
                        nSkip++;
                    Marshal.ReleaseComObject(pPin);
                } // end of while
                
                RemoveRedundantFilters(pGraph);
            }
        }

        #endregion

        #region Gathering the info about the media file
        private void GatherMediaInfo(FilterGraph pGraph, string source)
        {
            pGraph.info.source = source;
            if (pGraph.pSource == null)
                return;
            int hr;
            IntPtr ptr;
            IEnumMediaTypes pEnumTypes;
            int cFetched;
            IPin pPin;
            
            if (pGraph.SourceType == SourceType.Asf)
                pGraph.info.StreamSubType = MediaSubType.Asf;
            else
            {
                pPin=DsUtils.GetPin(pGraph.pSource, PinDirection.Output, true);
                if (pPin != null)
                {
                    hr=pPin.EnumMediaTypes(out pEnumTypes);
                    if (hr==DsHlp.S_OK)
                    {
                        if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                        {
                            AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
                            pGraph.info.StreamSubType=mt.subType;
                            DsUtils.FreeFormatBlock(ptr);
                            Marshal.FreeCoTaskMem(ptr);
                        }
                        Marshal.ReleaseComObject(pEnumTypes);
                    }
                    Marshal.ReleaseComObject(pPin);
                }
            }

            if (pGraph.pSplitterFilter==null)
                return;
    
            StreamInfo pStreamInfo;
            int nPinsToSkip=0;
            while ((pPin=DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, true, nPinsToSkip)) != null)
            {
                nPinsToSkip++;
                pStreamInfo = new StreamInfo();
                hr=pPin.EnumMediaTypes(out pEnumTypes);
                if (hr==DsHlp.S_OK)
                {
                    if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                    {
                        AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
                        GatherStreamInfo(pGraph, pStreamInfo, ref mt);

                        DsUtils.FreeFormatBlock(ptr);
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    Marshal.ReleaseComObject(pEnumTypes);
                }
                Marshal.ReleaseComObject(pPin);
                pGraph.info.streams.Add(pStreamInfo);
            }
        }

        private int GetVideoDimension(int value1, int value2)
        {
            int value = value1 != 0 ? value1 : value2;
            if (value < 0)
                value *= -1;
            return value;
        }

        private void GatherStreamInfo(FilterGraph pGraph, StreamInfo pStreamInfo, ref AMMediaType pmt)
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

                    pStreamInfo.rcSrc.right = GetVideoDimension(pGraph.rcSrc.right, pVih.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(pGraph.rcSrc.bottom, pVih.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = pGraph.rcSrc.right;
                    pStreamInfo.rcSrc.bottom = pGraph.rcSrc.bottom;
                }
        
                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
                return;
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

                    pStreamInfo.rcSrc.right = GetVideoDimension(pGraph.rcSrc.right, pVih2.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(pGraph.rcSrc.bottom, pVih2.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = pGraph.rcSrc.right;
                    pStreamInfo.rcSrc.bottom = pGraph.rcSrc.bottom;
                }
        
                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
                return;
            }
            else if (pmt.formatType == FormatType.WaveEx)
            {
                // Check the buffer size.
                if (pmt.formatSize >= /*Marshal.SizeOf(typeof(WAVEFORMATEX))*/ 18)
                {
                    WAVEFORMATEX pWfx = (WAVEFORMATEX)Marshal.PtrToStructure(pmt.formatPtr, typeof(WAVEFORMATEX));
                    pStreamInfo.wFormatTag=pWfx.wFormatTag;
                    pStreamInfo.nSamplesPerSec=pWfx.nSamplesPerSec;
                    pStreamInfo.nChannels=pWfx.nChannels;
                    pStreamInfo.wBitsPerSample=pWfx.wBitsPerSample;
                    pStreamInfo.nAvgBytesPerSec=pWfx.nAvgBytesPerSec;
                    pStreamInfo.Flags = StreamInfoFlags.SI_WAVEFORMAT | 
                        StreamInfoFlags.SI_SAMPLERATE | StreamInfoFlags.SI_WAVECHANNELS | 
                        StreamInfoFlags.SI_BITSPERSAMPLE | StreamInfoFlags.SI_AUDIOBITRATE;
                }
        
                return;
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

                    pStreamInfo.rcSrc.right = GetVideoDimension(pGraph.rcSrc.right, pM1vi.hdr.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(pGraph.rcSrc.bottom, pM1vi.hdr.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = pGraph.rcSrc.right;
                    pStreamInfo.rcSrc.bottom = pGraph.rcSrc.bottom;
                }
        
                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
                return;
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

                    pStreamInfo.rcSrc.right = GetVideoDimension(pGraph.rcSrc.right, pM2vi.hdr.bmiHeader.biWidth);
                    pStreamInfo.rcSrc.bottom = GetVideoDimension(pGraph.rcSrc.bottom, pM2vi.hdr.bmiHeader.biHeight);
                }
                else
                {
                    pStreamInfo.rcSrc.right = pGraph.rcSrc.right;
                    pStreamInfo.rcSrc.bottom = pGraph.rcSrc.bottom;
                }
        
                pStreamInfo.Flags |= (StreamInfoFlags.SI_RECT | StreamInfoFlags.SI_FOURCC);
                return;
            }
        }
        #endregion

        protected void ReportUnrenderedPins(FilterGraph pGraph)
        {
            int hr;
            IntPtr ptr;
            IEnumMediaTypes pEnumTypes;
            int cFetched;
            IPin pPin;

            StreamInfo pStreamInfo;
            int nPinsToSkip = 0;

            IList<StreamInfo> streams = new List<StreamInfo>();

            while ((pPin = DsUtils.GetPin(pGraph.pSplitterFilter, PinDirection.Output, false, nPinsToSkip)) != null)
            {
                nPinsToSkip++;
                pStreamInfo = new StreamInfo();
                hr = pPin.EnumMediaTypes(out pEnumTypes);
                if (hr == DsHlp.S_OK)
                {
                    if (pEnumTypes.Next(1, out ptr, out cFetched) == DsHlp.S_OK)
                    {
                        AMMediaType mt = (AMMediaType)Marshal.PtrToStructure(ptr, typeof(AMMediaType));
                        GatherStreamInfo(pGraph, pStreamInfo, ref mt);

                        DsUtils.FreeFormatBlock(ptr);
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    Marshal.ReleaseComObject(pEnumTypes);
                }
                Marshal.ReleaseComObject(pPin);
                streams.Add(pStreamInfo);
            }

            if (streams.Count > 0)
            {
                OnFailedStreamsAvailable(streams);
            }
        }
    }
}
