using System.Net;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace NoHesi.Bot.SlashCommands;

[RequireUserPermission(GuildPermission.Administrator)]
[Group("leaderboard", "Shows the No Hesi Leaderboard for guild members")]
public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
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

        await this.FollowupAsync(embed: embed.Build(), allowedMentions: new AllowedMentions());
    }
}