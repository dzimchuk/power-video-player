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

namespace Pvp.Core.MediaEngine
{
    public class UserDecisionEventArgs : EventArgs
    {
        public UserDecisionEventArgs(string message)
        {
            _message = message;
        }

        private readonly string _message;

        public bool Accept { get; set; }

        public string Message
        {
            get { return _message; }
        }
    }
}
