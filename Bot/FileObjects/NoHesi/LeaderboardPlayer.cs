using System.Text.Json.Serialization;

namespace NoHesi.Bot.FileObjects.NoHesi;

public class LeaderboardPlayer
{
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("steamid")]
    public String SteamId { get; set; }
    
    [JsonPropertyName("input")]
    public String Input { get; set; }
    
    [JsonPropertyName("combo")]
    public String Combo { get; set; }
    
    [JsonPropertyName("avg_speed")]
    public String AverageSpeed { get; set; }
    
    [JsonPropertyName("run_time")]
    public String RunTime { get; set; }
    
    [JsonPropertyName("run_distance")]
    public String RunDistance { get; set; }
    
    [JsonPropertyName("car_model")]
    public String CarModel { get; set; }
    
    [JsonPropertyName("server")]
    public String Server { get; set; }
    
    [JsonPropertyName("map")]
    public String Map { get; set; }
    
    [JsonPropertyName("top_proxy_steamid")]
    public String TopProxySteamId { get; set; }
    
    [JsonPropertyName("label")]
    public String Label { get; set; }
}