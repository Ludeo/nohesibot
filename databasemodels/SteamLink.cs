using System;
using System.Collections.Generic;

namespace NoHesi.databasemodels;

public partial class SteamLink
{
    public string DiscordId { get; set; } = null!;

    public string SteamId { get; set; } = null!;
}
