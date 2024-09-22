using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text.Json;
using NoHesi.Bot.FileObjects.NoHesi;
using NoHesi.databasemodels;

namespace NoHesi.Bot.SlashCommands;

[Group("leaderboard", "Shows the No Hesi Leaderboard for guild members")]
public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    private readonly NoHesiBotContext dbcontext;

    public LeaderboardModule(NoHesiBotContext dbcontext) => this.dbcontext = dbcontext;

    [SlashCommand("show", "Shows the No Hesi leaderboard for guild members")]
    public async Task ShowLeaderboard()
    {
        await this.DeferAsync();

        EmbedBuilder embed = new()
        {
            Title = "Leaderboard",
            Description = "",
            Color = Color.Purple,
        };

        HttpClientHandler handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        };

        HttpClient client = new();

        using HttpResponseMessage response = await client.GetAsync("https://www.nohesi.gg/api/leaderboard?page=1&pageSize=1000");
        LeaderboardAll leaderboardAll = JsonSerializer.Deserialize<LeaderboardAll>(await response.Content.ReadAsStringAsync());
        List<LeaderboardPlayer> players = leaderboardAll.Players;

        List<SteamLink> steamLinks = this.dbcontext.SteamLinks.ToList();

        List<LeaderboardPlayer> discordPlayers = players.FindAll(x => steamLinks.Exists(y => y.SteamId == x.SteamId));

        for (int i = 0; i < 10 && i < discordPlayers.Count; i++)
        {
            ulong discordId = ulong.Parse(steamLinks.First(x => x.SteamId == discordPlayers[i].SteamId).DiscordId);
            SocketGuildUser discordUser = this.Context.Guild.GetUser(discordId);
            embed.Description += discordUser.Mention + ", " + discordPlayers[i].Score.ToString("N") + "\n";
        }

        await this.FollowupAsync(embed: embed.Build(), allowedMentions: new AllowedMentions());
        //await this.Context.Channel.SendMessageAsync(embed: embed.Build());
    }
}