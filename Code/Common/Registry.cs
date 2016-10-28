using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointVisualizer
{
    // Registry stuff ------------------------------------------------------------------------------------------------------------
    class Registry
    {
        // The name of the key must include a valid root. 
        const string userRoot = "HKEY_CURRENT_USER";
        const string subkey = "PointVisualizer";
        const string keyName = userRoot + "\\" + subkey;

        const string LastPathValue = "LastPath";
        const string LastPathDefault = @"C:\Points\";

        const string LastFileFilterValue = "FileFilter";
        const int LastFileFilterDefault = 1;

        private static void CheckRegistry()
        {
            // Do the values exist?
            if (Microsoft.Win32.Registry.GetValue(keyName, LastPathValue, null) == null)
            {
                // First time, write defaults                
                WriteRegistry(LastPathDefault, LastFileFilterDefault);
            }
        }

        public static string GetLastPath()
        {
            CheckRegistry();
            return Microsoft.Win32.Registry.GetValue(keyName, LastPathValue, LastPathDefault).ToString();
        }

        public static int GetLastFileFilter()
        {
            CheckRegistry();
            return int.Parse(Microsoft.Win32.Registry.GetValue(keyName, LastFileFilterValue, LastFileFilterDefault).ToString());
        }

        public static void WriteRegistry(String LastPath, int LastFileFilter)
        {
            Microsoft.Win32.Registry.SetValue(keyName, LastPathValue, LastPath);
            Microsoft.Win32.Registry.SetValue(keyName, LastFileFilterValue, LastFileFilter.ToString());
        }

        // --- End of Registry Stuff -----
    }
}
