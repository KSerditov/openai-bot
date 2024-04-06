namespace KSerditov.TgBot.Openai;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramService : BackgroundService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly TelegramBotClient _client;
    private readonly IOpenAIService _chatgpt;

    public TelegramService(ILogger<TelegramService> logger, IOpenAIService chatgpt)
    {
        _logger = logger;
        //_client = new TelegramBotClient("7111936_<>_0KZE");
        _client = new TelegramBotClient(Environment.GetEnvironmentVariable("OPENAI_TG_BOT") ?? "");
        _chatgpt = chatgpt;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram Bot service running.");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = new UpdateType[] { UpdateType.Message }
        };

        _client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Telegram Bot started polling for updates.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;

        long chatId = message.Chat.Id;

        _logger.LogInformation($"Received update: '{update}'", JsonConvert.SerializeObject(update));

        if (!messageText.ToLower().StartsWith(@"/gpt "))
        {
            return;
        }

        string prompt = messageText.Substring(@"/gpt ".Length);

        _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            replyToMessageId: update.Message.MessageId,
            text: "Prompt accepted, please wait for reply.",
            cancellationToken: cancellationToken);

        string completion;
        try
        {
            completion = await _chatgpt.GetChatCompletion(prompt);
        }
        catch (Exception ex)
        {
            completion = $"Request Failed: {ex.Message}";
        }

        _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            replyToMessageId: update.Message.MessageId,
            text: completion,
            cancellationToken: cancellationToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        string ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError($"An error occurred: {ErrorMessage}");

        return Task.CompletedTask;
    }
}