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
using System.Drawing;

namespace Pvp.Core.WindowsForms
{
    public class InitSizeEventArgs : EventArgs
    {
        private Size _newSize;

        public InitSizeEventArgs(Size newSize)
            : base()
        {
            _newSize = newSize;
        }

        public Size NewVideSize { get { return _newSize; } }
    }
}