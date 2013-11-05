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
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.MediaEngine.FilterGraphs;

namespace Pvp.Core.MediaEngine.Internal
{
    public delegate void FailedStreamsHandler(IList<StreamInfo> streams);

    public delegate void ThrowExceptionForHRPointer(int hr, GraphBuilderError error);
    
    /// <summary>
    /// 
    /// </summary>
    internal static class FilterGraphBuilder
    {
        public static IFilterGraph BuildFilterGraph(string source, 
                                                    MediaSourceType mediaSourceType, 
                                                    IntPtr hMediaWindow, 
                                                    Renderer preferredVideoRenderer, 
                                                    Action<string> onErrorCallback, 
                                                    Func<string, bool> onPartialSuccessCallback)
        {
            IFilterGraph filterGraph = null;
            var success = false;
            try
            {
                TraceSink.GetTraceSink().TraceInformation(
                    String.Format("Start building filter graph. Source: {0}. WhatToPlay: {1}. PreferredVideoRenderer: {2}.",
                                  source, mediaSourceType, preferredVideoRenderer));

                filterGraph = GetInitialFilterGraph(source, mediaSourceType);

                var parameters = new FilterGraphBuilderParameters
                                     {
                                         Source = source,
                                         MediaWindowHandle = hMediaWindow,
                                         PreferredVideoRenderer = preferredVideoRenderer,
                                         DiscPath = source,
                                         Flags = AM_DVD_GRAPH_FLAGS.AM_DVD_HWDEC_PREFER,
                                         OnPartialSuccessCallback = onPartialSuccessCallback
                                     };

                filterGraph.BuildUp(parameters);
#if DEBUG
                filterGraph.AddToRot();
#endif
                TraceSink.GetTraceSink().TraceInformation("The graph was built successfully.");
                success = true;

                return filterGraph;
            }
            catch (AbortException)
            {
                TraceSink.GetTraceSink().TraceWarning("User abort.");
                return null;
            }
            catch (FilterGraphBuilderException e)
            {
                TraceSink.GetTraceSink().TraceError(e.ToString());
                onErrorCallback(e.Message);
                return null;
            }
            catch (Exception e)
            {
                TraceSink.GetTraceSink().TraceError(e.ToString());
                onErrorCallback(e.Message);
                return null;
            }
            finally
            {
                if (!success && filterGraph != null)
                {
                    filterGraph.Dispose();
                }
            }
        }

        private static IFilterGraph GetInitialFilterGraph(string source,
                                                          MediaSourceType mediaSourceType)
        {
            IFilterGraph filterGraph;
            if (mediaSourceType == MediaSourceType.Dvd)
            {
                filterGraph = new DvdFilterGraph();
            }
            else
            {
                var sourceType = SourceAnalizer.GetSourceType(source);
                switch (sourceType.Type)
                {
                    case SourceType.Basic:
                    case SourceType.Asf:
                    case SourceType.Mkv:
                    case SourceType.Flv:
                        filterGraph = new RegularFilterGraph(sourceType.Type, sourceType.ClsId);
                        break;
                    case SourceType.Dvd:
                        filterGraph = new DvdFilterGraph();
                        break;
                    default:
                        TraceSink.GetTraceSink().TraceWarning("Could not identify source type.");
                        throw new FilterGraphBuilderException(GraphBuilderError.CantPlayFile);
                }
            }

            return filterGraph;
        }
    }
}
