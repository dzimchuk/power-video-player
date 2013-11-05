using System;

namespace Pvp.Core.MediaEngine
{
    public enum GraphBuilderError
    {
        Unknown,
        FilterGraphManager,
        SourceFilter,
        NecessaryInterfaces,
        VideoRenderer,
        AddVideoRenderer,
        AddVMR9,
        ConfigureVMR9,
        AddVMR,
        ConfigureVMR,
        CantPlayFile,
        CantRenderFile,
        DirectSoundFilter,
        AddDirectSoundFilter,
        DvdGraphBuilder, 
        CantPlayDisc,
        NoVideoDimension,
        AddEVR,
        ConfigureEVR
    }
}