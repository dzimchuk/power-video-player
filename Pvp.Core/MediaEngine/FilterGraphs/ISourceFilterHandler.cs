using System;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.FilterGraphs
{
    internal interface ISourceFilterHandler : IDisposable
    {
        void RenderVideo(IGraphBuilder pGraphBuilder, IRenderer renderer);
        void RenderAudio(IGraphBuilder pGraphBuilder);
        void RenderSubpicture(IGraphBuilder pGraphBuilder, IRenderer renderer);

        void GetMainStreamSubtype(Action<AMMediaType> inspect);
        void GetStreamsMediaTypes(Action<AMMediaType> inspect);
        int AudioStreamsCount { get; }
        int CurrentAudioStream { get; set; }
        bool SetVolume(int volume);
        bool GetVolume(out int volume);
        string GetAudioStreamName(int nStream);
        void OnExternalStreamSelection();

        int NumberOfSubpictureStreams { get; }
        int CurrentSubpictureStream { get; set; }
        bool EnableSubpicture(bool bEnable);
        string GetSubpictureStreamName(int nStream);
        bool IsSubpictureEnabled();
        bool IsSubpictureStreamEnabled(int nStream);
    }
}