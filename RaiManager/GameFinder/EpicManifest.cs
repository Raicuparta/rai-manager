using Newtonsoft.Json;

namespace RaiManager.GameFinder
{
    public class EpicManifest
    {
        [JsonProperty("InstallLocation")]
        public string InstallLocation { get; set; }
        
        [JsonProperty("CatalogItemId")]
        public string CatalogItemId { get; set; }
    }
}