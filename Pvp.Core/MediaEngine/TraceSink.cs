/* ****************************************************************************
 *
 * Copyright (c) Andrei Dzimchuk. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;

namespace Pvp.Core.MediaEngine
{
    public sealed class TraceSink
    {
        private static TraceSink _traceSink;
        private TraceSwitch _traceSwitch;
        private const string MessageFormat = "{0}: {1}: {2}";
        private const string MessageFormatSimple = "{0}: {1}";
        private const string CategoryWarning = "WARNING";
        private const string CategoryError = "ERROR";

        private TraceSink()
        {
        }

        public static TraceSink GetTraceSink()
        {
            return _traceSink ?? (_traceSink = new TraceSink());
        }

        public static TraceListenerCollection Listeners
        {
            get { return Trace.Listeners; }
        }

        public TraceSwitch TraceSwitch
        {
            set { _traceSwitch = value; }
        }

        public void TraceMessage(string message, string category)
        {
            Trace.WriteLine(String.Format(MessageFormat, DateTime.Now.ToString(), category, message));
        }

        public void TraceMessage(string message)
        {
            Trace.WriteLine(String.Format(MessageFormatSimple, DateTime.Now.ToString(), message));
        }

        public void TraceInformation(string message)
        {
            if (_traceSwitch != null && _traceSwitch.TraceInfo)
                TraceMessage(message);
        }

        public void TraceWarning(string message)
        {
            if (_traceSwitch != null && _traceSwitch.TraceWarning)
                TraceMessage(message, CategoryWarning);
        }

        public void TraceError(string message)
        {
            if (_traceSwitch != null && _traceSwitch.TraceError)
                TraceMessage(message, CategoryError);
        }
    }
}