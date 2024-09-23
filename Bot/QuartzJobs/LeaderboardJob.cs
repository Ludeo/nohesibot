using System.Text.Json;
using Discord;
using Discord.WebSocket;
using NoHesi.Bot.FileObjects;
using NoHesi.Bot.FileObjects.NoHesi;
using NoHesi.Bot.Shared;
using NoHesi.databasemodels;
using Quartz;

namespace NoHesi.Bot.QuartzJobs;

public class LeaderboardJob(NoHesiBotContext dbcontext) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await LogService.Log(LogSeverity.Info, this.GetType().Name, "Executing Leaderboard Quartz job");

        SocketTextChannel textChannel = Program.LeaderboardChannel;
        IMessage message = await textChannel.GetMessagesAsync().Flatten().FirstAsync();

        EmbedBuilder embed = new()
        {
            Title = "Leaderboard",
            Description = "",
            Color = Color.Purple,
        };

        HttpClient client = new();

        List<LeaderboardPlayer> players = new();

        for (int i = 1; i < 11; i++)
        {
            using HttpResponseMessage response = await client.GetAsync("https://www.nohesi.gg/api/leaderboard?page=" + i + "&pageSize=100");
            LeaderboardAll leaderboardAll = JsonSerializer.Deserialize<LeaderboardAll>(await response.Content.ReadAsStringAsync());
            players.AddRange(leaderboardAll.Players);
        }

        List<SteamLink> steamLinks = dbcontext.SteamLinks.ToList();

        List<LeaderboardPlayer> discordPlayers = players.FindAll(x => steamLinks.Exists(y => y.SteamId == x.SteamId));

        int certifiedScore = players[499].Score;

        int localRanking = 1;

        for (int i = 0; i < discordPlayers.Count; i++)
        {
            ulong discordId = ulong.Parse(steamLinks.First(x => x.SteamId == discordPlayers[i].SteamId).DiscordId);
            SocketGuildUser discordUser = Program.Guild.GetUser(discordId);

            if (discordUser is null)
            {
                continue;
            }

            int position = players.IndexOf(discordPlayers[i]) + 1;

            embed.Description +=
                $"{localRanking}. {discordUser.Mention} {discordPlayers[i].Score.ToString("N").Replace(".00", "").Replace(",000", "")} [#{position}]";

            localRanking++;

            if (discordPlayers[i].Score >= certifiedScore)
            {
                embed.Description += " :white_check_mark:";
            }

            embed.Description += "\n";

            bool userHasRole = discordUser.Roles.Any(x => x.Id == Config.Default.CertifiedRoleId);

            if (position <= 500 && !userHasRole)
            {
                try
                {
                    SocketRole role = Program.Guild.GetRole(Config.Default.CertifiedRoleId);
                    await discordUser.AddRoleAsync(role, new RequestOptions());
                }
                catch (Exception e)
                {
                    await LogService.Log(LogSeverity.Error, this.GetType().Name, "Ex: " + e);
                }
            }
            else if (position > 500 && userHasRole)
            {
                await discordUser.RemoveRoleAsync(Config.Default.CertifiedRoleId);
            }
        }

        embed.Timestamp = DateTimeOffset.UtcNow;

        EmbedFooterBuilder footer = new()
        {
            Text = "Certified: " + players[499].Score.ToString("N").Replace(".00", "").Replace(",000", ""),
        };

        embed.Footer = footer;

        await textChannel.ModifyMessageAsync(messageId: message.Id, func => func.Embed = embed.Build());
    }
}