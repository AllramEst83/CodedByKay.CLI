using CodedByKay.CLI.Models;
using CodedByKay.SmartDialogue.Assistants.Interfaces;
using CodedByKay.SmartDialogue.Assistants.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CodedByKay.CLI.Handlers
{
    /// <summary>
    /// Handles the management and display of assistant information.
    /// </summary>
    internal class HandleAssistantsManager
    {
        // Define color constants used for console display.
        private const ConsoleColor HeaderColor = ConsoleColor.Blue;
        private const ConsoleColor InputColor = ConsoleColor.White;
        private const ConsoleColor FirstBackgroundColor = ConsoleColor.DarkGreen;
        private const ConsoleColor SecondBackgroundColor = ConsoleColor.Yellow;
        private const ConsoleColor TextColor = ConsoleColor.Black;

        /// <summary>
        /// Lists all assistants asynchronously by retrieving them from the service
        /// and displaying each assistant's details in the console.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get services.</param>
        /// <exception cref="InvalidOperationException">Thrown if the service factory is not available in the provided <paramref name="serviceProvider"/>.</exception>
        internal static async Task ListAssistants(IServiceProvider serviceProvider)
        {
            var serviceFactory = serviceProvider.GetService<ISmartDialogueAssistantsServiceFactory>();
            if (serviceFactory == null)
            {
                throw new InvalidOperationException("Service factory not available.");
            }

            var service = serviceFactory.Create();
            var assistantListResponse = await service.ListAssistants();

            DisplayHeader();

            bool isDarkGreen = true; // Toggle for alternating background colors.
            foreach (var item in assistantListResponse.Data)
            {
                DisplayAssistant(item, isDarkGreen ? FirstBackgroundColor : SecondBackgroundColor);
                isDarkGreen = !isDarkGreen;
            }

            PromptUserForCommand();
        }

        /// <summary>
        /// Displays the header information in the console.
        /// </summary>
        private static void DisplayHeader()
        {
            Console.ForegroundColor = HeaderColor;
            Console.WriteLine("Welcome to a list of your assistants\n");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the details of a single assistant, including their ID, name, model, and tools,
        /// with a specified background color.
        /// </summary>
        /// <param name="item">The assistant to display.</param>
        /// <param name="backgroundColor">The background color for displaying the assistant's information.</param>
        private static void DisplayAssistant(Assistant item, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = TextColor;

            string toolsString = ConstructToolsString(item.Tools);

            Console.WriteLine($"  Id: {item.Id} - Name: {item.Name} - Model: {item.Model} - Tools: {toolsString}");

            Console.ResetColor();
        }

        /// <summary>
        /// Constructs a string representing the tools associated with an assistant.
        /// If no tools are present, returns "no tool".
        /// </summary>
        /// <param name="tools">The collection of tools to construct the string from.</param>
        /// <returns>A string representation of the tools.</returns>
        private static string ConstructToolsString(object[] tools)
        {
            if (tools == null || tools.Length == 0)
            {
                return "no tool";
            }

            var toolsList = tools
                .Select(toolJson => JsonConvert.DeserializeObject<Tool>(toolJson?.ToString() ?? "{}"))
                .Where(tool => tool != null)
                .Select(tool => tool!.Type)
                .ToArray();

            return toolsList.Length != 0 ? string.Join(", ", toolsList) : "no tool";
        }

        /// <summary>
        /// Prompts the user for a command, then displays the help dialogue.
        /// </summary>
        private static void PromptUserForCommand()
        {
            Console.ForegroundColor = InputColor;
            Console.Write("\nPress any key to continue...");
            Console.ResetColor();
            Console.ReadLine();
            // Assuming HandletDialogues.DisplayHelp() is defined elsewhere and provides help information to the user.
            HandletDialogues.DisplayHelp();
        }
    }

}
