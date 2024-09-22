using System.Text.Json.Serialization;

namespace NoHesi.Bot.FileObjects.NoHesi;

public class LeaderboardAll
{
    [JsonPropertyName("players")]
    public List<LeaderboardPlayer> Players { get; set; }
}