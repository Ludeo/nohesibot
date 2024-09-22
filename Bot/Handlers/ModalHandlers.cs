using Discord;
using Discord.WebSocket;
using NoHesi.databasemodels;

namespace NoHesi.Bot.Handlers;

public class ModalHandlers
{
    private readonly NoHesiBotContext dbcontext;

    public ModalHandlers(NoHesiBotContext context) => this.dbcontext = context;

    public async Task ModalHandler(SocketModal modal)
    {
        await modal.DeferAsync();

        if (modal.Data.CustomId[..9] == "steamlink")
        {
            string discordId = modal.Data.CustomId[10..];
            string steamId = modal.Data.Components.First(x => x.CustomId == "steamid").Value;

            SteamLink steamLink = this.dbcontext.SteamLinks
                                      .FirstOrDefault(x => x.DiscordId == discordId)!;

            if (steamLink is null)
            {
                this.dbcontext.SteamLinks.Add(new SteamLink()
                {
                    DiscordId = discordId + "",
                    SteamId = steamId,
                });

                await this.dbcontext.SaveChangesAsync();

                EmbedBuilder embed = new()
                {
                    Title = "Steam linked",
                    Description = "Steam ID linked " + steamId,
                    Color = Color.Blue,
                };

                await modal.FollowupAsync(embed: embed.Build(), ephemeral: true);
            }
            else
            {
                steamLink.SteamId = steamId;
                this.dbcontext.SteamLinks.Update(steamLink);
                await this.dbcontext.SaveChangesAsync();

                EmbedBuilder embed = new()
                {
                    Title = "Steam updated",
                    Description = "Steam ID updated to " + steamId,
                    Color = Color.Blue,
                };

                await modal.FollowupAsync(embed: embed.Build(), ephemeral: true);
            }
        }
    }
}