using Newtonsoft.Json;

namespace RaiManager.Models.Manifest;

[JsonObject(MemberSerialization.OptIn)]
public class Manifest
{
    [JsonProperty("id")]
    public string Id {get; protected set;}

    [JsonProperty("modTitle")]
    public string ModTitle {get; protected set;}

    [JsonProperty("gameTitle")]
    public string GameTitle {get; protected set;}

    [JsonProperty("providers")]
    public ProviderManifest[] Providers {get; protected set;}
}