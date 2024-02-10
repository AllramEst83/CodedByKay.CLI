namespace CodedByKay.CLI.Models
{
    internal class ApplicationSettings
    {
        public string OpenAIApiKey { get; set; } = string.Empty;
        public string OpenAIAssistantId { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3-turbo";
        public string OpenAIApiUrl { get; set; } = "https://api.openai.com/v1/";
        public int MaxTokens { get; set; } = 2000;
        public double Temperature { get; set; } = 1;
        public double TopP { get; set; } = 1;
        public double AverageTokenLength { get; set; } = 2.85;
    }
}
