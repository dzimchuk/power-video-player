using System;

namespace Pvp.Core.MediaEngine.Internal
{
    internal class SourceInfo
    {
        public SourceInfo()
        {
            Type = SourceType.Unknown;
            ClsId = Guid.Empty;
        }

        public SourceType Type { get; set; }
        public Guid ClsId { get; set; }
    }
}