using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.Core.MediaEngine.Description;

namespace Pvp.App.ViewModel
{
    public interface IFailedStreamsContainer
    {
        IEnumerable<StreamInfo> GetFailedStreams();
        void SetFailedStreams(IEnumerable<StreamInfo> streams);
        void Clear();
    }
}