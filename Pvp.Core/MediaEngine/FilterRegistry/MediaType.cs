using System;

namespace Pvp.Core.MediaEngine.FilterRegistry
{
    [Serializable]
    public class MediaType
    {
        private readonly Guid _majortype;
        private readonly Guid _subtype;

        public MediaType(Guid major, Guid sub)
        {
            _majortype = major;
            _subtype = sub;
        }

        public Guid Majortype
        {
            get { return _majortype; }
        }

        public Guid Subtype
        {
            get { return _subtype; }
        }

        public Guid[] ToIdPair()
        {
            return new[] { _majortype, _subtype };
        }
    }
}