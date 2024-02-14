using CodedByKay.CLI.Constants;
using CodedByKay.SmartDialogue.Assistants.Interfaces;
using CodedByKay.SmartDialogueLib.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CodedByKay.CLI.Handlers
{
    internal static class HandletDialogues
    {
        internal static async Task HandleSmartDialogue(IServiceProvider serviceProvider, Guid userId, string model)
        {
            var serviceFactory = serviceProvider.GetService<ISmartDialogueServiceFactory>();
            if (serviceFactory == null)
            {
                Console.WriteLine("Smart Dialogue Service unavailable");
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Welcome to Smart Dialogue\n");
            Console.ResetColor();

            var service = serviceFactory.Create();
            await HandleDialogue(service, userId, model);
        }

        internal static async Task HandleAssistants(IServiceProvider serviceProvider, Guid userId, string model)
        {
            var serviceFactory = serviceProvider.GetService<ISmartDialogueAssistantsServiceFactory>();
            if (serviceFactory == null)
            {
                Console.WriteLine("Assistants Service unavailable");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Welcome to Smart Dialogue Assistants\n");
            Console.ResetColor();

            var service = serviceFactory.Create();
            await HandleDialogue(service, userId, model);
        }


        private static async Task HandleDialogue(dynamic service, Guid userId, string model)
        {
            Console.ForegroundColor = ConsoleColor.Cyan; // Set user prompt color.
            Console.Write("\nuser: "); // Display the prompt on the same line as where the user types their input.
            Console.ResetColor(); // Reset to default color.

            while (true)
            {
                var message = Console.ReadLine();
                if (string.Equals(message, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    DisplayHelp(); // Call your help display method
                    break; // or continue based on your logic
                }

                if (!string.IsNullOrEmpty(message))
                {
                    var response = await service.SendChatMessageAsync(userId, message);
                    Console.ForegroundColor = ConsoleColor.Yellow; // Set "Response" label color.
                    Console.Write($"\n{model}: ");
                    Console.ForegroundColor = ConsoleColor.White; // Set actual response text color.
                    Console.WriteLine($"{response}");
                    Console.ResetColor(); // Reset to default color after displaying the response.
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red; // Set warning color.
                    Console.WriteLine("Message is empty, please enter a valid message or type 'exit' to return.");
                    Console.ResetColor(); // Reset to default color.
                }
                Console.ForegroundColor = ConsoleColor.Cyan; // Set user prompt color for the next message.
                Console.Write("\nuser: "); // Prompt again for the next message on the same line.
                Console.ResetColor(); // Reset to default color.
            }
        }

        internal static void DisplayHelp()
        {
            Console.Clear(); // Clear the console
            PrintStartupMessage(); // Print the startup/help message
        }


        internal static void PrintStartupMessage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"CodedByKay.CLI {ApplicationConstants.Version}\n");
            Console.ResetColor(); // Reset to default color

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Available commands:");
            Console.ResetColor(); // Reset to default color

            // Set specific colors for each command description
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  smart-dialogue    - Use the Smart Dialogue Service.");
            Console.WriteLine("  assistants        - Use the Smart Dialogue Assistants Service.");
            Console.WriteLine("  list-assistants   - List and swicth assistants connected to your api key.");
            Console.WriteLine("  exit              - Exit the application or dialogues.");
            Console.WriteLine("  help              - Display this help message.\n");
            Console.ResetColor(); // Reset to default color

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter a command: ");
            Console.ResetColor(); // Reset to default color
        }
    }
}
