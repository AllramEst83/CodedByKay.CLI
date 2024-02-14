using CodedByKay.CLI.Models;
using CodedByKay.SmartDialogue.Assistants.Models;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

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

        internal static async Task<bool> UpdateAppSettingsAsync(string newAssistantId)
        {
            var environment = Environment.GetEnvironmentVariable("CODEDBYKAY_ENVIRONMENT");
            var fileName = "appSettings.json"; // Default file name
            if (!string.IsNullOrEmpty(environment))
            {
                fileName = $"appSettings.{environment}.json";
            }
            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            try
            {
                // Read the existing configuration
                var json = await File.ReadAllTextAsync(appSettingsPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Deserialize the JSON into the AppSettings class
                var config = JsonSerializer.Deserialize<AppSettings>(json, options);

                if (config?.ApplicationSettings != null)
                {
                    // Update the OpenAIAssistantId within ApplicationSettings
                    config.ApplicationSettings.OpenAIAssistantId = newAssistantId;

                    // Serialize the object back to JSON
                    var updatedJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

                    // Write the updated JSON back to the file
                    await File.WriteAllTextAsync(appSettingsPath, updatedJson);

                    return true;
                }

            }
            catch (Exception ex)
            {
                // Log the exception or provide feedback
                Console.Error.WriteLine($"An error occurred while updating the app settings: {ex.Message}");
            }

            return false;
        }
    }
}
