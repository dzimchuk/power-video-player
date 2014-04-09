using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Pvp.Core.Native;

namespace Pvp.App.Util.FileTypes
{
    internal class DefaultProgramsFileAssociator : IFileAssociator
    {
        private readonly Lazy<Shell.IApplicationAssociationRegistration> _pAppAssocReg = 
            new Lazy<Shell.IApplicationAssociationRegistration>(GetInstance<Shell.IApplicationAssociationRegistration>, true);
        private readonly Lazy<Shell.IApplicationAssociationRegistrationUI> _pAppAssocRegUi =
            new Lazy<Shell.IApplicationAssociationRegistrationUI>(GetInstance<Shell.IApplicationAssociationRegistrationUI>, true);

        private readonly string _docTypePrefix;
        private readonly string _appName; // canonical app name

        private const int S_OK = 0;
        private IDictionary<string, string> _dictProgIdApps;
        private IDictionary<string, string> _dictExtApps;

        public DefaultProgramsFileAssociator(string docTypePrefix, string appName)
        {
            _appName = appName;
            _docTypePrefix = docTypePrefix;
        }

        private static T GetInstance<T>()
        {
            Guid clsid;
            if (typeof(T) == typeof(Shell.IApplicationAssociationRegistration))
                clsid = Shell.CLSID_ApplicationAssociationRegistration;
            else if (typeof(T) == typeof(Shell.IApplicationAssociationRegistrationUI))
                clsid = Shell.CLSID_ApplicationAssociationRegistrationUI;
            else
                throw new Exception();

            object comobj = null;
            try
            {
                var type = Type.GetTypeFromCLSID(clsid, true);
                comobj = Activator.CreateInstance(type);
                return (T)comobj;
            }
            catch
            {
                if (comobj != null)
                {
                    Marshal.FinalReleaseComObject(comobj);
                }

                return default(T);
            }
        }

        public void Dispose()
        {
            if (_pAppAssocReg.IsValueCreated && _pAppAssocReg.Value != null)
            {
                Marshal.FinalReleaseComObject(_pAppAssocReg.Value);
            }

            if (_pAppAssocRegUi.IsValueCreated && _pAppAssocRegUi.Value != null)
            {
                Marshal.FinalReleaseComObject(_pAppAssocRegUi.Value);
            }
        }

        public bool CanShowExternalUI { get { return true; } }

        public string DocTypePrefix
        {
            get { return _docTypePrefix; }
        }

        public string AppName
        {
            get { return _appName; }
        }

        public virtual bool CanAssociate { get { return true; } }
        
        public virtual bool IsAssociated(string ext)
        {
            bool bAssoc;
            var hr = _pAppAssocReg.Value.QueryAppIsDefault(ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION, Shell.ASSOCIATIONLEVEL.AL_EFFECTIVE, _appName, out bAssoc);
            return hr == S_OK && bAssoc;
        }

        public virtual void Associate(string ext)
        {
            // determine the current default app and save it as PrevDefaultApp
            string prevProgId;
            var hr = _pAppAssocReg.Value.QueryCurrentDefault(ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION, Shell.ASSOCIATIONLEVEL.AL_EFFECTIVE, out prevProgId);
            if (hr == S_OK)
            {
                // once we have the ProgID we need to determine the application (from RegisteredApplications) because
                // we need application name in pAppAssocReg.SetAppAsDefault
                var prevApp = GetPreviousApp(prevProgId);
                if (prevApp != null && prevApp != _appName)
                {
                    // the app name should be stored in PrevDefaultApp
                    RegistryHelper.SetClassesValueUser(ext, "PrevDefaultApp", prevApp);
                }
            } // else IApplicationAssociationRegistration and 'Default Programs' don't leave much to do

            // but we will (try to) associate anyway
            _pAppAssocReg.Value.SetAppAsDefault(_appName, ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION);
        }

        public virtual void UnAssociate(string ext)
        {
            if (IsAssociated(ext))
            {
                var prevApp = SanatePreviousApp(RegistryHelper.GetClassesValueUser(ext, "PrevDefaultApp", null), ext);
                if (prevApp != null)
                {
                    _pAppAssocReg.Value.SetAppAsDefault(prevApp, ext, Shell.ASSOCIATIONTYPE.AT_FILEEXTENSION);
                } // else we failed to find previous app and another app to handle the extension
            }
        }

        public void NotifyShell()
        {
            Shell.SHChangeNotify(Shell.SHCNE_ASSOCCHANGED, Shell.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public bool ShowExternalUI()
        {
            return _pAppAssocRegUi.Value.LaunchAdvancedAssociationUI(_appName) == S_OK;
        }

        private string SanatePreviousApp(string prevApp, string ext)
        {
            // the idea is to determine if the previous app is still registered (then we can just use it)
            if (prevApp != null && RegistryHelper.GetValueLocal(@"Software\RegisteredApplications", prevApp, null) != null)
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
    }
}