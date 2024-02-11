using System.Reflection;

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
