using System.Text.Json;
using NoHesi.Bot.FileObjects;

namespace NoHesi.Bot.Shared;

public static class JsonParsers
{
    public static Config GetConfigFromJson()
    {
        if (!File.Exists(FileConfigurations.ConfigJson))
        {
            Config configFile = new();
            File.WriteAllText(FileConfigurations.ConfigJson, JsonSerializer.Serialize(configFile));

            return configFile;
        }

        return JsonSerializer.Deserialize<Config>(File.ReadAllText(FileConfigurations.ConfigJson));
    }

    public static async Task WriteConfigAsync(Config values) =>
        await File.WriteAllTextAsync(FileConfigurations.ConfigJson, JsonSerializer.Serialize(values));
}