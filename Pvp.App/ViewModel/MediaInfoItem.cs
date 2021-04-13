using System;

namespace Pvp.App.ViewModel
{
    public class MediaInfoItem
    {
        public MediaInfoItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}