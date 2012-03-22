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
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;

namespace Pvp.Core.MediaEngine.GraphBuilders
{
    internal delegate TResult Func<T, TResult>(T args); // supported since .NET 3.5
    
    public delegate void FailedStreamsHandler(IList<StreamInfo> streams);
    
    internal class AbortException : ApplicationException
    {
    }

    internal class FilterGraphBuilderException : ApplicationException
    {
        public FilterGraphBuilderException(Error error) : base(FilterGraph.GetErrorText(error))
        {
        }

        public FilterGraphBuilderException(Error error, Exception innerException)
            : base(FilterGraph.GetErrorText(error), innerException)
        {
        }

        public FilterGraphBuilderException(string message)
            : base(message)
        {
        }

        public FilterGraphBuilderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    internal delegate void ThrowExceptionForHRPointer(int hr, Error error);
    
    /// <summary>
    /// 
    /// </summary>
    internal abstract class FilterGraphBuilder
    {
        public event FailedStreamsHandler FailedStreamsAvailable;
        
        protected struct FilterGraphBuilderParameters
        {
            public FilterGraph pFilterGraph;                // regular and dvd
            public string source;                           // regular
            public IntPtr hMediaWindow;                     // regular and dvd
            public Renderer PreferredVideoRenderer;         // regular
            
            public string DiscPath;                         // dvd
            public AM_DVD_GRAPH_FLAGS dwFlags;              // dvd
            public Func<string, bool> OnPartialSuccessCallback; // dvd
        }
        
        public static FilterGraph BuildFilterGraph(string source,
                                                   MediaSourceType CurrentlyPlaying,
                                                   IntPtr hMediaWindow,
                                                   Renderer PreferredVideoRenderer,
                                                   Action<string> onErrorCallback,
                                                   Func<string, bool> onPartialSuccessCallback)
        {
            object comobj = null;
            FilterGraph pFilterGraph = null;
            try
            {
                Trace.GetTrace().TraceInformation(
                    String.Format("Start building filter graph. Source: {0}. WhatToPlay: {1}. PreferredVideoRenderer: {2}.",
                    source, CurrentlyPlaying, PreferredVideoRenderer));

                pFilterGraph = new FilterGraph();
                FilterGraphBuilder pFilterGraphBuilder = GetFilterGraphBuilder(source,
                                                                               CurrentlyPlaying,
                                                                               pFilterGraph);
                if (pFilterGraphBuilder == null)
                {
                    Trace.GetTrace().TraceWarning("Could not identify source type.");
                    pFilterGraphBuilder = RegularFilterGraphBuilder.GetGraphBuilder();
                }

                FilterGraphBuilderParameters parameters = new FilterGraphBuilderParameters();
                parameters.pFilterGraph = pFilterGraph;
                parameters.source = source;
                parameters.hMediaWindow = hMediaWindow;
                parameters.PreferredVideoRenderer = PreferredVideoRenderer;
                parameters.DiscPath = source;
                parameters.dwFlags = AM_DVD_GRAPH_FLAGS.AM_DVD_HWDEC_PREFER;
                parameters.OnPartialSuccessCallback = onPartialSuccessCallback;

                pFilterGraphBuilder.BuildFilterGraph(ref comobj, ref parameters);
                Trace.GetTrace().TraceInformation("The graph was built successfully.");
                return pFilterGraph;
            }
            catch (AbortException)
            {
                if (pFilterGraph != null)
                    pFilterGraph.Dispose();
                Trace.GetTrace().TraceWarning("User abort.");
                return null;
            }
            catch (FilterGraphBuilderException builder_ex)
            {
                if (pFilterGraph != null)
                    pFilterGraph.Dispose();
                Trace.GetTrace().TraceError(builder_ex.ToString());
                onErrorCallback(builder_ex.Message);
                return null;
            }
            catch (COMException com_ex)
            {
                string error;
                if (pFilterGraph != null)
                {
                    pFilterGraph.Dispose();
                    error = (pFilterGraph.error != Error.Unknown) ?
                        FilterGraph.GetErrorText(pFilterGraph.error) : com_ex.Message;
                }
                else
                    error = com_ex.Message;
                Trace.GetTrace().TraceError(com_ex.ToString());
                Trace.GetTrace().TraceError(error);
                onErrorCallback(error);
                return null;
            }
            catch (Exception e)
            {
                if (pFilterGraph != null)
                    pFilterGraph.Dispose();
                Trace.GetTrace().TraceError(e.ToString());
                onErrorCallback(e.Message);
                return null;
            }
            finally
            {
                if (comobj != null)
                {
                    while (Marshal.ReleaseComObject(comobj) > 0) { }
                    comobj = null;
                }
            }
        }

        protected void ThrowExceptionForHR(FilterGraph pGraph, int hr, Error error)
        {
            pGraph.error = error;
            Marshal.ThrowExceptionForHR(hr);
            pGraph.error = Error.Unknown;
        }
        
        private static FilterGraphBuilder GetFilterGraphBuilder(string source,
                                                                MediaSourceType CurrentlyPlaying,
                                                                FilterGraph pFilterGraph)
        {
            FilterGraphBuilder pFilterGraphBuilder;
            if (CurrentlyPlaying == MediaSourceType.Dvd)
            {
                pFilterGraphBuilder = DVDFilterGraphBuilder.GetGraphBuilder();
            }
            else
            {
                SourceAnalizer.SetSourceType(source, pFilterGraph);
                switch (pFilterGraph.SourceType)
                {
                    case SourceType.Asf:
                    case SourceType.Mkv:
                    case SourceType.Flv:
                    case SourceType.Basic:
                        pFilterGraphBuilder = RegularFilterGraphBuilder.GetGraphBuilder();
                        break;
                    case SourceType.DVD:
                        pFilterGraphBuilder = DVDFilterGraphBuilder.GetGraphBuilder();
                        break;
                    default:
                        pFilterGraphBuilder = null;
                        break;
                }
            }

            return pFilterGraphBuilder;
        }

        protected abstract void BuildFilterGraph(ref object comobj,
                                                 ref FilterGraphBuilderParameters parameters);


        protected bool BuildSoundRenderer(FilterGraph pGraph)
        {
            IBaseFilter pDSBaseFilter;
            object comobj = null;
            try
            {
                pGraph.error = Error.DirectSoundFilter;
                Type type = Type.GetTypeFromCLSID(Clsid.DSoundRender, true);
                comobj = Activator.CreateInstance(type);
                pDSBaseFilter = (IBaseFilter)comobj;
            }
            catch
            {
                if (comobj != null)
                    while(Marshal.ReleaseComObject(comobj) > 0) {}
                Trace.GetTrace().TraceWarning("Could not instantiate DirectSound Filter.");
                return false;
            }
            comobj = null;

            // add the DirectSound filter to the graph
            pGraph.error = Error.AddDirectSoundFilter;
            int hr = pGraph.pGraphBuilder.AddFilter(pDSBaseFilter, "DirectSound Filter");
            if (DsHlp.FAILED(hr))
            {
                while(Marshal.ReleaseComObject(pDSBaseFilter) > 0) {}
                Trace.GetTrace().TraceWarning("Could not add DirectSound Filter to the filter graph.");
                return false;
            }

            IBasicAudio pBA = pDSBaseFilter as IBasicAudio;
            if (pBA == null)
            {
                while(Marshal.ReleaseComObject(pDSBaseFilter) > 0) {}
                Trace.GetTrace().TraceWarning("Could not get IBasicAudio interface.");
                return false;
            }

            pGraph.arrayBasicAudio.Add(pBA);
            pGraph.arrayDSBaseFilter.Add(pDSBaseFilter);
            return true;
        }
        
        protected void GetFilter(Guid majortype, Guid subtype, out IBaseFilter filter)
        {
            filter = null;
            Guid guidFilter = Guid.Empty;
            using (var manager = new MediaTypeManager())
            {
                guidFilter = manager.GetTypeClsid(majortype, subtype);
            }
            if (guidFilter != Guid.Empty)
                GetFilter(guidFilter, out filter);
        }

        protected void GetFilter(Guid clsId, out IBaseFilter filter)
        {
            filter = null;
            object comobj = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(clsId, true);
                comobj = Activator.CreateInstance(type);
                filter = (IBaseFilter) comobj;
                comobj = null; // important! (see the finally block)
            }
            catch
            {
            }
            finally
            {
                if (comobj != null)
                    while(Marshal.ReleaseComObject(comobj) > 0) {}
            }
        }

        protected void RemoveRedundantFilters(FilterGraph pGraph)
        {
            IEnumFilters pEnumFilters = null;
            IBaseFilter pFilter = null;
            int cFetched;
            int hr;

            bool bCallAgain = false;

            // get information about the source filter (its name)
            FilterInfo fSourceInfo = new FilterInfo();
            if (pGraph.pSource != null)
            {
                hr = pGraph.pSource.QueryFilterInfo(out fSourceInfo);
                if (DsHlp.SUCCEEDED(hr))
                {
                    if (fSourceInfo.pGraph != null)
                        Marshal.ReleaseComObject(fSourceInfo.pGraph);
                }
                else
                    fSourceInfo.achName = null;
            }

            // let's start enumerating filters
            hr = pGraph.pGraphBuilder.EnumFilters(out pEnumFilters);
            if (DsHlp.FAILED(hr)) return;

            while ((pEnumFilters.Next(1, out pFilter, out cFetched) == DsHlp.S_OK))
            {
                FilterInfo fInfo = new FilterInfo();
                hr = pFilter.QueryFilterInfo(out fInfo);
                if (DsHlp.FAILED(hr))
                {
                    Marshal.ReleaseComObject(pFilter);
                    continue;  // don't touch this one
                }

                // The FILTER_INFO structure holds a pointer to the Filter Graph
                // Manager, with a reference count that must be released.
                if (fInfo.pGraph != null)
                    Marshal.ReleaseComObject(fInfo.pGraph);

                if (fInfo.achName == null || fSourceInfo.achName == null)
                {
                    Marshal.ReleaseComObject(pFilter);
                    continue;  
                }

                if (fInfo.achName == fSourceInfo.achName) // source filter
                {
                    Marshal.ReleaseComObject(pFilter);
                    continue;
                }
                              
                IPin pPin = DsUtils.GetPin(pFilter, PinDirection.Input, true, 0);
                if (pPin == null)
                {
                    // this filter does not have connected input pins
                    pGraph.pGraphBuilder.RemoveFilter(pFilter);
                    Marshal.ReleaseComObject(pFilter);
                    bCallAgain = true;
                    break;
                }
                else
                {
                    // this filter is connected, let's try another one
                    Marshal.ReleaseComObject(pPin);
                    Marshal.ReleaseComObject(pFilter);
                }     
            }
            
            Marshal.ReleaseComObject(pEnumFilters);
            if (bCallAgain)
                RemoveRedundantFilters(pGraph);
        }

        protected virtual void OnFailedStreamsAvailable(IList<StreamInfo> streams)
        {
            if (FailedStreamsAvailable != null)
            {
                FailedStreamsAvailable(streams);
            }
        }
    }
}
