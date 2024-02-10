using CodedByKay.CLI.Models;
using CodedByKay.SmartDialogue.Assistants.Interfaces;
using CodedByKay.SmartDialogueAssistantsOptions.Assistants.Helpers;
using CodedByKay.SmartDialogueLib;
using CodedByKay.SmartDialogueLib.Interfaces;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;

class Program
{
    private static List<string> commandHistory = new List<string>();
    private static int historyIndex = -1;
    private static readonly List<string> commands = new List<string> { "smart-dialogue", "assistants", "exit", "help" };
    private static readonly string model = "gpt-3.5-turbo";
    static async Task Main(string[] args)
    {
        DisplayHelp();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        StringBuilder inputBuilder = new StringBuilder();
        ConsoleKeyInfo keyInfo;

        while (true)
        {
            keyInfo = Console.ReadKey(true); // Intercept the key, don't display it yet

            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    HandleTabCompletion(ref inputBuilder, commands);
                    break;
                case ConsoleKey.Enter:
                    await ExecuteCommandAsync(inputBuilder.ToString().Trim(), serviceProvider);
                    inputBuilder.Clear(); // Clear the input builder for the next command
                    break;
                case ConsoleKey.Backspace:
                    HandleBackspace(ref inputBuilder);
                    break;
                case ConsoleKey.UpArrow:
                    NavigateCommandHistory(-1, ref inputBuilder);
                    break;
                case ConsoleKey.DownArrow:
                    NavigateCommandHistory(1, ref inputBuilder);
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write(keyInfo.KeyChar); // Display the character
                        inputBuilder.Append(keyInfo.KeyChar);
                    }
                    break;
            }
        }
    }

    private static void HandleTabCompletion(ref StringBuilder inputBuilder, List<string> commands)
    {
        var partialCommand = inputBuilder.ToString();
        var matches = commands.Where(c => c.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!matches.Any()) return; // No match found

        if (historyIndex >= 0 && historyIndex < matches.Count - 1)
        {
            // Cycle through matches on subsequent tab presses
            historyIndex++;
        }
        else
        {
            // Either no tab has been pressed yet, or we're at the end of the matches list
            historyIndex = 0;
        }

        var match = matches[historyIndex];
        ClearCurrentConsoleLine();
        Console.Write(match);
        inputBuilder.Clear();
        inputBuilder.Append(match);
    }

    private static async Task ExecuteCommandAsync(string command, IServiceProvider serviceProvider)
    {
        commandHistory.Add(command); // Add to history
        historyIndex = -1; // Reset history index

        if (command == "exit")
        {
            Environment.Exit(0); // Exit application
        }
        else if (command == "help")
        {
            DisplayHelp();
        }
        else
        {
            Console.Clear();
            // Handle other commands
            await Parser.Default.ParseArguments<Options.SmartDialogueOptions, Options.AssistantsOptions>(new[] { command })
                .MapResult(
                    (Options.SmartDialogueOptions opts) => HandleSmartDialogue(serviceProvider, Guid.NewGuid()),
                    (Options.AssistantsOptions opts) => HandleAssistants(serviceProvider, Guid.NewGuid()),
                    errs =>
                    {
                        foreach (var err in errs)
                        {
                            // You can switch on the error type if you want specific messages for each error type
                            switch (err)
                            {
                                case MissingRequiredOptionError missingRequired:
                                    Console.WriteLine($"The option '{missingRequired.NameInfo.NameText}' is required but was not provided.");
                                    break;
                                case UnknownOptionError unknownOption:
                                    Console.WriteLine($"The option '{unknownOption.Token}' is unknown.");
                                    break;
                                // Add more cases as necessary for different types of errors
                                default:
                                    Console.WriteLine("An error occurred parsing the command.");
                                    break;
                            }
                        }

                        DisplayHelp(); // Consider showing help or usage information here

                        return Task.FromResult(1); // Return a non-zero Task to indicate an error
                    });
        }
    }

    private static void HandleBackspace(ref StringBuilder inputBuilder)
    {
        if (inputBuilder.Length > 0)
        {
            inputBuilder.Remove(inputBuilder.Length - 1, 1);
            Console.Write("\b \b"); // Handle backspace visually
        }
    }

    private static void NavigateCommandHistory(int direction, ref StringBuilder inputBuilder)
    {
        if (commandHistory.Count == 0) return;

        historyIndex += direction;

        if (historyIndex < 0) historyIndex = 0; // Prevent going beyond the oldest command
        else if (historyIndex >= commandHistory.Count) historyIndex = commandHistory.Count - 1; // Prevent going beyond the most recent command

        inputBuilder.Clear();
        inputBuilder.Append(commandHistory[historyIndex]);
        ClearCurrentConsoleLine();
        Console.Write(inputBuilder.ToString());
    }


    private static void DisplayHelp()
    {
        Console.Clear(); // Clear the console
        PrintStartupMessage(); // Print the startup/help message
    }


    private static void PrintStartupMessage()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("CodedByKay.CLI 1.0.0\n");
        Console.ResetColor(); // Reset to default color

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Available commands:");
        Console.ResetColor(); // Reset to default color

        // Set specific colors for each command description
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  smart-dialogue    - Use the Smart Dialogue Service.");
        Console.WriteLine("  assistants        - Use the Smart Dialogue Assistants Service.");
        Console.WriteLine("  exit              - Exit the application.");
        Console.WriteLine("  help              - Display this help message.\n");
        Console.ResetColor(); // Reset to default color

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter a command: ");
        Console.ResetColor(); // Reset to default color
    }



    private static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    private static async Task HandleSmartDialogue(IServiceProvider serviceProvider, Guid userId)
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
        await HandleDialogue(service, userId);
    }

    private static async Task HandleAssistants(IServiceProvider serviceProvider, Guid userId)
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
        await HandleDialogue(service, userId);
    }


    private static async Task HandleDialogue(dynamic service, Guid userId)
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



    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSmartDialogue(options =>
        {
            options.OpenAIApiKey = "<api key>";
            options.Model = model;
            options.OpenAIApiUrl = "https://api.openai.com/v1/";
            options.MaxTokens = 1000;
            options.Temperature = 1;
            options.TopP = 1;
            options.AverageTokenLength = 2.85;

        });

        services.AddSmartDialogueAssistants(options =>
        {
            // All values are default for the CodedByKay.SmartDialogue.Assistants library
            options.OpenAIApiKey = "<api key>";
            options.OpenAIAssistantId = "<assistand id>";
            options.OpenAIApiUrl = "https://api.openai.com/v1/";
            options.MaxTokens = 1000;
            options.AverageTokenLength = 2.85;
        });
    }
}
