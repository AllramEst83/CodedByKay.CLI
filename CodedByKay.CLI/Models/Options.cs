using CommandLine;

namespace CodedByKay.CLI.Models
{
    public class Options
    {
        [Verb("smart-dialogue", HelpText = "Use the Smart Dialogue Service.")]
        public class SmartDialogueOptions
        {
            // Add options specific to the Smart Dialogue Service here
        }

        [Verb("assistants", HelpText = "Use the Smart Dialogue Assistants Service.")]
        public class AssistantsOptions
        {
            // Add options specific to the Assistants Service here
        }
    }
}
