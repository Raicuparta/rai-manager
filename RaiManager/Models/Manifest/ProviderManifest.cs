using Newtonsoft.Json;
using RaiManager.Models.GameProviders;

namespace RaiManager.Models.Manifest;

[JsonObject(MemberSerialization.OptIn)]
public class ProviderManifest
{    
    /// <summary>
    /// Unique identifier for this provider. Must match the <sGameProvidereProvider.Id"/> property of a class
    /// that extends <see cref="GameProvider"/>.
    /// </summary>
    [JsonProperty("providerId")]
    public string ProviderId {get; protected set;}

    /// <summary>
    /// Unique identifier of the game in this provider. Can be a store id, or some other unique name.
    /// It depends on how exactly the game finder for this provider is implemented, so check the implementations to know
    /// which ID you need to provide here.
    /// </summary>
    [JsonProperty("gameIdentifier")]
    public string GameIdentifier {get; protected set;}
    
    /// <summary>
    /// File name of the game executable in this provider.
    /// </summary>
    [JsonProperty("gameExe")]
    public string GameExe {get; protected set;}

    /// <summary>
    /// If true, will run the game with admin privileges.
    /// This can be required if the mod tries to patch game files for UWP (Xbox / Game Pass) games.
    /// If the mod is able to run without modifying any game files, this shouldn't be needed.
    /// </summary>
    [JsonProperty("requireAdmin")]
    public bool RequireAdmin {get; protected set;}
}