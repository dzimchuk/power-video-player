using System;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.SourceFilterHandlers
{
    internal interface ISubpictureStreamHandler : IDisposable
    {
        void RenderSubpicture(IGraphBuilder pGraphBuilder, IBaseFilter splitter, IRenderer renderer);
        int SubpictureStreamsCount { get; }
        int CurrentSubpictureStream { get; set; }
        string GetSubpictureStreamName(int nStream);
        void OnExternalStreamSelection();
        bool IsSubpictureStreamEnabled(int nStream);
        bool EnableSubpicture(bool bEnable);
        bool IsSubpictureEnabled();
    }
}