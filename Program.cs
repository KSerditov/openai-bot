using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KSerditov.TgBot.Openai;

class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.File(@"log/telegram.bot.openai.txt", rollOnFileSizeLimit: true, fileSizeLimitBytes: 1000000, retainedFileCountLimit: 10,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.ms}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient("TelegramBotClient", client =>
                    {
                        client.BaseAddress = new Uri("https://api.telegram.org");
                        client.Timeout = TimeSpan.FromSeconds(15);
                    });
                services.AddHostedService<TelegramService>();
                services.AddSingleton<IOpenAIService, OpenAIService>();
            });
}
