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
    private readonly IOpenAIService _chatgpt;
    private readonly IHttpClientFactory _httpClientFactory;
    private TelegramBotClient _client = null!;

    public TelegramService(ILogger<TelegramService> logger, IOpenAIService chatgpt, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _chatgpt = chatgpt;
        InitializeClient();
    }

    private void InitializeClient()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("TelegramBotClient");
        //_client = new TelegramBotClient("7111936_<>_0KZE");
        _client = new TelegramBotClient(Environment.GetEnvironmentVariable("OPENAI_TG_BOT") ?? "", httpClient);
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
        string strUpdate = JsonConvert.SerializeObject(update);
        _logger.LogInformation($"Received update: '{strUpdate}'");


        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;
        if (update.Message.From?.Id == botClient.BotId)
            return;

        long chatId = message.Chat.Id;

        Message? awaitMessageId = await SendTgMessageAsync(
            botClient,
            chatId,
            update.Message.MessageId,
            $"Prompt accepted, please wait for reply.",
            cancellationToken
        );

        string completion;
        try
        {
            completion = await _chatgpt.GetChatCompletion(messageText);
        }
        catch (Exception ex)
        {
            completion = $"Request Failed: {ex.Message}";
        }

        await SendTgMessageAsync(
            botClient,
            chatId,
            update.Message.MessageId,
            completion,
            cancellationToken
        );

        if (awaitMessageId != null)
            await botClient.DeleteMessageAsync(chatId, awaitMessageId.MessageId, cancellationToken);

    }

    private async Task<Message?> SendTgMessageAsync(ITelegramBotClient botClient, long chatId, int replyToMessageId, string text, CancellationToken cancellationToken)
    {
        Message? msg = null;
        try
        {
            msg = await botClient.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: replyToMessageId,
                text: text,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occured while SengTgMessageAsync.");
        }
        return msg;
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