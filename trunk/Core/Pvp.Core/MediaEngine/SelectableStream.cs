using System;

namespace Pvp.Core.MediaEngine
{
    public class SelectableStream : IEquatable<SelectableStream>
    {
        public bool Equals(SelectableStream other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SelectableStream)obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(SelectableStream left, SelectableStream right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SelectableStream left, SelectableStream right)
        {
            return !Equals(left, right);
        }

        public int Index { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Guid MajorType { get; set; }
        public Guid SubType { get; set; }
    }
}