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
using System.Windows.Forms;
using Pvp.Core.Native;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// VS.NET like status bar with CAP, NUM and INS indicators
	/// </summary>
	public class NetStatusBarEx : NetStatusBar
	{
		NetStatusBarPanel CapsPanel = new NetStatusBarPanel();
		NetStatusBarPanel NumPanel = new NetStatusBarPanel();
		NetStatusBarPanel InsPanel = new NetStatusBarPanel();
		
		public NetStatusBarEx()
		{
			NetStatusBarPanel panel = new NetStatusBarPanel();
			panel.Text = "Ready";
			panel.AutoSize = StatusBarPanelAutoSize.Spring;
			CapsPanel.Width = NumPanel.Width = InsPanel.Width = 33;
			CapsPanel.Alignment = NumPanel.Alignment = InsPanel.Alignment = 
				HorizontalAlignment.Center;
			InsPanel.Text = "INS";
			Panels.AddRange(new StatusBarPanel[] { panel, CapsPanel, NumPanel, InsPanel } );
			ShowPanels = true;
			Application.Idle += new EventHandler(OnIdle);
		}

		private void OnIdle(object sender, EventArgs e)
		{
			bool CapsLock = (((ushort) WindowsManagement.GetKeyState(0x14 /*VK_CAPITAL*/)) & 0xffff) != 0;
			bool NumLock = (((ushort) WindowsManagement.GetKeyState(0x90 /*VK_NUMLOCK*/)) & 0xffff) != 0;
			bool Insert = (((ushort) WindowsManagement.GetKeyState(0x2D /*VK_INSERT*/)) & 0xffff) != 0;
			if (CapsLock && CapsPanel.Text.Length == 0)
				CapsPanel.Text = "CAP";
			else if (!CapsLock && CapsPanel.Text.Length != 0)
				CapsPanel.Text = "";

			if (NumLock && NumPanel.Text.Length == 0)
				NumPanel.Text = "NUM";
			else if (!NumLock && NumPanel.Text.Length != 0)
				NumPanel.Text = "";

			if (Insert && InsPanel.Text == "OVR")
				InsPanel.Text = "INS";
			else if (!Insert && InsPanel.Text == "INS")
				InsPanel.Text = "OVR";
		}
	}
}
