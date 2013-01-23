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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;


namespace Pvp
{
    [RunInstaller(true)]
    public partial class PVPInstaller : Installer
    {
        public PVPInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            MainForm.HandleRegApp();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            MainForm.HandleUnRegApp();
        }
    }
}
