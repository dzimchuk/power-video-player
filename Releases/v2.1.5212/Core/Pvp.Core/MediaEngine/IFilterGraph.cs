using System;
using System.Collections.Generic;
using Pvp.Core.MediaEngine.Description;
using Pvp.Core.Native;

namespace Pvp.Core.MediaEngine
{
    internal interface IFilterGraph : IDisposable
    {
        void BuildUp(FilterGraphBuilderParameters parameters);
        void AddToRot();

        MediaInfo MediaInfo { get; }
        SourceType SourceType { get; }
        IRenderer Renderer { get; }

        GraphState GraphState { get; }
        bool IsGraphSeekable { get; }
        /// <summary>
        /// number of 100 nanoseconds units (by default)
        /// </summary>
        long Duration { get; }

        double Rate { get; }
        int AudioStreamsCount { get; }
        int CurrentAudioStream { get; set; }
        int FilterCount { get; }
        GDI.RECT SourceRect { get; }
        double AspectRatio { get; }

        bool PauseGraph();
        bool ResumeGraph();
        void SetCurrentPosition(long time);
        bool StopGraph();
        void SetRate(double rate);
        string GetAudioStreamName(int nStream);
        /// <summary>
        /// to get it in seconds you should divide 'em by 10000000
        /// </summary>
        long GetCurrentPosition();

        string GetFilterName(int nFilterNum);

        bool SetVolume(int volume);
        bool GetVolume(out int volume);
        bool DisplayFilterPropPage(IntPtr hParent, string strFilter, bool bDisplay);
        void HandleGraphEvent();
        event EventHandler<string> GraphError;
        event EventHandler PlayBackComplete;
        event EventHandler ErrorAbort;
        event FailedStreamsHandler FailedStreamsAvailable;
        IEnumerable<SelectableStream> GetSelectableStreams(string filterName);
        bool IsStreamSelected(string filterName, int index);
        void SelectStream(string filterName, int index);

        int NumberOfSubpictureStreams { get; }
        int CurrentSubpictureStream { get; set; }
        bool EnableSubpicture(bool bEnable);
        string GetSubpictureStreamName(int nStream);
        bool IsSubpictureEnabled();
        bool IsSubpictureStreamEnabled(int ulStreamNum);
    }
}