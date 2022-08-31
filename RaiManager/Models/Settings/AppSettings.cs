using System.Collections.Generic;
using Newtonsoft.Json;

namespace RaiManager.Models.Settings;

[JsonObject(MemberSerialization.OptIn)]
public class AppSettings
{
    /// <summary>
    /// Dictionary where keys are the <see cref="GameFinder.BaseFinder.Id"/> property of a class that extends
    /// <see cref="GameFinder.BaseFinder"/>, and the values are the full paths to the game's exe in that provider.
    /// </summary>
    [JsonProperty("paths")]
    public Dictionary<string, string> Paths {get; protected set;}
}