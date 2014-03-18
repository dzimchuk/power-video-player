using System;

namespace Pvp.Core.MediaEngine
{
    public class SelectableStream
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Guid MajorType { get; set; }
        public Guid SubType { get; set; }
    }
}