using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NoHesi.Bot;
using NoHesi.Bot.FileObjects;
using NoHesi.Bot.QuartzJobs;
using NoHesi.Bot.Shared;
using NoHesi.databasemodels;
using Quartz;
using Quartz.Impl;

namespace NoHesi;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    private static DiscordSocketClient Client { get; set; }

    private static ServiceProvider services;

    public static SocketGuild Guild { get; private set; }

    public static SocketTextChannel LeaderboardChannel { get; set; }

    private static DiscordSocketConfig BuildDiscordSocketConfig()
    {
        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers,
            UseInteractionSnowflakeDate = false,
            AlwaysDownloadUsers = true,
        };

        return config;
    }

    private static ServiceProvider ConfigureServices() =>
        new ServiceCollection()
            .AddSingleton(new NoHesiBotContext())
            .AddSingleton(new DiscordSocketClient(BuildDiscordSocketConfig()))
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<LeaderboardJob>()
            .BuildServiceProvider();

    private static Task InitializeGlobalVariables()
    {
        Guild = Client.GetGuild(Config.Default.Server);
        LeaderboardChannel = Guild.GetTextChannel(Config.Default.LeaderboardChannel);

        return Task.CompletedTask;
    }

    private static async Task InitializeScheduledTask()
    {
        CustomJobFactory jobFactory = new(services);
        StdSchedulerFactory stdSchedulerFactory = new();
        IScheduler scheduler = await stdSchedulerFactory.GetScheduler();
        scheduler.JobFactory = jobFactory;

        await scheduler.Start();

        IJobDetail merchantJob = JobBuilder.Create<LeaderboardJob>()
                                           .WithIdentity("leaderboardjob", "leaderboardgroup")
                                           .Build();

        ITrigger merchantTrigger = TriggerBuilder.Create()
                                                 .WithIdentity("leaderboardtrigger", "leaderboardgroup")
                                                 .StartNow()
                                                 .WithCronSchedule("0 */5 * ? * *")
                                                 //.WithCronSchedule("0 0/1 * * * ?")
                                                 .Build();

        await scheduler.ScheduleJob(merchantJob, merchantTrigger);
        Client.Ready -= InitializeScheduledTask;
    }

    private static void Main() => MainAsync().GetAwaiter().GetResult();

    private static async Task MainAsync()
    {
        services = ConfigureServices();
        Client = services.GetRequiredService<DiscordSocketClient>();
        InteractionService commands = services.GetRequiredService<InteractionService>();
        CommandHandlingService commandHandlingService = services.GetRequiredService<CommandHandlingService>();

        await LogService.Log(LogSeverity.Info, "Setup", "============================================================================");
        await LogService.Log(LogSeverity.Info, "Setup", "=========================== Application starting ===========================");
        await LogService.Log(LogSeverity.Info, "Setup", "============================================================================");

        await commandHandlingService.Initialize();

        Client.Log += LogService.LogHandler;
        commands.Log += LogService.LogHandler;
        Client.Ready += InitializeGlobalVariables;
        Client.Ready += InitializeScheduledTask;

        string token = Config.Default.Token;

        if (string.IsNullOrEmpty(token))
        {
            await LogService.Log(
                                 LogSeverity.Critical,
                                 "Setup",
                                 "The bot token is not available in the config.json file. Add it and restart the bot.");

            Environment.Exit(0);
        }

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
        await Client.SetGameAsync("No Hesi");

        await Task.Delay(Timeout.Infinite);
    }
}