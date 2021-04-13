using System;

namespace Pvp.Core.MediaEngine.FilterRegistry
{
    public class FilterDescription
    {
        private readonly string _name;
        private readonly Guid _classId;

        public FilterDescription(string name, Guid classId)
        {
            _name = name;
            _classId = classId;
        }

        public string Name
        {
            get { return _name; }
        }

        public Guid ClassId
        {
            get { return _classId; }
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}