using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodedByKay.CLI.Helpers
{
    internal class ApplicationHelpers
    {
        public static string Version
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                return assembly?.GetName().Version?.ToString() ?? "No version was found";
            }
        }
    }
}
