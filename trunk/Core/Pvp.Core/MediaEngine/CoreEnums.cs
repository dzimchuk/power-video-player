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

namespace Pvp.Core.MediaEngine
{
    public enum Renderer
    {
        VR,
        VMR_Windowless,
        VMR_Windowed,
        VMR9_Windowless,
        VMR9_Windowed,
        EVR
    }

    public enum MediaSourceType
    {
        File,
        Dvd
    }

    public enum GraphState
    {
        Running,
        Paused,
        Stopped,
        Reset
    }

    public enum SourceType
    {
        Unknown,
        Basic,		// avi, mpeg...
        Asf,		// asf, wmv, wma
        DVD,		// DVD disc
        Mkv,        // matroska
        Flv
    }

    public enum AspectRatio
    {
        AR_ORIGINAL,
        AR_16x9,
        AR_4x3,
        AR_47x20,
        AR_1x1,
        AR_5x4,
        AR_16x10,
        AR_FREE
    }

    public enum VideoSize
    {
        SIZE_FREE = 0,
        SIZE100 = 1,
        SIZE200 = 2,
        SIZE50 = 3
    }
}
