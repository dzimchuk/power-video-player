using System;

namespace Pvp.App.Messaging
{
    internal class ResizeMainWindowCommandMessage : CommandMessage
    {
        private readonly Tuple<double, double> _size;
        private readonly double? _coefficient;
        private readonly bool _centerWindow;

        public ResizeMainWindowCommandMessage(Command command, bool centerWindow)
            : this(command, null, new double?(), centerWindow)
        {
        }

        public ResizeMainWindowCommandMessage(Command command, Tuple<double, double> size, double? coeffiecient, bool centerWindow)
            : base(command)
        {
            _size = size;
            _coefficient = coeffiecient;
            _centerWindow = centerWindow;
        }

        public Tuple<double, double> Size
        {
            get { return _size; }
        }

        public bool CenterWindow
        {
            get { return _centerWindow; }
        }

        public double? Coefficient
        {
            get { return _coefficient; }
        }
    }
}