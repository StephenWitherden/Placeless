using Placeless.App.Windows.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Placeless.App.Windows
{
    class SettingsBasedConfig : IPlacelessconfig
    {
        public SettingsBasedConfig()
        {
            if (Settings.Default.CallUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CallUpgrade = false;
                Settings.Default.Save();
            }
        }

        public string GetValue(string path)
        {
            path = path.Replace(":", "_");
            return Settings.Default[path].ToString();
        }

        public IEnumerable<string> GetValues(string path)
        {
            path = path.Replace(":", "_");
            var stringCollection = Settings.Default[path] as System.Collections.Specialized.StringCollection;
            return stringCollection.OfType<string>();
        }

        public void SetValue(string path, string value)
        {
            path = path.Replace(":", "_");
            Settings.Default[path] = value;
            Settings.Default.Save();
        }

        public void SetValues(string path, IEnumerable<string> values)
        {
            path = path.Replace(":", "_");
            var stringCollection = new System.Collections.Specialized.StringCollection();
            stringCollection.AddRange(values.ToArray());
            Settings.Default[path] = stringCollection;
            Settings.Default.Save();
        }
    }
}
