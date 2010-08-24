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
using System.Collections.Generic;
using System.Text;

namespace Dzimchuk.MediaEngine.Core
{
    public class Trace
    {
        private static Trace trace;
        private System.Diagnostics.TraceSwitch traceSwitch;
        private const string messageFormat = "{0}: {1}: {2}";
        private const string messageFormatSimple = "{0}: {1}";
        private const string categoryWarning = "WARNING";
        private const string categoryError = "ERROR";

        public static Trace GetTrace()
        {
            if (trace == null)
                trace = new Trace();
            return trace;
        }

        public static System.Diagnostics.TraceListenerCollection Listeners
        {
            get
            {
                return System.Diagnostics.Trace.Listeners;
            }
        }

        public System.Diagnostics.TraceSwitch TraceSwitch
        {
            set
            {
                traceSwitch = value;
            }
        }

        public void TraceMessage(string message, string category)
        {
            System.Diagnostics.Trace.WriteLine(String.Format(messageFormat, DateTime.Now.ToString(), category, message));
        }

        public void TraceMessage(string message)
        {
            System.Diagnostics.Trace.WriteLine(String.Format(messageFormatSimple, DateTime.Now.ToString(), message));
        }

        public void TraceInformation(string message)
        {
            if (traceSwitch != null && traceSwitch.TraceInfo)
                TraceMessage(message);
        }

        public void TraceWarning(string message)
        {
            if (traceSwitch != null && traceSwitch.TraceWarning)
                TraceMessage(message, categoryWarning);
        }

        public void TraceError(string message)
        {
            if (traceSwitch != null && traceSwitch.TraceError)
                TraceMessage(message, categoryError);
        }
    }
}
