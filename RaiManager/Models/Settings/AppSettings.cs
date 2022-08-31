using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaiManager.Models.GameFinder;
using RaiManager.Models.Manifest;

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
    
    public static async Task LoadSettings(AppManifest appManifest, ManualGameFinder manualGameFinder)
    {
        var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
        Debug.WriteLine($"Lets se....");
        Debug.WriteLine($"Lets se.... {appManifest.Id}");
        var modDataPath = Path.Join(managerDataPath, appManifest.Id);
        var settingsDocument = await JsonHelper.Read<AppSettings>(Path.Join(modDataPath, "settings.json"));

        if (settingsDocument == null)
        {
            Debug.WriteLine($"No settings file found in {Path.Join(modDataPath, "settings.json")}");
            return;
        }
        
        settingsDocument.Paths.TryGetValue("manual", out var manualPath);

        if (string.IsNullOrEmpty(manualPath)) return;
        
        manualGameFinder.SetGamePath(manualPath);
    }
}