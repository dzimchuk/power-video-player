using System;
using System.Collections.Generic;
using System.Linq;
using Pvp.App.ViewModel;
using Pvp.Core.MediaEngine.Description;

namespace Pvp.App.Service
{
    internal class FailedStreamsContainer : IFailedStreamsContainer
    {
        private readonly List<StreamInfo> _streams = new List<StreamInfo>();

        public IEnumerable<StreamInfo> GetFailedStreams()
        {
            return _streams.ToArray();
        }

        public void SetFailedStreams(IEnumerable<StreamInfo> streams)
        {
            Clear();
            _streams.AddRange(streams);
        }

        public void Clear()
        {
            _streams.Clear();
        }
    }
}