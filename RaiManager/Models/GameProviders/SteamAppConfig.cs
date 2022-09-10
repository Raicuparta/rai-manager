using System.Collections.Generic;
using Newtonsoft.Json;

namespace RaiManager.Models.GameProviders;

public class SteamAppConfig
{
    [JsonProperty("manifest_paths")] public List<string> ManifestPaths { get; set; } = new();
}