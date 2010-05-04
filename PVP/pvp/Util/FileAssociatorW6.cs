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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Dzimchuk.Native;

namespace Dzimchuk.PVP.Util
{
    public class FileAssociatorW6 : FileAssociator
    {
        private Shell.IApplicationAssociationRegistration pAppAssocReg = null;
        private const int S_OK = 0;
        private IDictionary<string, string> _dictProgIdApps = null;
        private IDictionary<string, string> _dictExtApps = null;
        
        public static new FileAssociator GetFileAssociator(string docTypePrefix, string appName)
        {
            return new FileAssociatorW6(docTypePrefix, appName);
        }
        
        private FileAssociatorW6(string docTypePrefix, string appName) : base(docTypePrefix, appName)
        {
            object comobj = null;
            try
            {
                Type type = Type.GetTypeFromCLSID(Shell.CLSID_ApplicationAssociationRegistration, true);
                comobj = Activator.CreateInstance(type);
                pAppAssocReg = (Shell.IApplicationAssociationRegistration)comobj;
            }
            catch
            {
                if (comobj != null)
                    Marshal.FinalReleaseComObject(comobj);
            }
        }

        public override bool IsAssociated(string ext)
        {
            if (pAppAssocReg != null)
            {
                bool bAssoc = false;
                int hr = pAppAssocReg.QueryAppIsDefault(ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION, Shell.ASSOCIATIONLEVEL.AL_EFFECTIVE, _appName, out bAssoc);
                return hr == S_OK && bAssoc;
            }
            else
                return false;
        }

        public override void Associate(string ext, bool bAssociate)
        {
            if (pAppAssocReg != null)
            {
                if (bAssociate)
                {
                    // determine the current default app and save it as PrevDefaultApp
                    string prevProgId = null;
                    int hr = pAppAssocReg.QueryCurrentDefault(ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION, Shell.ASSOCIATIONLEVEL.AL_EFFECTIVE, out prevProgId);
                    if (hr == S_OK)
                    {
                        // once we have the ProgID we need to determine the application (from RegisteredApplications) because
                        // we need application name in pAppAssocReg.SetAppAsDefault
                        string prevApp = GetPreviousApp(prevProgId);
                        if (prevApp != null && prevApp != _appName)
                        {
                            // the app name should be stored in PrevDefaultApp
                            SetClassesValueUser(ext, "PrevDefaultApp", prevApp);
                        }
                    } // else IApplicationAssociationRegistration and 'Default Programs' don't leave much to do
                    
                    // but we will (try to) associate anyway
                    pAppAssocReg.SetAppAsDefault(_appName, ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION);
                }
                else if (IsAssociated(ext))
                {
                    string prevApp = SanatePreviousApp(GetClassesValueUser(ext, "PrevDefaultApp", null), ext);
                    if (prevApp != null)
                    {
                        pAppAssocReg.SetAppAsDefault(prevApp, ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION);
                    } // else we failed to find previous app and another app to handle the extension
                }
            }
        }

        private string SanatePreviousApp(string prevApp, string ext)
        {
            // the idea is to determine if the previous app is still registered (then we can just use it)
            if (prevApp != null && GetValueLocal(@"Software\RegisteredApplications", prevApp, null) != null)
                return prevApp;

            // if not we will try to find another registered app that can handle the extension
            if (_dictExtApps == null)
                FillUpDictionaries();

            if (_dictExtApps.ContainsKey(ext))
                return _dictExtApps[ext];
            else
                return prevApp;
        }

        private string GetPreviousApp(string progId)
        {
            if (_dictProgIdApps == null)
                FillUpDictionaries();
            
            if (_dictProgIdApps.ContainsKey(progId))
            {
                return _dictProgIdApps[progId];
            }
            else
                return null;
        }

        private void FillUpDictionaries()
        {
            _dictExtApps = new Dictionary<string, string>();
            _dictProgIdApps = new Dictionary<string, string>();

            RegistryKey keyApps = null, keyFileAssoc = null;
            try
            {
                keyApps = Registry.LocalMachine.OpenSubKey(@"Software\RegisteredApplications");
                if (keyApps != null)
                {
                    string[] apps = keyApps.GetValueNames();
                    foreach (string app in apps)
                    {
                        if (app != String.Empty)
                        {
                            string fileAssocPath = String.Format(@"{0}\FileAssociations", keyApps.GetValue(app));
                            keyFileAssoc = Registry.LocalMachine.OpenSubKey(fileAssocPath); // If the specified subkey cannot be found, then null is returned.
                            if (keyFileAssoc != null)
                            {
                                string[] extensions = keyFileAssoc.GetValueNames();
                                foreach (string extension in extensions)
                                {
                                    if (extension != String.Empty)
                                    {
                                        if (!_dictExtApps.ContainsKey(extension.ToLowerInvariant()))
                                            _dictExtApps.Add(extension.ToLowerInvariant(), app);
                                        
                                        string id = (string)keyFileAssoc.GetValue(extension);
                                        if (!_dictProgIdApps.ContainsKey(id))
                                            _dictProgIdApps.Add(id, app);
                                    }
                                }

                                keyFileAssoc.Close();
                                keyFileAssoc = null;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (keyApps != null)
                    keyApps.Close();
                if (keyFileAssoc != null)
                    keyFileAssoc.Close();
            }
        }

        protected override void CleanUp()
        {
            if (pAppAssocReg != null)
            {
                Marshal.FinalReleaseComObject(pAppAssocReg);
                pAppAssocReg = null;
            }
            
            base.CleanUp();
        }
    }
}
