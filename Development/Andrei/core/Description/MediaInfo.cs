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
using System.Collections;
using Dzimchuk.DirectShow;

namespace Dzimchuk.MediaEngine.Core.Description
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaInfo
    {
        public string source;
        public Guid StreamSubType;
        internal ArrayList streams;
        
        public MediaInfo()
        {
            streams = new ArrayList();
        }

        public int NumberOfStreams
        {
            get { return streams.Count; }
        }

        public StreamInfo GetStreamInfo(int nStream)
        {
            return nStream < streams.Count ? (StreamInfo)streams[nStream] : null;
        }

        public string GetStreamSubType()
        {
            string strSubType;
            if (StreamSubType==MediaSubType.DVD)
                strSubType=Resources.Resources.mi_dvd_disc;
            else if (StreamSubType==MediaSubType.AIFF)
                strSubType = Resources.Resources.mi_aiff;
            else if (StreamSubType==MediaSubType.Asf)
                strSubType = Resources.Resources.mi_asf;
            else if (StreamSubType==MediaSubType.Avi)
                strSubType = Resources.Resources.mi_avi;
            else if (StreamSubType==MediaSubType.AU)
                strSubType = Resources.Resources.mi_au;
            else if (StreamSubType==MediaSubType.DssAudio)
                strSubType=Resources.Resources.mi_dss_audio;
            else if (StreamSubType==MediaSubType.DssVideo)
                strSubType=Resources.Resources.mi_dss_video;
            else if (StreamSubType==MediaSubType.MPEG1Audio)
                strSubType=Resources.Resources.mi_mpeg1_audio;
            else if (StreamSubType==MediaSubType.MPEG1System)
                strSubType=Resources.Resources.mi_mpeg1_system;
            else if (StreamSubType==MediaSubType.MPEG1Video)
                strSubType=Resources.Resources.mi_mpeg1_video;
            else if (StreamSubType==MediaSubType.MPEG1VideoCD)
                strSubType=Resources.Resources.mi_mpeg1_video_cd;
            else if (StreamSubType==MediaSubType.WAVE)
                strSubType = Resources.Resources.mi_wav;
            else
                strSubType = String.Empty;
        
            return strSubType;
        }
    }
}
