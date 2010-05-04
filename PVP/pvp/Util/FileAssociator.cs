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
using Dzimchuk.Native;
using Microsoft.Win32;

namespace Dzimchuk.PVP.Util
{
	/// <summary>
	/// Summary description for FileAssociator.
	/// </summary>
	public class FileAssociator : IDisposable
	{
		private string _docTypePrefix;
        protected string _appName; // canonical app name
        private const string SOFTWARE_CLASSES = @"Software\Classes\{0}";

        public class DocTypeInfo
        {
            public string Extension;
            public string Description;
            public string DefaultIcon;
            public string OpenCommand;
            public string PlayCommand;
            public bool PlayCommandNeeded;

            public DocTypeInfo(string extension, string description, string defaultIcon, string openCommand, string playCommand, bool playCommandNeeded)
            {
                Extension = extension;
                Description = description;
                DefaultIcon = defaultIcon;
                OpenCommand = openCommand;
                PlayCommand = playCommand;
                PlayCommandNeeded = playCommandNeeded;
            }
        }

        public static FileAssociator GetFileAssociator(string docTypePrefix, string appName)
        {
            if (Environment.OSVersion.Version.Major >= 6)
                return FileAssociatorW6.GetFileAssociator(docTypePrefix, appName);
            else
                return new FileAssociator(docTypePrefix, appName);
        }

        protected FileAssociator(string docTypePrefix, string appName)
		{
			_docTypePrefix = docTypePrefix;
            _appName = appName;
		}

        public void Register(string regPath, 
                             string defaultIcon, 
                             string openCommand,
                             string localizedAppName, // can be in the form: @FilePath,-StringID
                             string localizedAppDescription, // can be in the from: @FilePath,-StringID
                             IList<DocTypeInfo> docTypes)
        {
            string appPath = String.Format(@"{0}\{1}", regPath, _appName);
            string capabilitiesPath = String.Format(@"{0}\Capabilities", appPath);
            string iconPath = String.Format(@"{0}\DefaultIcon", appPath);
            string commandPath = String.Format(@"{0}\shell\open\command", appPath);
            string fileAssocPath = String.Format(@"{0}\FileAssociations", capabilitiesPath);

            SetValueLocal(@"Software\RegisteredApplications", _appName, capabilitiesPath);
            SetValueLocal(appPath, null, _appName); // fallback display name, also a special 'LocalizedString' value can be added here in the form: @FilePath,-StringID
                                                    // (see MSDN) for 'Registering Programs with Client Types'
            SetValueLocal(iconPath, null, defaultIcon); // [path to exe OR dll],index
                                                        // do not enclose in double quotes even if the path contains spaces
            SetValueLocal(commandPath, null, openCommand); // enclose in double quotes
            SetValueLocal(capabilitiesPath, "ApplicationName", localizedAppName);
            SetValueLocal(capabilitiesPath, "ApplicationDescription", localizedAppDescription);

            foreach (DocTypeInfo dti in docTypes)
            {
                string docType = _docTypePrefix + dti.Extension.ToUpperInvariant();
                SetValueLocal(fileAssocPath, dti.Extension, docType);

                SetClassesValueLocal(docType, null, dti.Description);
                SetClassesValueLocal(docType + @"\DefaultIcon", null, dti.DefaultIcon);
                SetClassesValueLocal(docType + @"\shell\open\command", null, dti.OpenCommand);
                if (dti.PlayCommandNeeded)
                    SetClassesValueLocal(docType + @"\shell\play\command", null, dti.PlayCommand);
            }
        }

        public void Unregister()
        {
            string capabilitiesPath = GetValueLocal(@"Software\RegisteredApplications", _appName, null);
            if (capabilitiesPath != null)
            {
                string fileAssocPath = String.Format(@"{0}\FileAssociations", capabilitiesPath);

                string[] extensions;
                RegistryKey key = null;
                try
                {
                    key = Registry.LocalMachine.OpenSubKey(fileAssocPath);
                    if (key != null)
                    {
                        extensions = key.GetValueNames();
                    }
                    else throw new Exception();
                }
                catch
                {
                    return; // what else?
                }
                finally
                {
                    if (key != null)
                        key.Close();
                }

                foreach (string ext in extensions)
                {
                    if (ext != String.Empty) // check for an accident default value (which should never be the case but still)
                    {
                        Associate(ext, false);
                        
                        string docType = GetValueLocal(fileAssocPath, ext, null);
                        if (docType != null)
                        {
                            DeleteDocID(docType);
                        }
                    }
                }

                DeleteAppRegistration(capabilitiesPath);
            }
        }

        private void DeleteDocID(string docType)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"Software\Classes", true);
                if (key != null)
                    key.DeleteSubKeyTree(docType);
            }
            catch
            {
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }

        private void DeleteAppRegistration(string capabilitiesPath)
        {
            RegistryKey key = null;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"Software\RegisteredApplications", true);
                if (key != null)
                {
                    key.DeleteValue(_appName);
                    key.Close();
                    key = null;
                }

                if (capabilitiesPath.LastIndexOf('\\') != -1)
                {
                    string appPath = capabilitiesPath.Substring(0, capabilitiesPath.LastIndexOf('\\'));
                    if (appPath.EndsWith(_appName) && appPath.LastIndexOf('\\') != -1)
                    {
                        string parent = appPath.Substring(0, appPath.LastIndexOf('\\'));
                        key = Registry.LocalMachine.OpenSubKey(parent, true);
                        if (key != null)
                            key.DeleteSubKeyTree(_appName);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (key != null)
                    key.Close();
            }
        }

		public virtual bool IsAssociated(string ext)
		{
			string docType = GetClassesValueUser(ext, null, String.Empty);
            return (docType != null && docType == (_docTypePrefix + ext.ToUpperInvariant()));
		}

		public virtual void Associate(string ext, bool bAssociate)
		{
			if (bAssociate)
			{
                string docType = _docTypePrefix + ext.ToUpperInvariant();
                string prevDocType = GetClassesValueUser(ext, null, String.Empty);
                if (prevDocType != null && prevDocType != docType)
                    SetClassesValueUser(ext, "PrevDefault", prevDocType);
                SetClassesValueUser(ext, null, docType);
			}
			else if (IsAssociated(ext))
			{
                string prevDocType = GetClassesValueUser(ext, "PrevDefault", String.Empty);
                if (prevDocType != null)
                    SetClassesValueUser(ext, null, prevDocType);
				else
					SetClassesValueUser(ext, null, String.Empty);
			}
		}

		public static void NotifyShell()
		{
			Shell.SHChangeNotify(Shell.SHCNE_ASSOCCHANGED, Shell.SHCNF_IDLIST, 
				IntPtr.Zero, IntPtr.Zero);
		}

        protected void SetClassesValueUser(string strSubKey, string name, object val)
        {
            SetValueUser(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        protected void SetClassesValueLocal(string strSubKey, string name, object val)
        {
            SetValueLocal(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        protected void SetValueUser(string strSubKey, string name, object val)
        {
            SetValue(Registry.CurrentUser, strSubKey, name, val);
        }

        protected void SetValueLocal(string strSubKey, string name, object val)
        {
            SetValue(Registry.LocalMachine, strSubKey, name, val);
        }

        private void SetValue(RegistryKey parentKey, string strSubKey, string name, object val)
        {
            RegistryKey regkey = null;
            try
            {
                regkey = parentKey.OpenSubKey(strSubKey, true);
                if (regkey == null)
                    regkey = parentKey.CreateSubKey(strSubKey);

                regkey.SetValue(name, val);
            }
            catch
            {
            }
            finally
            {
                if (regkey != null)
                    regkey.Close();
            }
        }

        protected string GetClassesValueUser(string strSubKey, string name, object val)
        {
            return GetValueUser(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        protected string GetClassesValueLocal(string strSubKey, string name, object val)
        {
            return GetValueLocal(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        protected string GetValueUser(string strSubKey, string name, object val)
        {
            return GetValue(Registry.CurrentUser, strSubKey, name, val);
        }

        protected string GetValueLocal(string strSubKey, string name, object val)
        {
            return GetValue(Registry.LocalMachine, strSubKey, name, val);
        }

        private string GetValue(RegistryKey parentKey, string strSubKey, string name, object val)
        {
            string ret = null;
            RegistryKey regkey = null;
            try
            {
                regkey = parentKey.OpenSubKey(strSubKey);
                if (regkey != null)
                    ret = (string)regkey.GetValue(name, val);

                return ret;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (regkey != null)
                    regkey.Close();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        ~FileAssociator()
        {
            Dispose(false);
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (!_disposed)
                {
                    if (disposing) // release managed components if any
                    {
                        CleanUp();
                    }

                    // release unmanaged components if any
                    _disposed = true;
                }
            }
        }

        protected virtual void CleanUp()
        {
        }
    }
}
