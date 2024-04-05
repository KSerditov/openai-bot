namespace KSerditov.TgBot.Openai
{
    public interface IOpenAIService
    {
        Task<string> GetChatCompletion(string prompt);
    }
}