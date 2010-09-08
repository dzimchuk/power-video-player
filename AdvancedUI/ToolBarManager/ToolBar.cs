using System;
using System.Windows.Forms;

namespace Dzimchuk.AUI
{
	/// <summary>
	/// This class automaticly sets Appearance to Flat and Divider to false;
	/// </summary>
	public class ToolBar : System.Windows.Forms.ToolBar
	{
		public ToolBar(string strTitle)
		{
			Text = strTitle;
			Appearance = ToolBarAppearance.Flat;
			Divider = false;
		}
	}
}
