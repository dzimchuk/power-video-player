using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using Pvp.App.Util;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.Service
{
    internal class SettingsProvider : ISettingsProvider
    {
        private const string ConfigFile = "pvp2.config";
        private PropertyBag _props;

        public event EventHandler<SettingChangeEventArgs> SettingChanged;

        public SettingsProvider()
        {
            _props = new PropertyBag();
        	LoadSaveSettings(true);
        }

        public void Set<T>(string name, T value)
        {
            bool valueSet = false;

            T existingValue;
            if (_props.TryGetValue(name, out existingValue))
            {
            	if (!existingValue.Equals(value))
                {
                	_props.Set(name, value);
                    valueSet = true;
                }
            }
            else
            {
                _props.Set(name, value);
                valueSet = true;
            }

            if (valueSet)
                OnSettingChanged(new SettingChangeEventArgs(name));
        }

        public T Get<T>(string name, T defaultValue)
        {
            T value;
            if (!_props.TryGetValue(name, out value))
            {
            	_props.Set(name, defaultValue);
                value = defaultValue;
            }

            return value;
        }

        public void Save()
        {
            LoadSaveSettings(false);
        }

        private void LoadSaveSettings(bool bLoad)
        {
            IsolatedStorageFilePermission perm =
                new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            perm.UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser;
            if (!SecurityManager.IsGranted(perm))
            {
                return;
            }

            IsolatedStorageFileStream stream = null;
            try
            {
                IsolatedStorageFile storage =
                    IsolatedStorageFile.GetUserStoreForAssembly();
                if (bLoad)
                {
                    string[] astr = storage.GetFileNames(ConfigFile);
                    if (astr.Length > 0)
                    {
                        stream = new IsolatedStorageFileStream(ConfigFile, FileMode.Open,
                            FileAccess.Read, FileShare.Read, storage);

                        _props = new PropertyBag(stream);
                    }
                }
                else
                {
                    stream = new IsolatedStorageFileStream(ConfigFile, FileMode.Create, FileAccess.Write, storage);
                    _props.Save(stream);
                }
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        protected virtual void OnSettingChanged(SettingChangeEventArgs e)
        {
            var onSettingChanged = SettingChanged;
            if (onSettingChanged != null)
            {
                onSettingChanged(this, e);
            }
        }
    }
}
