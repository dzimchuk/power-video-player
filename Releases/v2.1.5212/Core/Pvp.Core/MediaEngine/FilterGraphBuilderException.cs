using System;

namespace Pvp.Core.MediaEngine
{
    internal class FilterGraphBuilderException : Exception
    {
        public FilterGraphBuilderException(GraphBuilderError error) : base(error.GetErrorText())
        {
        }

        public FilterGraphBuilderException(GraphBuilderError error, Exception innerException)
            : base(error.GetErrorText(), innerException)
        {
        }

        public FilterGraphBuilderException(string message)
            : base(message)
        {
        }
    }
}