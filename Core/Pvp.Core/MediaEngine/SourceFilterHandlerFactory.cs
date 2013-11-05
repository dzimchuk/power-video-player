using System;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterGraphs;
using Pvp.Core.MediaEngine.SourceFilterHandlers;

namespace Pvp.Core.MediaEngine
{
    internal static class SourceFilterHandlerFactory
    {
        public static ISourceFilterHandler AddSourceFilter(IGraphBuilder graphBuilder, string source, Guid recommendedSourceFilterId)
        {
            IBaseFilter sourceFilter = null;

            int hr;
//            if (recommendedSourceFilterId != Guid.Empty)
//            {
//                var pBaseFilter = DsUtils.GetFilter(recommendedSourceFilterId, false);
//                if (pBaseFilter != null)
//                {
//                    hr = graphBuilder.AddFilter(pBaseFilter, source);
//                    if (DsHlp.SUCCEEDED(hr))
//                    {
//                        IFileSourceFilter pFileSourceFilter = null;
//                        try
//                        {
//                            pFileSourceFilter = (IFileSourceFilter)pBaseFilter;
//                        }
//                        catch
//                        {
//                        }
//
//                        if (pFileSourceFilter != null)
//                        {
//                            hr = pFileSourceFilter.Load(source, IntPtr.Zero);
//                            if (DsHlp.SUCCEEDED(hr))
//                            {
//                                sourceFilter = pBaseFilter; // success
//                            }
//                            else
//                            {
//                                Marshal.FinalReleaseComObject(pBaseFilter);
//                                // TODO trace something here
//                            }
//                        }
//                        else
//                        {
//                            Marshal.FinalReleaseComObject(pBaseFilter);
//                            // TODO trace something here
//                        }
//                    }
//                    else
//                    {
//                        Marshal.FinalReleaseComObject(pBaseFilter);
//                        // TODO trace something here
//                    }
//                }
//            }

            // SourceAnalyzer first searches in MEDIATYPE_Stream for appropriate filters but according to 
            // http://msdn.microsoft.com/en-us/library/windows/desktop/dd377513(v=vs.85).aspx it should first check for filters that
            // are registered by extensions ((Wow6432Node)Media Type\Extensions)

            if (sourceFilter == null)
            {
                // last resort
                hr = graphBuilder.AddSourceFilter(source, null, out sourceFilter);
                hr.ThrowExceptionForHR(GraphBuilderError.SourceFilter);
            }

            if (sourceFilter == null)
            {
                throw new FilterGraphBuilderException(GraphBuilderError.SourceFilter);
            }

            return new RegularSourceFilterHandler(sourceFilter);
        }
    }
}