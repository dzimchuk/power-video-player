using System;

namespace Pvp.App.Util.FileTypes
{
    public class DocTypeInfo
    {
        public readonly string Extension;
        public readonly string Description;
        public readonly string DefaultIcon;
        public readonly string OpenCommand;
        public readonly string PlayCommand;
        public readonly bool PlayCommandNeeded;

        public DocTypeInfo(string extension, string description, string defaultIcon, string openCommand, string playCommand, bool playCommandNeeded)
        {
            Extension = extension;
            Description = description;
            DefaultIcon = defaultIcon;
            OpenCommand = openCommand;
            PlayCommand = playCommand;
            PlayCommandNeeded = playCommandNeeded;
        }
    }
}