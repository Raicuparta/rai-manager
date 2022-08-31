using Newtonsoft.Json;

namespace RaiManager.Models.GameFinder;

public class EpicManifest
{
    [JsonProperty("InstallLocation")]
    public string InstallLocation { get; set; }
        
    [JsonProperty("CatalogItemId")]
    public string CatalogItemId { get; set; }
}