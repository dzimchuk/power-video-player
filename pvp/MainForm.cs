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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Dzimchuk.Common;
using Pvp.Core.Native;
using System.Runtime.InteropServices;
using Dzimchuk.AUI;
using Pvp.Util;

namespace Pvp
{
    /// <summary>
    /// The MainForm. This class contains the entry point.
    /// </summary>
    public class MainForm : MainFormSettings
    {
        static Guid guid = new Guid("{232FF40A-67C4-4a5d-B82C-6CF3C6053110}");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) 
        {
            if (Array.Find(args,
                            delegate(string arg)
                            {
                                string s = arg.ToLowerInvariant();
                                return s == "-regapp" || s == "/regapp";
                            }
                           ) != default(string))
            {
                HandleRegApp();
                return;
            }
            else if (Array.Find(args,
                                delegate(string arg)
                                {
                                    string s = arg.ToLowerInvariant();
                                    return s == "-unregapp" || s == "/unregapp";
                                }
                ) != default(string))
            {
                HandleUnRegApp();
                return;
            }
            
            SingleInstance si = SingleInstance.CreateSingleInstance(guid);
            si.ArgsRecieved += new ArgsHandler(OnNewCommandLineArgs);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            si.Run(typeof(MainForm));
        }
        
        public MainForm()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            Application.Idle += new EventHandler(OnIdle);
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) // astr[1] may contain the file name to play
            {
                strFileName = args[1];
                whatToPlay = Pvp.Core.MediaEngine.MediaSourceType.File;
                bPlayPending = true;
            }
        }

        private static void OnNewCommandLineArgs(Form form, string[] args)
        {
            if (args.Length > 1)
            {
                MainForm me = (MainForm) form;
                me.strFileName = args[1];
                me.whatToPlay = Pvp.Core.MediaEngine.MediaSourceType.File;
                me.bPlayPending = true;
            }
        }

        bool bSecondPending;
        private void OnIdle(object sender, EventArgs e)
        {
            if (bPlayPending)
            {
                if (!bSecondPending)
                {
                    bSecondPending = true;
                    nicon.Restore();
                }
                else
                {
                    PlayIt(strFileName, whatToPlay);
                    bPlayPending = false;
                    bSecondPending = false;
                }
            }
        }

        private void PerformPopup(Menu popup, IntPtr hMenu, ref bool bFound)
        {
            if (hMenu == popup.Handle && !(popup is ContextMenu))
            {
                // no need to handle for ContextMenu
                ((MenuItemEx)popup).PerformPopup();
                bFound = true;
                return;
            }

            MenuItem item;
            int count = popup.MenuItems.Count;
            for(int i=0; i<count; i++)
            {
                if(bFound)
                    break;
                item = popup.MenuItems[i];
                if(item.IsParent)
                    PerformPopup(item, hMenu, ref bFound);
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool bFound = false;
            if (m.Msg == (int)WindowsMessages.WM_INITMENUPOPUP)
            {
                PerformPopup(contextMenu, m.WParam, ref bFound);
                if (bFound)
                    m.Result = IntPtr.Zero;
            }
            
            if (!bFound)
                base.WndProc (ref m);
        }

        public static void HandleRegApp()
        {
            if (IsAdmin)
            {
                CultureInfo ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                string strExe = Assembly.GetExecutingAssembly().Location;
                string command = "\"" + strExe + "\"";
                string icon = strExe + ",0";

                string appName = Resources.Resources.program_name;
                string appDescription = Resources.Resources.program_description;

                using (FileAssociator fa = FileAssociator.GetFileAssociator(strDocTypePrefix, strProgName))
                {
                    fa.Register(@"Software\Clients\Media", icon, command, appName, appDescription, GetTypesInfo());
                }
            }
            else
            {
                Elevate("-regapp");
            }
        }

        public static void HandleUnRegApp()
        {
            if (IsAdmin)
            {
                using (FileAssociator fa = FileAssociator.GetFileAssociator(strDocTypePrefix, strProgName))
                {
                    fa.Unregister();
                }
            }
            else
            {
                Elevate("-unregapp");
            }
        }

        private static void Elevate(string command)
        {
            // do not elevate if it's already being elevated, i.e. if -elevate is specified we won't try to run another process (it shouldn't happen but an extra protection won't harm)
            if (Array.Find(Environment.GetCommandLineArgs(), delegate(string arg)
                                                                {
                                                                    string s = arg.ToLowerInvariant();
                                                                    return s == "-elevate" || s == "/elevate";
                                                                }
                ) == default(string)) // no -elevate option is found
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Assembly.GetExecutingAssembly().Location;
                startInfo.Verb = "runas";
                startInfo.Arguments = "-elevate " + command;
                try
                {
                    Process p = Process.Start(startInfo);
                    p.WaitForExit();
                }
                catch // System.ComponentModel.Win32Exception is thrown if the user refused to run the application with admin permissions; but we also guard against other errors
                {
                    // just exit if there is any problem or the user denied the elevation
                }
            }
        }

        private static IList<FileAssociator.DocTypeInfo> GetTypesInfo()
        {
            string strExe = Assembly.GetExecutingAssembly().Location;
            string command = "\"" + strExe + "\" \"%L\"";
            string icon = strExe + ",0";
            
            IList<FileAssociator.DocTypeInfo> types = new List<FileAssociator.DocTypeInfo>();

            IList<FileType> ts = Pvp.FileTypes.GetFileTypes();
            foreach (FileType t in ts)
                types.Add(new FileAssociator.DocTypeInfo(t.Extension, t.Description.Substring(t.Description.IndexOf('-') + 2), icon, command, command, true));

            return types;
        }

        private static bool IsAdmin
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
