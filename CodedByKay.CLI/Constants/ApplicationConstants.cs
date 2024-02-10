using CodedByKay.CLI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodedByKay.CLI.Constants
{
    internal static class ApplicationConstants
    {
        public static string PromptText { get; set; } = "Enter a command: ";
        public static string ApplicationSettingsString { get; set; } = "ApplicationSettings";
        public static List<string> Commands { get; set; } = new List<string> { "smart-dialogue", "assistants", "exit", "help" };
        public static string Help { get; set; } = "help";
        public static string Exit { get; set; } = "exit";

        public static string Version { get; set; } = ApplicationHelpers.Version;
    }

}
