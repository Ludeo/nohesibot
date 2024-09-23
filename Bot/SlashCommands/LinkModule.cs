using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace NoHesi.Bot.SlashCommands;

[RequireUserPermission(GuildPermission.Administrator)]
[Group("link", "Links accounts to the discord account")]
public class LinkModule : InteractionModuleBase<SocketInteractionContext<SocketSlashCommand>>
{
    [SlashCommand("steam", "Links the steam account to the discord account")]
    public async Task LinkSteam([Summary("steam_id", "Your steam id")] string steamId)
    {
        await this.DeferAsync();

        ButtonBuilder button = new ButtonBuilder().WithCustomId("linkaddbutton").WithLabel("Link Steam").WithStyle(ButtonStyle.Secondary);
        ButtonBuilder button2 = new ButtonBuilder().WithCustomId("linkremovebutton").WithLabel("Remove Link").WithStyle(ButtonStyle.Danger);

        ComponentBuilder component =
            new ComponentBuilder().WithButton(button).WithButton(button2);

        EmbedBuilder embed = new()
        {
            Title = "Link Steam",
            Description = "Click on the Button below to link your Steam profile to your Discord account\n"
                        + "To get your Steam ID, please follow the following steps\n"
                        + "1. Press on your Steam name on the top right corner\n"
                        + "2. Press on Account Settings\n"
                        + "3. You should see your Steam ID right away below your account name",
            Color = Color.Blue,
        };

        await this.FollowupAsync(embed: embed.Build(), components: component.Build());
    }
}