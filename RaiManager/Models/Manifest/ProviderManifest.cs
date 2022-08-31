using Newtonsoft.Json;

namespace RaiManager.Models.Manifest;

[JsonObject(MemberSerialization.OptIn)]
public class ProviderManifest
{    
    /// <summary>
    /// Unique identifier for this provider. Must match the <see cref="GameFinder.BaseFinder.Id"/> property of a class
    /// that extends <see cref="GameFinder.BaseFinder"/>.
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
}