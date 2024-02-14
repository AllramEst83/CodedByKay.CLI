using CodedByKay.CLI.Constants;
using CodedByKay.CLI.Helpers;
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
        private const ConsoleColor SuccessBackgroundCollor = ConsoleColor.Green;
        private const ConsoleColor CycleAssistantIdsBackgroundColor = ConsoleColor.Yellow;

        /// <summary>
        /// Lists all assistants asynchronously by retrieving them from the service
        /// and displaying each assistant's details in the console.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get services.</param>
        /// <exception cref="InvalidOperationException">Thrown if the service factory is not available in the provided <paramref name="serviceProvider"/>.</exception>
        internal static async Task ListAssistants(IServiceProvider serviceProvider, ApplicationSettings applicationSettings)
        {
            var serviceFactory = serviceProvider.GetService<ISmartDialogueAssistantsServiceFactory>();
            if (serviceFactory == null)
            {
                throw new InvalidOperationException("Service factory not available.");
            }

            var service = serviceFactory.Create();
            var assistantListResponse = await service.ListAssistants();
            Assistant[] listOfAssistants = [.. assistantListResponse.Data];

            DisplayHeader();

            bool isDarkGreen = true; // Toggle for alternating background colors.
            foreach (var item in assistantListResponse.Data)
            {
                DisplayAssistant(item, isDarkGreen ? FirstBackgroundColor : SecondBackgroundColor);
                isDarkGreen = !isDarkGreen;
            }

            await PromptUserForCommand(applicationSettings, listOfAssistants);

            HandletDialogues.DisplayHelp();
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
        private static async Task PromptUserForCommand(ApplicationSettings applicationSettings, Assistant[] listOfAssistants)
        {
            var listOfAssistantIds = listOfAssistants.Select(i => i.Id).ToList();
            if (!listOfAssistantIds.Any())
            {
                DisplayFeedback("No assistants available.");
                return;
            }

            int currentIndex = 0; // Start with the first assistant ID.
            Console.WriteLine("\nNavigate through Assistant IDs using UP and DOWN arrow keys.");
            Console.WriteLine("Press ENTER to select the highlighted Assistant ID.");
            Console.WriteLine("Press E to exit.");

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.Write("\nCurrent Assistant ID: ");
            Console.ResetColor();

            // Initial display of the first assistant ID.
            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = CycleAssistantIdsBackgroundColor;
            Console.Write(listOfAssistantIds[currentIndex]);
            Console.ResetColor();
            while (true)
            {
                var key = Console.ReadKey(intercept: true); // Do not display the pressed key.

                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        currentIndex = (currentIndex + 1) % listOfAssistantIds.Count; // Cycle forward through the list.
                        break;
                    case ConsoleKey.UpArrow:
                        currentIndex = (currentIndex - 1 + listOfAssistantIds.Count) % listOfAssistantIds.Count; // Cycle backward through the list.
                        break;
                    case ConsoleKey.Enter:
                        // Attempt to update the assistant ID with the current selection.
                        var selectedAssistantId = listOfAssistantIds[currentIndex];
                        if (await TryUpdateAssistantId(applicationSettings, selectedAssistantId))
                        {
                            DisplaySuccessMessage();
                            return; // Exit after successful update.
                        }
                        else
                        {
                            DisplayFeedback("\nAn error occurred while updating the settings. Please restart the CLI tool.");
                            return; // Optionally exit or allow the user to try again.
                        }
                    case ConsoleKey.E:
                        return; // Exit without updating if 'E' is pressed.
                }

                UpdateCurrentAssistantIdDisplay(listOfAssistantIds[currentIndex]);
            }
        }

        private static void UpdateCurrentAssistantIdDisplay(string currentAssistantId)
        {
            // Move the cursor back to the start of the assistant ID display area.
            Console.SetCursorPosition("Current Assistant ID: ".Length, Console.CursorTop);

            // Clear the line from the current position.
            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));

            // Reset the cursor position again to rewrite the assistant ID.
            Console.SetCursorPosition("Current Assistant ID: ".Length, Console.CursorTop);

            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = CycleAssistantIdsBackgroundColor;
            Console.Write(currentAssistantId);
            Console.ResetColor();
        }


        private static void DisplayInitialInstructions()
        {
            Console.ForegroundColor = InputColor;
            Console.Write("\nEnter assistant id to switch from the default one (gpt-3.5-turbo).\nEnter exit to exit.");
            Console.ResetColor();
            Console.Write("\nEnter command: ");
        }

        private static string GetUserCommand()
        {
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        private static async Task<bool> TryUpdateAssistantId(ApplicationSettings applicationSettings, string assistantId)
        {
            applicationSettings.OpenAIAssistantId = assistantId;
            return await ApplicationHelpers.UpdateAppSettingsAsync(applicationSettings.OpenAIAssistantId);
        }

        private static void DisplayFeedback(string message)
        {
            Console.WriteLine(message);
            Console.Write("\nEnter command: "); // Prompt for next command.
        }

        private static void DisplaySuccessMessage()
        {
            Console.ForegroundColor = TextColor;
            Console.BackgroundColor = SuccessBackgroundCollor;
            Console.WriteLine("\n\nAssistant id has been successfully updated.Please  restart CLI tool 'smartd' Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}
