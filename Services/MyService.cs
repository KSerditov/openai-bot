namespace KSerditov.TgBot.Openai;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MyService : IHostedService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("My Service Starting");
        // Your startup code here

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("My Service Stopping");
        // Your cleanup code here

        return Task.CompletedTask;
    }
}