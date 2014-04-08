using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine.FilterRegistry;
using Pvp.Core.MediaEngine.SourceFilterHandlers;
using MediaType = Pvp.Core.DirectShow.MediaType;

namespace Pvp.Core.MediaEngine.StreamHandlers
{
    internal class DirectVobSubSubpictureStreamHandler : ISubpictureStreamHandler
    {
        private static readonly FilterDescription _filterDescription =
            new FilterDescription("DirectVobSub (auto-loading version)", new Guid("9852A670-F845-491B-9BE6-EBD841B8A613"));
        private IAMStreamSelect _streamSelect;

        private readonly List<SelectableStream> _subpicutureStreams;

        public DirectVobSubSubpictureStreamHandler()
        {
            _subpicutureStreams = new List<SelectableStream>();
        }

        public void Dispose()
        {
        }

        private static bool IsFilterPresent
        {
            get
            {
                var baseFilter = DsUtils.GetFilter(_filterDescription.ClassId, false);
                if (baseFilter != null)
                {
                    Marshal.FinalReleaseComObject(baseFilter);
                    return true;
                }

                return false;
            }
        }

        public static bool CanHandle(IBaseFilter splitter)
        {
            var result = false;

            if (IsFilterPresent)
            {
                var pPin = DsUtils.GetPin(splitter, PinDirection.Output, new[] { MediaType.Subtitle });
                if (pPin != null)
                {
                    Marshal.ReleaseComObject(pPin);

                    var pStreamSelect = splitter as IAMStreamSelect;
                    if (pStreamSelect != null)
                    {
                        result = pStreamSelect.GetSelectableStreams().Any(s => s.MajorType == MediaType.Subtitle);
                    }
                }
            }

            return result;
        }

        public void RenderSubpicture(IGraphBuilder pGraphBuilder, IBaseFilter splitter, IRenderer renderer)
        {
            var pPin = DsUtils.GetPin(splitter, PinDirection.Output, new[] { MediaType.Subtitle });
            if (pPin != null)
            {
                try
                {
                    _streamSelect = splitter as IAMStreamSelect;
                    if (_streamSelect != null)
                    {
                        TryRenderSubpicture(pGraphBuilder, pPin, renderer);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(pPin);
                }
            }
        }

        private void TryRenderSubpicture(IGraphBuilder pGraphBuilder, IPin subpictureOutPin, IRenderer renderer)
        {
            IPin videoOutPin = null;
            IPin videoRendererInputPin = null;
            try
            {
                videoOutPin = renderer.GetConnectedSourcePin();
                videoRendererInputPin = renderer.GetInputPin();

                if (videoOutPin == null || videoRendererInputPin == null)
                    return;

                TryRenderSubpicture(pGraphBuilder, subpictureOutPin, videoOutPin, videoRendererInputPin);
            }
            catch
            {
                ReconnectVideoStream(pGraphBuilder, videoOutPin, videoRendererInputPin);
            }
            finally
            {
                if (videoOutPin != null)
                {
                    Marshal.ReleaseComObject(videoOutPin);
                }

                if (videoRendererInputPin != null)
                {
                    Marshal.ReleaseComObject(videoRendererInputPin);
                }
            }
        }

        private static void ReconnectVideoStream(IGraphBuilder pGraphBuilder, IPin videoOutPin, IPin videoRendererInputPin)
        {
            var hr = pGraphBuilder.Connect(videoOutPin, videoRendererInputPin);
            hr.ThrowExceptionForHR(GraphBuilderError.CantRenderFile);
        }

        private void TryRenderSubpicture(IGraphBuilder pGraphBuilder, IPin subpictureOutPin, IPin videoOutPin, IPin videoRendererInputPin)
        {
            var subpictureFilter = GetSubpictureFilter(pGraphBuilder);
            if (subpictureFilter == null)
                throw new Exception();

            try
            {
                ConnectVideoToSubpictureFilter(pGraphBuilder, videoOutPin, subpictureFilter);
                ConnectSubpictureToSubpictureFilter(pGraphBuilder, subpictureOutPin, subpictureFilter);
                ConnectSubpictureFilterToRenderer(pGraphBuilder, subpictureFilter, videoRendererInputPin);

                CollectSubpictureStreamsInfo();
            }
            catch
            {
                pGraphBuilder.Disconnect(videoRendererInputPin);
                pGraphBuilder.Disconnect(subpictureOutPin);
                pGraphBuilder.Disconnect(videoOutPin);
                throw;
            }
            finally
            {
                Marshal.FinalReleaseComObject(subpictureFilter);
            }
        }

        private void CollectSubpictureStreamsInfo()
        {
            _subpicutureStreams.AddRange(_streamSelect.GetSelectableStreams().Where(s => s.MajorType == MediaType.Subtitle));
        }

        private static void ConnectSubpictureFilterToRenderer(IGraphBuilder pGraphBuilder, IBaseFilter subpictureFilter, IPin videoRendererInputPin)
        {
            var subpictureVideoOutputPin = GetOutputPin(subpictureFilter);
            try
            {
                var hr = pGraphBuilder.Connect(subpictureVideoOutputPin, videoRendererInputPin);
                hr.ThrowExceptionForHR(GraphBuilderError.CantRenderSubpicture);
            }
            finally
            {
                Marshal.ReleaseComObject(subpictureVideoOutputPin);
            }
        }

        private static void ConnectVideoToSubpictureFilter(IGraphBuilder pGraphBuilder, IPin videoOutPin, IBaseFilter subpictureFilter)
        {
            var videoInputPin = GetInputPin(subpictureFilter);
            try
            {
                var hr = pGraphBuilder.Connect(videoOutPin, videoInputPin);
                hr.ThrowExceptionForHR(GraphBuilderError.CantRenderSubpicture);
            }
            finally
            {
                Marshal.ReleaseComObject(videoInputPin);
            }
        }

        private static void ConnectSubpictureToSubpictureFilter(IGraphBuilder pGraphBuilder, IPin subpictureOutPin, IBaseFilter subpictureFilter)
        {
            var subpictureInputPin = GetInputPin(subpictureFilter);
            try
            {
                var hr = pGraphBuilder.Connect(subpictureOutPin, subpictureInputPin);
                hr.ThrowExceptionForHR(GraphBuilderError.CantRenderSubpicture);
            }
            finally
            {
                Marshal.ReleaseComObject(subpictureInputPin);
            }
        }

        private static IPin GetInputPin(IBaseFilter subpictureFilter)
        {
            var pin = DsUtils.GetPin(subpictureFilter, PinDirection.Input);
            if (pin == null)
                throw new Exception();

            return pin;
        }

        private static IPin GetOutputPin(IBaseFilter subpictureFilter)
        {
            var pin = DsUtils.GetPin(subpictureFilter, PinDirection.Output, new[] { MediaType.Video });
            if (pin == null)
                throw new Exception();

            return pin;
        }

        private static IBaseFilter GetSubpictureFilter(IGraphBuilder pGraphBuilder)
        {
            return pGraphBuilder.AddFilter(_filterDescription);
        }

        public int SubpictureStreamsCount
        {
            get { return _subpicutureStreams.Count; }
        }

        public int CurrentSubpictureStream
        {
            get
            {
                var index = 0;

                for (var i = 0; i < _subpicutureStreams.Count; i++)
                {
                    if (_subpicutureStreams[i].Enabled)
                    {
                        index = i;
                        break;
                    }
                }

                return index;
            }
            set
            {
                if (value < 0 || value >= _subpicutureStreams.Count)
                    return;

                var stream = _subpicutureStreams.Skip(value).Take(1).Single();
                if (_streamSelect.SelectStream(stream.Index))
                {
                    stream.Enabled = true;
                    _subpicutureStreams.Where(s => s != stream).ToList().ForEach(s => s.Enabled = false);
                }
            }
        }

        public string GetSubpictureStreamName(int nStream)
        {
            if (nStream < 0 || nStream >= _subpicutureStreams.Count)
                return string.Empty;

            return _subpicutureStreams[nStream].Name;
        }

        public void OnExternalStreamSelection()
        {
            _subpicutureStreams.ForEach(s =>
            {
                s.Enabled = _streamSelect.IsStreamSelected(s.Index);
            });
        }

        public bool IsSubpictureStreamEnabled(int nStream)
        {
            return true;
        }

        public bool EnableSubpicture(bool bEnable)
        {
            return true;
        }

        public bool IsSubpictureEnabled()
        {
            return true;
        }
    }
}