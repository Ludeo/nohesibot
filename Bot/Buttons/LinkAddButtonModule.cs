using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace NoHesi.Bot.Buttons;

public class LinkAddButtonModule : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    [ComponentInteraction("linkaddbutton")]
    public async Task LinkAdd()
    {
        TextInputBuilder input = new TextInputBuilder()
                                 .WithCustomId("steamid")
                                 .WithLabel("Steam ID")
                                 .WithPlaceholder("e.g. 76561198143629166")
                                 .WithRequired(true)
                                 .WithMinLength(17)
                                 .WithMaxLength(17)
                                 .WithStyle(TextInputStyle.Short);

        Modal modal = new ModalBuilder()
                      .WithCustomId($"steamlink:{this.Context.User.Id}")
                      .WithTitle("Enter Steam ID")
                      .AddTextInput(input)
                      .Build();

        await this.RespondWithModalAsync(modal);
    }
}