using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaiManager.Models.Manifest;

[JsonObject(MemberSerialization.OptIn)]
public class AppManifest
{
    private const string ManifestPath = "./Mod/manifest.json";
    
    [JsonProperty("id")]
    public string Id {get; protected set;}

    [JsonProperty("modTitle")]
    public string ModTitle {get; protected set;}

    [JsonProperty("gameTitle")]
    public string GameTitle {get; protected set;}
    
    [JsonProperty("modAuthor")]
    public string? ModAuthor {get; protected set;}

    [JsonProperty("providers")]
    public ProviderManifest[] Providers {get; protected set;}
    
    public static async Task<AppManifest?> LoadManifest()
    {
        if (!File.Exists(ManifestPath)) throw new FileNotFoundException($"Failed to find manifest in {ManifestPath}");
        
        var manifest = await JsonHelper.Read<AppManifest>(ManifestPath);

        if (manifest == null) throw new FileNotFoundException($"Failed to read manifest in {ManifestPath}");

        return manifest;
    }
}