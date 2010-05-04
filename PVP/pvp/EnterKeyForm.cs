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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Dzimchuk.PVP
{
	/// <summary>
	/// Summary description for EnterKeyForm.
	/// </summary>
	public class EnterKeyForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelKey;
		private System.Windows.Forms.Button btnOK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		const string ctrl = "Ctrl+";
		const string shift = "Shift+";
		const string alt = "Alt+";
		static System.Text.StringBuilder builder = new System.Text.StringBuilder();
			
		Keys keydata;

		public EnterKeyForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		public Keys KeyData
		{
			get { return keydata; }
			set
			{
				keydata = value;
				labelKey.Text = GetString(keydata);
			}
		}

		public static string GetString(Keys keydata)
		{
			if (keydata == Keys.None)
				return String.Empty;
			builder.Remove(0, builder.Length);
			if ((keydata & Keys.Control) != 0)
				builder.Append(ctrl);
			if ((keydata & Keys.Shift) != 0)
				builder.Append(shift);
			if ((keydata & Keys.Alt) != 0)
				builder.Append(alt);
			Keys code = keydata & Keys.KeyCode;
			if (code != Keys.ControlKey && code != Keys.ShiftKey && 
				code != Keys.Menu)
			{
				if (code >= Keys.D0 && code <= Keys.D9)
					builder.Append((code - Keys.D0).ToString());
				else
					builder.Append(code.ToString());
			}
			else
				builder.Remove(builder.Length-1, 1);
			return builder.ToString();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown (e);
			keydata = e.KeyData;
			labelKey.Text = GetString(keydata);
			e.Handled = true;
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated (e);
			ActiveControl = null;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterKeyForm));
            this.label1 = new System.Windows.Forms.Label();
            this.labelKey = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // labelKey
            // 
            this.labelKey.AccessibleDescription = null;
            this.labelKey.AccessibleName = null;
            resources.ApplyResources(this.labelKey, "labelKey");
            this.labelKey.Font = null;
            this.labelKey.Name = "labelKey";
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.TabStop = false;
            // 
            // EnterKeyForm
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.BackgroundImage = null;
            this.ControlBox = false;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.labelKey);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = null;
            this.KeyPreview = true;
            this.Name = "EnterKeyForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

		}
		#endregion
	}
}
