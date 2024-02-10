using CodedByKay.CLI.Models;
using CodedByKay.SmartDialogueAssistantsOptions.Assistants.Helpers;
using CodedByKay.SmartDialogueLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CodedByKay.CLI.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddCustomSmartDialogueServices(this IServiceCollection services,ApplicationSettings applicationSettings)
        {
            services.AddSmartDialogue(options =>
            {
                options.OpenAIApiKey = applicationSettings.OpenAIApiKey;
                options.Model = applicationSettings.Model;
                options.OpenAIApiUrl = applicationSettings.OpenAIApiUrl;
                options.MaxTokens = applicationSettings.MaxTokens;
                options.Temperature = applicationSettings.Temperature;
                options.TopP = applicationSettings.TopP;
                options.AverageTokenLength = applicationSettings.AverageTokenLength;
            });

            services.AddSmartDialogueAssistants(options =>
            {
                // All values are default for the CodedByKay.SmartDialogue.Assistants library
                options.OpenAIApiKey = applicationSettings.OpenAIApiKey;
                options.OpenAIAssistantId = applicationSettings.OpenAIAssistantId;
                options.OpenAIApiUrl = applicationSettings.OpenAIApiUrl;
                options.MaxTokens = applicationSettings.MaxTokens;
                options.AverageTokenLength = applicationSettings.AverageTokenLength;
            });
        }

        public static IConfigurationRoot BuildConfiguration()
        {
            // Use AppContext.BaseDirectory to support single-file deployment scenarios
            var basePath = AppContext.BaseDirectory;

            var environment = Environment.GetEnvironmentVariable("CODEDBYKAY_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath) // Use the application's base directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            if (!string.IsNullOrEmpty(environment))
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            }

            builder.AddEnvironmentVariables();

            return builder.Build();
        }

    }
}