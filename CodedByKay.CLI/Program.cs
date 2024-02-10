using CodedByKay.CLI.Constants;
using CodedByKay.CLI.Extensions;
using CodedByKay.CLI.Handlers;
using CodedByKay.CLI.Models;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

class Program
{
    private static List<string> commandHistory = [];
    private static int historyIndex = -1;
    static async Task Main(string[] args)
    {
        var configuration = ServiceCollectionExtensions.BuildConfiguration();
        var appSettings = configuration.GetSection(ApplicationConstants.ApplicationSettingsString).Get<ApplicationSettings>();
        if (appSettings == null)
        {
            throw new Exception("ApplicationSettings can not be null.");
        }

        var serviceCollection = new ServiceCollection();
        ServiceCollectionExtensions.AddCustomSmartDialogueServices(serviceCollection, appSettings);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        HandletDialogues.DisplayHelp();

        StringBuilder inputBuilder = new StringBuilder();
        ConsoleKeyInfo keyInfo;

        while (true)
        {
            keyInfo = Console.ReadKey(true); // Intercept the key, don't display it yet

            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    HandleTabCompletion(ref inputBuilder, ApplicationConstants.Commands, ApplicationConstants.PromptText);
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

    private static void HandleTabCompletion(ref StringBuilder inputBuilder, List<string> commands, string promptText)
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

        // Clear the console line and redraw with prompt and matched command
        ClearCurrentConsoleLine();
        Console.Write($"\n{promptText}{match}"); // Combine the prompt text and the match
        inputBuilder.Clear();
        inputBuilder.Append(match); // Update the input builder with the matched command
    }


    private static async Task ExecuteCommandAsync(string command, IServiceProvider serviceProvider)
    {
        commandHistory.Add(command); // Add to history
        historyIndex = -1; // Reset history index

        if (command == ApplicationConstants.Exit)
        {
            Environment.Exit(0); // Exit application
        }
        else if (command == ApplicationConstants.Help)
        {
            HandletDialogues.DisplayHelp();
        }
        else
        {
            Console.Clear();
            // Handle other commands
            await Parser.Default.ParseArguments<Options.SmartDialogueOptions, Options.AssistantsOptions>(new[] { command })
                .MapResult(
                    (Options.SmartDialogueOptions opts) => HandletDialogues.HandleSmartDialogue(serviceProvider, Guid.NewGuid(), "AI"),
                    (Options.AssistantsOptions opts) => HandletDialogues.HandleAssistants(serviceProvider, Guid.NewGuid(), "AI"),
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

                        HandletDialogues.DisplayHelp(); // Consider showing help or usage information here

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
    private static void ClearCurrentConsoleLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }
}
