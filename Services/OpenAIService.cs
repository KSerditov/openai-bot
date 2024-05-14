
using KSerditov.TgBot.Openai;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Models;

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<IOpenAIService> _logger;
    private readonly OpenAIAPI _api;

    public OpenAIService(ILogger<IOpenAIService> logger)
    {
        _logger = logger;
        //_api = new OpenAIAPI("sk-7M8v_<>_RO64");
        _api = new OpenAIAPI(APIAuthentication.LoadFromEnv()); // use env vars
    }

    public async Task<string> GetChatCompletion(string prompt)
    {
        OpenAI_API.Chat.Conversation chat = _api.Chat.CreateConversation();
        chat.Model = new Model("gpt-4o");
        chat.RequestParameters.Temperature = 0;

        chat.AppendUserInput(prompt);

        _logger.LogInformation($"Sending prompt to OpenAI API: '{prompt}'");

        string response = await chat.GetResponseFromChatbotAsync();

        _logger.LogInformation($"Received reply from OpenAI API: '{response}'");

        return response;
    }
}