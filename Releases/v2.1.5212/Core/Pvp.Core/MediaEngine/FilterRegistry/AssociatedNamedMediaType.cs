using System;

namespace Pvp.Core.MediaEngine.FilterRegistry
{
    [Serializable]
    internal class AssociatedNamedMediaType
    {
        private Guid _associatedFilterClassId;
        private readonly string _mediaTypeFriendlyName;

        public AssociatedNamedMediaType(string mediaTypeFriendlyName, Guid associatedFilterClassId)
        {
            _associatedFilterClassId = associatedFilterClassId;
            _mediaTypeFriendlyName = mediaTypeFriendlyName;
        }

        public string MediaTypeFriendlyName
        {
            get { return _mediaTypeFriendlyName; }
        }

        public Guid AssociatedFilterClassId
        {
            get { return _associatedFilterClassId; }
            set { _associatedFilterClassId = value; }
        }
    }
}