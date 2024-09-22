using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NoHesi.databasemodels;

namespace NoHesi.Bot.Buttons;

public class LinkRemoveButtonModule : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly NoHesiBotContext dbcontext;

    public LinkRemoveButtonModule(NoHesiBotContext context) => this.dbcontext = context;

    [ComponentInteraction("linkremovebutton")]
    public async Task LinkRemove()
    {
        await this.DeferAsync();

        SteamLink steamLink = this.dbcontext.SteamLinks
                                  .FirstOrDefault(x => x.DiscordId == this.Context.User.Id + "")!;

        if (steamLink is not null)
        {
            this.dbcontext.SteamLinks.Remove(steamLink);
            await this.dbcontext.SaveChangesAsync();
        }

        EmbedBuilder embed = new()
        {
            Title = "Steam removed",
            Description = "Removed steam link",
            Color = Color.Blue,
        };

        await this.FollowupAsync(embed: embed.Build(), ephemeral: true);
    }
}