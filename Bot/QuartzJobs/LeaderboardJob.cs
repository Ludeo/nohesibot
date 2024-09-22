using System.Text.Json;
using Discord;
using Discord.WebSocket;
using NoHesi.Bot.FileObjects.NoHesi;
using NoHesi.Bot.Shared;
using NoHesi.databasemodels;
using Quartz;

namespace NoHesi.Bot.QuartzJobs;

public class LeaderboardJob(NoHesiBotContext dbcontext) : IJob
{
    private readonly Dictionary<int, ulong> scoreRoles = new()
    {
        { 10, 1284217204228886548 },
        { 20, 1284219941838393477 },
        { 30, 1284217482328018964 },
        { 40, 1284219995970211871 },
        { 60, 1284219327142170745 },
        { 100, 1284220324178694184 },
        { 150, 1284220398451294291 },
        { 200, 1284220486573621269 },
    };

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

        using HttpResponseMessage response = await client.GetAsync("https://www.nohesi.gg/api/leaderboard?page=1&pageSize=1000");
        LeaderboardAll leaderboardAll = JsonSerializer.Deserialize<LeaderboardAll>(await response.Content.ReadAsStringAsync());
        List<LeaderboardPlayer> players = leaderboardAll.Players;

        List<SteamLink> steamLinks = dbcontext.SteamLinks.ToList();

        List<LeaderboardPlayer> discordPlayers = players.FindAll(x => steamLinks.Exists(y => y.SteamId == x.SteamId));

        int certifiedScore = players[499].Score;

        for (int i = 0; i < 20 && i < discordPlayers.Count; i++)
        {
            ulong discordId = ulong.Parse(steamLinks.First(x => x.SteamId == discordPlayers[i].SteamId).DiscordId);
            SocketGuildUser discordUser = Program.Guild.GetUser(discordId);
            int position = players.IndexOf(discordPlayers[i]) + 1;

            embed.Description +=
                $"{i}. {discordUser.Mention} {discordPlayers[i].Score.ToString("N").Replace(".00", "").Replace(",000", "")} [#{position}]";

            if (discordPlayers[i].Score >= certifiedScore)
            {
                embed.Description += " :white_check_mark:";
            }

            embed.Description += "\n";

            ulong roleId = this.GetScoreRole(discordPlayers[i].Score);

            if (roleId != 0)
            {
                bool userHasRole = discordUser.Roles.Any(x => x.Id == roleId);

                if (!userHasRole)
                {
                    List<SocketRole> assignedRoles = discordUser.Roles.Where(x => this.scoreRoles.Values.Contains(x.Id)).ToList();

                    bool hasNewRole = false;

                    foreach (SocketRole roleToDelete in assignedRoles)
                    {
                        if (roleToDelete.Id != roleId)
                        {
                            await discordUser.RemoveRoleAsync(roleToDelete.Id);
                        }
                        else
                        {
                            hasNewRole = true;
                        }
                    }

                    if (!hasNewRole)
                    {
                        try
                        {
                            SocketRole role = Program.Guild.GetRole(roleId);
                            await discordUser.AddRoleAsync(role, new RequestOptions());
                        }
                        catch (Exception e)
                        {
                            await LogService.Log(LogSeverity.Error, this.GetType().Name, "Ex: " + e);
                        }
                    }
                }
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

    private ulong GetScoreRole(int score)
    {
        int millions = score / 1000000;

        switch (millions)
        {
            case < 10:             return 0;
            case >= 10 and < 20:   return this.scoreRoles[10];
            case >= 20 and < 30:   return this.scoreRoles[20];
            case >= 30 and < 40:   return this.scoreRoles[30];
            case >= 40 and < 60:   return this.scoreRoles[40];
            case >= 60 and < 100:  return this.scoreRoles[60];
            case >= 100 and < 150: return this.scoreRoles[100];
            case >= 150 and < 200: return this.scoreRoles[150];
            case >= 200:           return this.scoreRoles[200];
        }
    }
}