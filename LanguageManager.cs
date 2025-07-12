using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SignalAnalayzer
{
    public static class LanguageManager
    {
        
            private static readonly ResourceManager ResourceManager =
                new ResourceManager("SignalAnalayzer.Resources.Resources", Assembly.GetExecutingAssembly());

            public static string GetString(string key)
            {
                return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
            }
    }
}
