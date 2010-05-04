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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Dzimchuk.MediaEngine.Core;

namespace Dzimchuk.PVP
{
	public enum MouseWheelAction
	{
		Volume,
		Seek
	}
	
	/// <summary>
	/// Summary description for SettingsForm.
	/// </summary>
	public class SettingsForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.GroupBox grpPlayerOptions;
		private System.Windows.Forms.CheckBox chkFullscreen;
		private System.Windows.Forms.CheckBox chkAutoPlay;
		private System.Windows.Forms.CheckBox chkVolume;
		private System.Windows.Forms.CheckBox chkCenter;
		private System.Windows.Forms.CheckBox chkLogo;
		private System.Windows.Forms.CheckBox chkOnTop;
		private System.Windows.Forms.GroupBox grpSysTray;
		private System.Windows.Forms.RadioButton radioTaskbarOnly;
		private System.Windows.Forms.RadioButton radioMinToTray;
		private System.Windows.Forms.CheckBox chkShowTrayAlways;
		private System.Windows.Forms.RadioButton radioSysTrayOnly;
		private System.Windows.Forms.GroupBox grpRenderer;
		private System.Windows.Forms.RadioButton radioVR;
		private System.Windows.Forms.RadioButton radioVMR;
		private System.Windows.Forms.RadioButton radioVMR9;
		private System.Windows.Forms.GroupBox grpVMRMode;
		private System.Windows.Forms.RadioButton radioWindowless;
		private System.Windows.Forms.RadioButton radioWindowed;
		private System.Windows.Forms.Label labelTypes;
		private System.Windows.Forms.ListView listTypes;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage pageGeneral;
		private System.Windows.Forms.TabPage pageVideo;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnSelectAll;
		private System.Windows.Forms.Button btnRemoveAll;
		private System.Windows.Forms.TabPage pageKM;
		private System.Windows.Forms.GroupBox groupMouseWheel;
		private System.Windows.Forms.RadioButton radioVolume;
		private System.Windows.Forms.RadioButton radioSeek;
		private System.Windows.Forms.ColumnHeader columnExtensions;
		private System.Windows.Forms.ColumnHeader columnAction;
		private System.Windows.Forms.ColumnHeader columnKeys;
		private System.Windows.Forms.ListView listKeys;
		private System.Windows.Forms.Button btnKeysDefault;
		private System.Windows.Forms.Button btnKeysClearAll;
		private System.Windows.Forms.Button btnKeysClear;
		private System.Windows.Forms.TabPage pageFileTypes;
		private System.Windows.Forms.TabPage pagePrefFilters;
		private System.Windows.Forms.GroupBox grpTechnique;
		private System.Windows.Forms.CheckBox chkRegularGraphs;
		private System.Windows.Forms.CheckBox chkDVDGraphs;
		private System.Windows.Forms.Panel panelFilters;
		
		public const string strKeysFileOpen = "FileOpen";
		public const string strKeysFileClose = "FileClose";
		public const string strKeysFileInfo = "FileInfo";
		public const string strKeysPlay = "Play";
		public const string strKeysStop = "Stop";
		public const string strKeysPause = "Pause";
		public const string strKeys50 = "50 %";
		public const string strKeys100 = "100 %";
		public const string strKeys200 = "200 %";
		public const string strKeysFree = "Free";
		public const string strKeysFullscreen = "Full Screen";
		public const string strKeysRepeat = "Repeat";
		public const string strKeysMute = "Mute";
		public const string strKeysUp = "Up";
		public const string strKeysDown = "Down";
		public const string strKeysBack = "Back";
		public const string strKeysForth = "Forth";
		public const string strKeysAbout = "About";
		public const string strKeysExit = "Exit";
		public const string strKeysPref = "Pref";
		public const string strKeysAspectOriginal = "Keep Original";
		public const string strKeysAspect16_9 = "16:9";
		public const string strKeysAspect4_3 = "4:3";
		public const string strKeysAspect47_20 = "47:20";
		public const string strKeysAspectFree = "Aspect_Free";

		const string strDivx3 = "DivX 3 Video";
		const string strDivx4 = "DivX 4 Video";
		const string strDivx5 = "DivX 5 Video";
		const string strXvid = "XviD Video";
		const string strMPEG1Video = "MPEG 1 Video";
		const string strMPEG2Video = "MPEG 2 Video";
		const string strAC3 = "Dolby AC3 Audio";
		const string strMP3 = "MPEG Layer3 Audio";
		const string strAvi = "Avi";
		const string strMPEG1 = "MPEG 1";
		const string strMPEG2 = "MPEG 2";
		const string strDVDNavigator = "DVD Navigator";
				
		bool bFileTypesChanged;
		int nSysTray;
		bool bRestartTriggered;
		int count;
		EnterKeyForm KeyForm;
        SizeF sizefOrigAutoScaleDimensions;
        private RadioButton radioEVR;
        private Button btnRecommendedRenderer;
		public static Hashtable htDefault; // will be initialized by the static constructor
		        
		public event EventHandler Apply;

		public SettingsForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            FillListViews();
            sizefOrigAutoScaleDimensions = AutoScaleDimensions;
			
            CreateFiltersPanel();
            
			foreach(ListViewItem item in listKeys.Items)
				if (item.Tag != null)
					item.Tag = new KeyInfo((string)item.Tag, item);
				else
					item.Font = new Font(item.Font.FontFamily, item.Font.SizeInPoints, 
						FontStyle.Bold);
			listKeys.ItemActivate += new EventHandler(listKeys_ItemActivate);
			listKeys.SelectedIndexChanged += new EventHandler(listKeys_SelectedIndexChanged);
			KeyForm = new EnterKeyForm();

            SetRenderers();
		}

        private void FillListViews()
        {
            IList<FileType> types = Dzimchuk.PVP.FileTypes.GetFileTypes();
            foreach (FileType t in types)
                listTypes.Items.Add(GetListViewItem(t.Description, t.Extension));            

            string indent = "   ";
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_file, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_open_file, strKeysFileOpen));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_close_file, strKeysFileClose));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_file_info, strKeysFileInfo));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_playback, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_play, strKeysPlay));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_pause, strKeysPause));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_stop, strKeysStop));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_repeat, strKeysRepeat));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_full_screen, strKeysFullscreen));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_seek, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_forward, strKeysForth));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_backward, strKeysBack));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_video_size, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_video_size_50, strKeys50));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_video_size_100, strKeys100));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_video_size_200, strKeys200));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_video_size_free, strKeysFree));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_aspect_ratio, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_ar_original, strKeysAspectOriginal));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_ar_4_3, strKeysAspect4_3));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_ar_16_9, strKeysAspect16_9));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_ar_47_20, strKeysAspect47_20));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_ar_free, strKeysAspectFree));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_volume, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_volume_increase, strKeysUp));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_volume_decrease, strKeysDown));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_volume_mute, strKeysMute));
            listKeys.Items.Add(GetListViewItem(indent + Resources.Resources.keys_app, null));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_pref, strKeysPref));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_about, strKeysAbout));
            listKeys.Items.Add(GetListViewItem(Resources.Resources.keys_exit, strKeysExit));
        }

        private ListViewItem GetListViewItem(string text, string tag)
        {
            ListViewItem item = new ListViewItem(text);
            item.Tag = tag;
            return item;
        }

		#region Preferred Filters
		void CreateFiltersPanel()
		{
			string[,] astrTypes = {{strDVDNavigator, strDVDNavigator}, 
									{"Avi Splitter", "Avi"},
									{"MPEG 1 Splitter", strMPEG1},
									{"MPEG 2 Splitter", strMPEG2},
									{strDivx3, strDivx3},
									{strDivx4, strDivx4},
									{strDivx5, strDivx5},
									{strXvid, strXvid},
									{strMPEG1Video, strMPEG1Video},
									{strMPEG2Video, strMPEG2Video},
									{strAC3, strAC3},
									{strMP3, strMP3}};
			int y = 5;
			for (int i=0; i<astrTypes.GetLength(0); i++)
			{
				Label label = new Label();
				label.Parent = panelFilters;
				label.Text = astrTypes[i,0];
				label.Location = new Point(5, y);
				label.Width = 110;
				
				ComboBox cb = new ComboBox();
				cb.Parent = panelFilters;
				cb.DropDownStyle = ComboBoxStyle.DropDownList;
				cb.Location = new Point(label.Right, y);
				cb.Width = panelFilters.Width - cb.Left - (int)(SystemInformation.VerticalScrollBarWidth*1.5);
				cb.Tag = astrTypes[i,1];

				MediaTypeManager.Filter[] filters = MediaTypeManager.GetInstance().GetFilters(astrTypes[i,1]);
				MediaTypeManager.Filter[] afilters = 
					new MediaTypeManager.Filter[filters != null ? filters.Length+1 : 1];
				afilters[0] = new MediaTypeManager.Filter();
				afilters[0].filterName = Resources.Resources.preferred_filter_auto;
				afilters[0].Clsid = Guid.Empty;
				if (filters != null)
					filters.CopyTo(afilters, 1);
				
				cb.DataSource = afilters;
				Guid filterid = MediaTypeManager.GetInstance().GetTypeClsid(astrTypes[i,1]);
				if (filterid != Guid.Empty)
					for(int n=1; n<afilters.Length; n++)
						if (filterid == afilters[n].Clsid)
						{
							cb.SelectedIndex = n;
							break;
						}
								
				y+=label.Height+5;
			}
		}

		public Guid GetTypeClsid(string strType)
		{
			Guid filter = Guid.Empty;
			foreach(Control ctrl in panelFilters.Controls)
			{
				if (ctrl is ComboBox)
				{
					ComboBox cb = (ComboBox)ctrl;
					if ((string)cb.Tag == strType)
					{
						filter = ((MediaTypeManager.Filter)cb.SelectedItem).Clsid;
						break;
					}
				}
			}
			return filter;
		}
		#endregion

		#region Keys stuff
		private class KeyInfo
		{
			public string name;
			
			Keys keydata;
			ListViewItem item;
			
			public KeyInfo(string name, ListViewItem item)
			{
				this.name = name;
				this.item = item;
				item.SubItems.Add(String.Empty);
			}

			public Keys KeyData
			{
				get { return keydata; }
				set
				{
					keydata = value;
					item.SubItems[1].Text = EnterKeyForm.GetString(keydata);
				}
			}
		}

		public Hashtable KeysTable
		{
			set
			{
				Hashtable table = new Hashtable(value.Count);
				IDictionaryEnumerator ie = value.GetEnumerator();
				while(ie.MoveNext())
					table.Add(ie.Value, ie.Key);
				foreach(ListViewItem item in listKeys.Items)
					if (item.Tag != null)
					{
						KeyInfo info = (KeyInfo)item.Tag;
						if (table.ContainsKey(info.name))
							info.KeyData = (Keys)table[info.name];
						else
							info.KeyData = Keys.None;
					}
			}
			get
			{
				Hashtable table = new Hashtable();
				foreach(ListViewItem item in listKeys.Items)
					if (item.Tag != null)
					{
						KeyInfo info = (KeyInfo)item.Tag;
						if (info.KeyData != Keys.None)
							table.Add(info.KeyData, info.name);
					}
				return table;
			}
		}
				
		private void listKeys_ItemActivate(object sender, EventArgs e)
		{
			ListViewItem item = listKeys.SelectedItems[0];
			if (item.Tag != null)
			{
				KeyForm.KeyData = Keys.None;
				KeyForm.TopMost = TopMost;
				KeyForm.ShowDialog();
				KeyInfo info = (KeyInfo)item.Tag;
				if (info.KeyData != KeyForm.KeyData)
				{
					foreach(ListViewItem i in listKeys.Items)
						if (i.Index != item.Index && i.Tag != null && 
							((KeyInfo)i.Tag).KeyData == KeyForm.KeyData)
							((KeyInfo)i.Tag).KeyData = Keys.None;
					info.KeyData = KeyForm.KeyData;
					updateControls(true);
				}
			}
		}
		
		private void listKeys_SelectedIndexChanged(object sender, EventArgs e)
		{
			updateControls(btnApply.Enabled);
		}

		private void btnKeysClear_Click(object sender, System.EventArgs e)
		{
			((KeyInfo)listKeys.SelectedItems[0].Tag).KeyData = Keys.None;
			updateControls(true);
		}

		private void btnKeysClearAll_Click(object sender, System.EventArgs e)
		{
			KeysTable = new Hashtable();
			updateControls(true);
		}

		private void btnKeysDefault_Click(object sender, System.EventArgs e)
		{
			KeysTable = htDefault;
			updateControls(true);
		}
		
		static SettingsForm()
		{
			htDefault = new Hashtable(40);
			htDefault.Add(Keys.Control | Keys.O, strKeysFileOpen);
			htDefault.Add(Keys.Control | Keys.X, strKeysFileClose);
			htDefault.Add(Keys.I, strKeysFileInfo);
			htDefault.Add(Keys.P, strKeysPlay);
			htDefault.Add(Keys.S, strKeysStop);
			htDefault.Add(Keys.Space, strKeysPause);
			htDefault.Add(Keys.D1, strKeys50);
			htDefault.Add(Keys.D2, strKeys100);
			htDefault.Add(Keys.D3, strKeys200);
			htDefault.Add(Keys.D4, strKeysFree);
			htDefault.Add(Keys.F, strKeysFullscreen);
			htDefault.Add(Keys.R, strKeysRepeat);
			htDefault.Add(Keys.M, strKeysMute);
			htDefault.Add(Keys.Up, strKeysUp);
			htDefault.Add(Keys.Down, strKeysDown);
			htDefault.Add(Keys.Left, strKeysBack);
			htDefault.Add(Keys.Right, strKeysForth);
			htDefault.Add(Keys.A, strKeysAbout);
			htDefault.Add(Keys.Escape, strKeysExit);
			htDefault.Add(Keys.Alt | Keys.D1, strKeysAspectOriginal);
			htDefault.Add(Keys.Alt | Keys.D2, strKeysAspect4_3);
			htDefault.Add(Keys.Alt | Keys.D3, strKeysAspect16_9);
			htDefault.Add(Keys.Alt | Keys.D4, strKeysAspect47_20);
			htDefault.Add(Keys.Alt | Keys.D5, strKeysAspectFree);
			htDefault.Add(Keys.F2, strKeysPref);
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			updateControls(false);
			nSysTray = SystemTray;

			float w = listKeys.Columns[0].Width;
            w *= AutoScaleDimensions.Width / sizefOrigAutoScaleDimensions.Width;
			listKeys.Columns[0].Width = (int)w;
			listKeys.Columns[1].Width = -2;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed (e);
			if ((nSysTray == 2 && SystemTray != 2) || (nSysTray != 2 && SystemTray == 2))
				bRestartTriggered = true;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.grpSysTray = new System.Windows.Forms.GroupBox();
            this.radioSysTrayOnly = new System.Windows.Forms.RadioButton();
            this.chkShowTrayAlways = new System.Windows.Forms.CheckBox();
            this.radioMinToTray = new System.Windows.Forms.RadioButton();
            this.radioTaskbarOnly = new System.Windows.Forms.RadioButton();
            this.grpPlayerOptions = new System.Windows.Forms.GroupBox();
            this.chkOnTop = new System.Windows.Forms.CheckBox();
            this.chkLogo = new System.Windows.Forms.CheckBox();
            this.chkCenter = new System.Windows.Forms.CheckBox();
            this.chkVolume = new System.Windows.Forms.CheckBox();
            this.chkAutoPlay = new System.Windows.Forms.CheckBox();
            this.chkFullscreen = new System.Windows.Forms.CheckBox();
            this.grpVMRMode = new System.Windows.Forms.GroupBox();
            this.radioWindowed = new System.Windows.Forms.RadioButton();
            this.radioWindowless = new System.Windows.Forms.RadioButton();
            this.grpRenderer = new System.Windows.Forms.GroupBox();
            this.radioEVR = new System.Windows.Forms.RadioButton();
            this.radioVMR9 = new System.Windows.Forms.RadioButton();
            this.radioVMR = new System.Windows.Forms.RadioButton();
            this.radioVR = new System.Windows.Forms.RadioButton();
            this.btnRemoveAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.listTypes = new System.Windows.Forms.ListView();
            this.columnExtensions = new System.Windows.Forms.ColumnHeader();
            this.labelTypes = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageGeneral = new System.Windows.Forms.TabPage();
            this.pageVideo = new System.Windows.Forms.TabPage();
            this.btnRecommendedRenderer = new System.Windows.Forms.Button();
            this.pagePrefFilters = new System.Windows.Forms.TabPage();
            this.panelFilters = new System.Windows.Forms.Panel();
            this.grpTechnique = new System.Windows.Forms.GroupBox();
            this.chkDVDGraphs = new System.Windows.Forms.CheckBox();
            this.chkRegularGraphs = new System.Windows.Forms.CheckBox();
            this.pageKM = new System.Windows.Forms.TabPage();
            this.btnKeysDefault = new System.Windows.Forms.Button();
            this.btnKeysClearAll = new System.Windows.Forms.Button();
            this.btnKeysClear = new System.Windows.Forms.Button();
            this.listKeys = new System.Windows.Forms.ListView();
            this.columnAction = new System.Windows.Forms.ColumnHeader();
            this.columnKeys = new System.Windows.Forms.ColumnHeader();
            this.groupMouseWheel = new System.Windows.Forms.GroupBox();
            this.radioSeek = new System.Windows.Forms.RadioButton();
            this.radioVolume = new System.Windows.Forms.RadioButton();
            this.pageFileTypes = new System.Windows.Forms.TabPage();
            this.grpSysTray.SuspendLayout();
            this.grpPlayerOptions.SuspendLayout();
            this.grpVMRMode.SuspendLayout();
            this.grpRenderer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pageGeneral.SuspendLayout();
            this.pageVideo.SuspendLayout();
            this.pagePrefFilters.SuspendLayout();
            this.grpTechnique.SuspendLayout();
            this.pageKM.SuspendLayout();
            this.groupMouseWheel.SuspendLayout();
            this.pageFileTypes.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSysTray
            // 
            this.grpSysTray.Controls.Add(this.radioSysTrayOnly);
            this.grpSysTray.Controls.Add(this.chkShowTrayAlways);
            this.grpSysTray.Controls.Add(this.radioMinToTray);
            this.grpSysTray.Controls.Add(this.radioTaskbarOnly);
            resources.ApplyResources(this.grpSysTray, "grpSysTray");
            this.grpSysTray.Name = "grpSysTray";
            this.grpSysTray.TabStop = false;
            // 
            // radioSysTrayOnly
            // 
            resources.ApplyResources(this.radioSysTrayOnly, "radioSysTrayOnly");
            this.radioSysTrayOnly.Name = "radioSysTrayOnly";
            this.radioSysTrayOnly.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkShowTrayAlways
            // 
            resources.ApplyResources(this.chkShowTrayAlways, "chkShowTrayAlways");
            this.chkShowTrayAlways.Name = "chkShowTrayAlways";
            this.chkShowTrayAlways.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioMinToTray
            // 
            resources.ApplyResources(this.radioMinToTray, "radioMinToTray");
            this.radioMinToTray.Name = "radioMinToTray";
            this.radioMinToTray.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioTaskbarOnly
            // 
            this.radioTaskbarOnly.Checked = true;
            resources.ApplyResources(this.radioTaskbarOnly, "radioTaskbarOnly");
            this.radioTaskbarOnly.Name = "radioTaskbarOnly";
            this.radioTaskbarOnly.TabStop = true;
            this.radioTaskbarOnly.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // grpPlayerOptions
            // 
            this.grpPlayerOptions.Controls.Add(this.chkOnTop);
            this.grpPlayerOptions.Controls.Add(this.chkLogo);
            this.grpPlayerOptions.Controls.Add(this.chkCenter);
            this.grpPlayerOptions.Controls.Add(this.chkVolume);
            this.grpPlayerOptions.Controls.Add(this.chkAutoPlay);
            this.grpPlayerOptions.Controls.Add(this.chkFullscreen);
            resources.ApplyResources(this.grpPlayerOptions, "grpPlayerOptions");
            this.grpPlayerOptions.Name = "grpPlayerOptions";
            this.grpPlayerOptions.TabStop = false;
            // 
            // chkOnTop
            // 
            resources.ApplyResources(this.chkOnTop, "chkOnTop");
            this.chkOnTop.Name = "chkOnTop";
            this.chkOnTop.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkLogo
            // 
            resources.ApplyResources(this.chkLogo, "chkLogo");
            this.chkLogo.Name = "chkLogo";
            this.chkLogo.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkCenter
            // 
            resources.ApplyResources(this.chkCenter, "chkCenter");
            this.chkCenter.Name = "chkCenter";
            this.chkCenter.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkVolume
            // 
            resources.ApplyResources(this.chkVolume, "chkVolume");
            this.chkVolume.Name = "chkVolume";
            this.chkVolume.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkAutoPlay
            // 
            resources.ApplyResources(this.chkAutoPlay, "chkAutoPlay");
            this.chkAutoPlay.Name = "chkAutoPlay";
            this.chkAutoPlay.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkFullscreen
            // 
            resources.ApplyResources(this.chkFullscreen, "chkFullscreen");
            this.chkFullscreen.Name = "chkFullscreen";
            this.chkFullscreen.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // grpVMRMode
            // 
            this.grpVMRMode.Controls.Add(this.radioWindowed);
            this.grpVMRMode.Controls.Add(this.radioWindowless);
            resources.ApplyResources(this.grpVMRMode, "grpVMRMode");
            this.grpVMRMode.Name = "grpVMRMode";
            this.grpVMRMode.TabStop = false;
            // 
            // radioWindowed
            // 
            resources.ApplyResources(this.radioWindowed, "radioWindowed");
            this.radioWindowed.Name = "radioWindowed";
            this.radioWindowed.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioWindowless
            // 
            this.radioWindowless.Checked = true;
            resources.ApplyResources(this.radioWindowless, "radioWindowless");
            this.radioWindowless.Name = "radioWindowless";
            this.radioWindowless.TabStop = true;
            this.radioWindowless.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // grpRenderer
            // 
            this.grpRenderer.Controls.Add(this.radioEVR);
            this.grpRenderer.Controls.Add(this.radioVMR9);
            this.grpRenderer.Controls.Add(this.radioVMR);
            this.grpRenderer.Controls.Add(this.radioVR);
            resources.ApplyResources(this.grpRenderer, "grpRenderer");
            this.grpRenderer.Name = "grpRenderer";
            this.grpRenderer.TabStop = false;
            // 
            // radioEVR
            // 
            resources.ApplyResources(this.radioEVR, "radioEVR");
            this.radioEVR.Name = "radioEVR";
            this.radioEVR.TabStop = true;
            this.radioEVR.UseVisualStyleBackColor = true;
            this.radioEVR.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioVMR9
            // 
            resources.ApplyResources(this.radioVMR9, "radioVMR9");
            this.radioVMR9.Name = "radioVMR9";
            this.radioVMR9.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioVMR
            // 
            resources.ApplyResources(this.radioVMR, "radioVMR");
            this.radioVMR.Name = "radioVMR";
            this.radioVMR.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioVR
            // 
            this.radioVR.Checked = true;
            resources.ApplyResources(this.radioVR, "radioVR");
            this.radioVR.Name = "radioVR";
            this.radioVR.TabStop = true;
            this.radioVR.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // btnRemoveAll
            // 
            resources.ApplyResources(this.btnRemoveAll, "btnRemoveAll");
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Click += new System.EventHandler(this.OnFileTypesBtnClick);
            // 
            // btnSelectAll
            // 
            resources.ApplyResources(this.btnSelectAll, "btnSelectAll");
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Click += new System.EventHandler(this.OnFileTypesBtnClick);
            // 
            // listTypes
            // 
            this.listTypes.CheckBoxes = true;
            this.listTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnExtensions});
            this.listTypes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            resources.ApplyResources(this.listTypes, "listTypes");
            this.listTypes.Name = "listTypes";
            this.listTypes.UseCompatibleStateImageBehavior = false;
            this.listTypes.View = System.Windows.Forms.View.Details;
            this.listTypes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnFileTypesItemCheck);
            // 
            // columnExtensions
            // 
            resources.ApplyResources(this.columnExtensions, "columnExtensions");
            // 
            // labelTypes
            // 
            resources.ApplyResources(this.labelTypes, "labelTypes");
            this.labelTypes.Name = "labelTypes";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.Click += new System.EventHandler(this.OnClickApply);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageGeneral);
            this.tabControl.Controls.Add(this.pageVideo);
            this.tabControl.Controls.Add(this.pagePrefFilters);
            this.tabControl.Controls.Add(this.pageKM);
            this.tabControl.Controls.Add(this.pageFileTypes);
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // pageGeneral
            // 
            this.pageGeneral.Controls.Add(this.grpPlayerOptions);
            this.pageGeneral.Controls.Add(this.grpSysTray);
            resources.ApplyResources(this.pageGeneral, "pageGeneral");
            this.pageGeneral.Name = "pageGeneral";
            this.pageGeneral.UseVisualStyleBackColor = true;
            // 
            // pageVideo
            // 
            this.pageVideo.Controls.Add(this.btnRecommendedRenderer);
            this.pageVideo.Controls.Add(this.grpRenderer);
            this.pageVideo.Controls.Add(this.grpVMRMode);
            resources.ApplyResources(this.pageVideo, "pageVideo");
            this.pageVideo.Name = "pageVideo";
            this.pageVideo.UseVisualStyleBackColor = true;
            // 
            // btnRecommendedRenderer
            // 
            resources.ApplyResources(this.btnRecommendedRenderer, "btnRecommendedRenderer");
            this.btnRecommendedRenderer.Name = "btnRecommendedRenderer";
            this.btnRecommendedRenderer.UseVisualStyleBackColor = true;
            this.btnRecommendedRenderer.Click += new System.EventHandler(this.btnRecommendedRenderer_Click);
            // 
            // pagePrefFilters
            // 
            this.pagePrefFilters.Controls.Add(this.panelFilters);
            this.pagePrefFilters.Controls.Add(this.grpTechnique);
            resources.ApplyResources(this.pagePrefFilters, "pagePrefFilters");
            this.pagePrefFilters.Name = "pagePrefFilters";
            this.pagePrefFilters.UseVisualStyleBackColor = true;
            // 
            // panelFilters
            // 
            resources.ApplyResources(this.panelFilters, "panelFilters");
            this.panelFilters.BackColor = System.Drawing.SystemColors.Window;
            this.panelFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFilters.ForeColor = System.Drawing.SystemColors.WindowText;
            this.panelFilters.Name = "panelFilters";
            // 
            // grpTechnique
            // 
            this.grpTechnique.Controls.Add(this.chkDVDGraphs);
            this.grpTechnique.Controls.Add(this.chkRegularGraphs);
            resources.ApplyResources(this.grpTechnique, "grpTechnique");
            this.grpTechnique.Name = "grpTechnique";
            this.grpTechnique.TabStop = false;
            // 
            // chkDVDGraphs
            // 
            resources.ApplyResources(this.chkDVDGraphs, "chkDVDGraphs");
            this.chkDVDGraphs.Name = "chkDVDGraphs";
            this.chkDVDGraphs.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // chkRegularGraphs
            // 
            resources.ApplyResources(this.chkRegularGraphs, "chkRegularGraphs");
            this.chkRegularGraphs.Name = "chkRegularGraphs";
            this.chkRegularGraphs.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // pageKM
            // 
            this.pageKM.Controls.Add(this.btnKeysDefault);
            this.pageKM.Controls.Add(this.btnKeysClearAll);
            this.pageKM.Controls.Add(this.btnKeysClear);
            this.pageKM.Controls.Add(this.listKeys);
            this.pageKM.Controls.Add(this.groupMouseWheel);
            resources.ApplyResources(this.pageKM, "pageKM");
            this.pageKM.Name = "pageKM";
            this.pageKM.UseVisualStyleBackColor = true;
            // 
            // btnKeysDefault
            // 
            resources.ApplyResources(this.btnKeysDefault, "btnKeysDefault");
            this.btnKeysDefault.Name = "btnKeysDefault";
            this.btnKeysDefault.Click += new System.EventHandler(this.btnKeysDefault_Click);
            // 
            // btnKeysClearAll
            // 
            resources.ApplyResources(this.btnKeysClearAll, "btnKeysClearAll");
            this.btnKeysClearAll.Name = "btnKeysClearAll";
            this.btnKeysClearAll.Click += new System.EventHandler(this.btnKeysClearAll_Click);
            // 
            // btnKeysClear
            // 
            resources.ApplyResources(this.btnKeysClear, "btnKeysClear");
            this.btnKeysClear.Name = "btnKeysClear";
            this.btnKeysClear.Click += new System.EventHandler(this.btnKeysClear_Click);
            // 
            // listKeys
            // 
            this.listKeys.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAction,
            this.columnKeys});
            this.listKeys.FullRowSelect = true;
            this.listKeys.GridLines = true;
            this.listKeys.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listKeys.HideSelection = false;
            resources.ApplyResources(this.listKeys, "listKeys");
            this.listKeys.MultiSelect = false;
            this.listKeys.Name = "listKeys";
            this.listKeys.UseCompatibleStateImageBehavior = false;
            this.listKeys.View = System.Windows.Forms.View.Details;
            // 
            // columnAction
            // 
            resources.ApplyResources(this.columnAction, "columnAction");
            // 
            // columnKeys
            // 
            resources.ApplyResources(this.columnKeys, "columnKeys");
            // 
            // groupMouseWheel
            // 
            this.groupMouseWheel.Controls.Add(this.radioSeek);
            this.groupMouseWheel.Controls.Add(this.radioVolume);
            resources.ApplyResources(this.groupMouseWheel, "groupMouseWheel");
            this.groupMouseWheel.Name = "groupMouseWheel";
            this.groupMouseWheel.TabStop = false;
            // 
            // radioSeek
            // 
            resources.ApplyResources(this.radioSeek, "radioSeek");
            this.radioSeek.Name = "radioSeek";
            this.radioSeek.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // radioVolume
            // 
            resources.ApplyResources(this.radioVolume, "radioVolume");
            this.radioVolume.Name = "radioVolume";
            this.radioVolume.CheckedChanged += new System.EventHandler(this.OnCheckChanged);
            // 
            // pageFileTypes
            // 
            this.pageFileTypes.Controls.Add(this.labelTypes);
            this.pageFileTypes.Controls.Add(this.listTypes);
            this.pageFileTypes.Controls.Add(this.btnRemoveAll);
            this.pageFileTypes.Controls.Add(this.btnSelectAll);
            resources.ApplyResources(this.pageFileTypes, "pageFileTypes");
            this.pageFileTypes.Name = "pageFileTypes";
            this.pageFileTypes.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.grpSysTray.ResumeLayout(false);
            this.grpPlayerOptions.ResumeLayout(false);
            this.grpVMRMode.ResumeLayout(false);
            this.grpRenderer.ResumeLayout(false);
            this.grpRenderer.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.pageGeneral.ResumeLayout(false);
            this.pageVideo.ResumeLayout(false);
            this.pagePrefFilters.ResumeLayout(false);
            this.grpTechnique.ResumeLayout(false);
            this.pageKM.ResumeLayout(false);
            this.groupMouseWheel.ResumeLayout(false);
            this.pageFileTypes.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void OnClickApply(object sender, System.EventArgs e)
		{
			if ((nSysTray == 2 && SystemTray != 2) || (nSysTray != 2 && SystemTray == 2))
				bRestartTriggered = true;
			btnApply.Enabled = false;
			if (Apply != null)
				Apply(this, new EventArgs());
		}

		private void updateControls(bool bEnableBtnApply)
		{
			btnApply.Enabled = bEnableBtnApply;
			chkShowTrayAlways.Enabled = radioMinToTray.Checked;
			radioWindowless.Enabled = radioWindowed.Enabled = !radioVR.Checked && !radioEVR.Checked;

			btnKeysClear.Enabled = listKeys.SelectedItems.Count > 0 && 
									listKeys.SelectedItems[0].Tag != null;
			Hashtable table = KeysTable;
			btnKeysClearAll.Enabled = table.Count > 0;
			bool bEnable;
			if (table.Count == htDefault.Count)
			{
				bEnable = false;
				foreach(DictionaryEntry entry in htDefault)
				{
					if (table.ContainsKey(entry.Key) && 
						(string)table[entry.Key] == (string)entry.Value)
						continue;
					else
					{
						bEnable = true;
						break;
					}
				}
			}
			else
				bEnable = true;
			btnKeysDefault.Enabled = bEnable;
		}

		private void OnCheckChanged(object sender, System.EventArgs e)
		{
			updateControls(true);
		}

		private void OnFileTypesItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (count == 0)
			{
				bFileTypesChanged = true;
				updateControls(true);
			}
			else
				count--;
		}

		private void OnFileTypesBtnClick(object sender, System.EventArgs e)
		{
			Button btn = (Button) sender;
			foreach (ListViewItem item in listTypes.Items)
				item.Checked = (sender == btnSelectAll) ? true : false;			
		}
		
		public bool RestartTriggered
		{
			get { return bRestartTriggered; }
		}

		public string[] FileTypes
		{
			get
			{
				string[] astr = new string[listTypes.Items.Count];
				int i = 0;
				foreach (ListViewItem item in listTypes.Items)
				{
					astr[i] = (string) item.Tag;
					i++;
				}

				return astr;
			}
		}

		public Hashtable SelectedFileTypes
		{
			get
			{
				Hashtable table = new Hashtable();
				foreach (ListViewItem item in listTypes.Items)
					table[item.Tag] = item.Checked;
							
				return table;
			}
			set
			{
				foreach (ListViewItem item in listTypes.Items)
					if ((bool)value[item.Tag])
						count++;
				foreach (ListViewItem item in listTypes.Items)
					item.Checked = (bool)value[item.Tag];
			}
		}

		public bool FileTypesChanged
		{
			get { return bFileTypesChanged; }
		}

		#region General tab properties
		public bool StartFullscreen
		{
			set { chkFullscreen.Checked = value; }
			get { return chkFullscreen.Checked; }
		}

		public bool AutoPlay
		{
			set { chkAutoPlay.Checked = value; }
			get { return chkAutoPlay.Checked; }
		}

		public bool RememberVolume
		{
			set { chkVolume.Checked = value; }
			get { return chkVolume.Checked; }
		}

		public bool CenterWindow
		{
			set { chkCenter.Checked = value; }
			get { return chkCenter.Checked; }
		}

		public bool ShowLogo
		{
			set { chkLogo.Checked = value; }
			get { return chkLogo.Checked; }
		}

		public bool AlwaysOnTop
		{
			set { chkOnTop.Checked = value; }
			get { return chkOnTop.Checked; }
		}

		public int SystemTray
		{
			set
			{
				switch(value)
				{
					case 1: radioMinToTray.Checked = true; break;
					case 2: radioSysTrayOnly.Checked = true; break;
					default: radioTaskbarOnly.Checked = true; break;
				}
			}
			get
			{
				if (radioSysTrayOnly.Checked)
					return 2;
				else if (radioMinToTray.Checked)
					return 1;
				else
					return 0;
			}
		}

		public bool ShowTrayAlways
		{
			get { return chkShowTrayAlways.Checked; }
			set { chkShowTrayAlways.Checked = value; }
		}
		#endregion

		public Renderer VideoRenderer
		{
			set
			{
				switch(value)
				{
					case Renderer.VMR_Windowless: radioVMR.Checked = radioWindowless.Checked = true; break;
					case Renderer.VMR_Windowed: radioVMR.Checked = radioWindowed.Checked = true; break;
					case Renderer.VMR9_Windowless: radioVMR9.Checked = radioWindowless.Checked = true; break;
					case Renderer.VMR9_Windowed: radioVMR9.Checked = radioWindowed.Checked = true; break;
                    case Renderer.EVR: radioEVR.Checked = true; break;
					default: radioVR.Checked = true; break;
				}
			}
			get
			{
                if (radioVMR.Checked)
                    return radioWindowless.Checked ? Renderer.VMR_Windowless : Renderer.VMR_Windowed;
                else if (radioVMR9.Checked)
                    return radioWindowless.Checked ? Renderer.VMR9_Windowless : Renderer.VMR9_Windowed;
                else if (radioEVR.Checked)
                    return Renderer.EVR;
                else
                    return Renderer.VR;
			}
		}

		public MouseWheelAction MouseWheelAction
		{
			get
			{
				return radioVolume.Checked ? MouseWheelAction.Volume : MouseWheelAction.Seek;
			}
			set
			{
				if (value == MouseWheelAction.Volume)
					radioVolume.Checked = true;
				else
					radioSeek.Checked = true;
			}
		}

		public bool UsePreferredFilters
		{
			get { return chkRegularGraphs.Checked; }
			set { chkRegularGraphs.Checked = value; }
		}

		public bool UsePreferredFilters4DVD
		{
			get { return chkDVDGraphs.Checked; }
			set { chkDVDGraphs.Checked = value; }
		}

        private void btnRecommendedRenderer_Click(object sender, EventArgs e)
        {
            switch (MediaWindow.RecommendedRenderer)
            {
                case Renderer.EVR:
                    radioEVR.Checked = true;
                    break;
                case Renderer.VMR_Windowless:
                    radioVMR.Checked = radioWindowless.Checked = true;
                    break;
                default:
                    radioVR.Checked = true;
                    break;
            }
        }

        private void SetRenderers()
        {
            IList<Renderer> renderers = MediaWindow.PresentRenderers;
            radioEVR.Enabled = renderers.Contains(Renderer.EVR);
            radioVMR.Enabled = renderers.Contains(Renderer.VMR_Windowed);
            radioVMR9.Enabled = renderers.Contains(Renderer.VMR9_Windowed);
            radioVR.Enabled = renderers.Contains(Renderer.VR);
        }
	}

}
