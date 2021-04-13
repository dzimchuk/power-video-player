using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Pvp.App.Util.FileTypes
{
    public class AppRegisterer : IDisposable
    {
        private readonly IFileAssociator _associator;

        public AppRegisterer(IFileAssociator associator)
        {
            _associator = associator;
        }

        public void Dispose()
        {
            if (_associator != null)
            {
                _associator.Dispose();
            }
        }

        public void Register(string regPath,
                             string defaultIcon,
                             string openCommand,
                             string localizedAppName, // can be in the form: @FilePath,-StringID
                             string localizedAppDescription, // can be in the from: @FilePath,-StringID
                             IEnumerable<DocTypeInfo> docTypes)
        {
            var appPath = String.Format(@"{0}\{1}", regPath, _associator.AppName);
            var capabilitiesPath = String.Format(@"{0}\Capabilities", appPath);
            var iconPath = String.Format(@"{0}\DefaultIcon", appPath);
            var commandPath = String.Format(@"{0}\shell\open\command", appPath);
            var fileAssocPath = String.Format(@"{0}\FileAssociations", capabilitiesPath);

            RegistryHelper.SetValueLocal(@"Software\RegisteredApplications", _associator.AppName, capabilitiesPath);
            RegistryHelper.SetValueLocal(appPath, null, _associator.AppName);
            // fallback display name, also a special 'LocalizedString' value can be added here in the form: @FilePath,-StringID
            // (see MSDN) for 'Registering Programs with Client Types'
            RegistryHelper.SetValueLocal(iconPath, null, defaultIcon); // [path to exe OR dll],index
            // do not enclose in double quotes even if the path contains spaces
            RegistryHelper.SetValueLocal(commandPath, null, openCommand); // enclose in double quotes
            RegistryHelper.SetValueLocal(capabilitiesPath, "ApplicationName", localizedAppName);
            RegistryHelper.SetValueLocal(capabilitiesPath, "ApplicationDescription", localizedAppDescription);

            foreach (var dti in docTypes)
            {
                var docType = _associator.DocTypePrefix + dti.Extension.ToUpperInvariant();
                RegistryHelper.SetValueLocal(fileAssocPath, dti.Extension, docType);

                RegistryHelper.SetClassesValueLocal(docType, null, dti.Description);
                RegistryHelper.SetClassesValueLocal(docType + @"\DefaultIcon", null, dti.DefaultIcon);
                RegistryHelper.SetClassesValueLocal(docType + @"\shell\open\command", null, dti.OpenCommand);
                if (dti.PlayCommandNeeded)
                    RegistryHelper.SetClassesValueLocal(docType + @"\shell\play\command", null, dti.PlayCommand);
            }
        }

        public void Unregister()
        {
            var capabilitiesPath = RegistryHelper.GetValueLocal(@"Software\RegisteredApplications", _associator.AppName, null);
            if (capabilitiesPath != null)
            {
                var fileAssocPath = String.Format(@"{0}\FileAssociations", capabilitiesPath);

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
                        if (_associator.CanAssociate)
                        {
                            _associator.UnAssociate(ext);
                        }
                        
                        var docType = RegistryHelper.GetValueLocal(fileAssocPath, ext, null);
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
                    key.DeleteValue(_associator.AppName);
                    key.Close();
                    key = null;
                }

                if (capabilitiesPath.LastIndexOf('\\') != -1)
                {
                    string appPath = capabilitiesPath.Substring(0, capabilitiesPath.LastIndexOf('\\'));
                    if (appPath.EndsWith(_associator.AppName) && appPath.LastIndexOf('\\') != -1)
                    {
                        string parent = appPath.Substring(0, appPath.LastIndexOf('\\'));
                        key = Registry.LocalMachine.OpenSubKey(parent, true);
                        if (key != null)
                            key.DeleteSubKeyTree(_associator.AppName);
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
    }
}