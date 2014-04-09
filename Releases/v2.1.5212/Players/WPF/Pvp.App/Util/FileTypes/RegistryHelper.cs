using System;
using Microsoft.Win32;

namespace Pvp.App.Util.FileTypes
{
    internal static class RegistryHelper
    {
        private const string SOFTWARE_CLASSES = @"Software\Classes\{0}";

        public static void SetClassesValueUser(string strSubKey, string name, object val)
        {
            SetValueUser(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        public static void SetClassesValueLocal(string strSubKey, string name, object val)
        {
            SetValueLocal(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        public static void SetValueUser(string strSubKey, string name, object val)
        {
            SetValue(Registry.CurrentUser, strSubKey, name, val);
        }

        public static void SetValueLocal(string strSubKey, string name, object val)
        {
            SetValue(Registry.LocalMachine, strSubKey, name, val);
        }

        private static void SetValue(RegistryKey parentKey, string strSubKey, string name, object val)
        {
            RegistryKey regkey = null;
            try
            {
                regkey = parentKey.OpenSubKey(strSubKey, true) ?? parentKey.CreateSubKey(strSubKey);

                if (regkey != null)
                    regkey.SetValue(name, val);
                else
                    throw new Exception("Failed to open or create registry key.");
            }
            finally
            {
                if (regkey != null)
                    regkey.Close();
            }
        }

        public static string GetClassesValueUser(string strSubKey, string name, object val)
        {
            return GetValueUser(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        public static string GetClassesValueLocal(string strSubKey, string name, object val)
        {
            return GetValueLocal(String.Format(SOFTWARE_CLASSES, strSubKey), name, val);
        }

        public static string GetValueUser(string strSubKey, string name, object val)
        {
            return GetValue(Registry.CurrentUser, strSubKey, name, val);
        }

        public static string GetValueLocal(string strSubKey, string name, object val)
        {
            return GetValue(Registry.LocalMachine, strSubKey, name, val);
        }

        private static string GetValue(RegistryKey parentKey, string strSubKey, string name, object val)
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
            finally
            {
                if (regkey != null)
                    regkey.Close();
            }
        }
    }
}