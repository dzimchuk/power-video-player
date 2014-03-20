using System;
using Pvp.Core.DirectShow;

namespace Pvp.Core.MediaEngine.SourceFilterHandlers
{
    internal interface IAudioStreamHandler : IDisposable
    {
        void RenderAudio(IGraphBuilder pGraphBuilder, IBaseFilter splitter);
        int AudioStreamsCount { get; }
        int CurrentAudioStream { get; set; }
        bool SetVolume(int volume);
        bool GetVolume(out int volume);
        string GetAudioStreamName(int nStream);
        void OnExternalStreamSelection();
        void EnumMediaTypes(IPin pin, AMMediaType pinMediaType, Action<AMMediaType> action);
    }
}