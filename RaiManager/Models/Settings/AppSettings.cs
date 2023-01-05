using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RaiManager.Models.GameProviders;
using RaiManager.Models.Manifest;

namespace RaiManager.Models.Settings;

[JsonObject(MemberSerialization.OptIn)]
public class AppSettings
{
    private const string SettingsFileName = "settings.json";

    /// <summary>
    /// Dictionary where keys are the <see cref="GameProvider.Id"/> property of a class that extends
    /// <see cref="GameProvider"/>, and the values are the full paths to the game's exe in that provider.
    /// </summary>
    [JsonProperty("paths")]
    public Dictionary<string, string> Paths { get; protected set; } = new();
    
    public static async Task<AppSettings?> LoadSettings(AppManifest appManifest, ManualProvider manualProvider)
    {
        var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
        var modDataPath = Path.Join(managerDataPath, appManifest.Id);
        var appSettings = await JsonHelper.Read<AppSettings>(Path.Join(modDataPath, SettingsFileName));

        if (appSettings == null)
        {
            Debug.WriteLine($"No settings file found in {Path.Join(modDataPath, SettingsFileName)}");
            return null;
        }
        
        appSettings.Paths.TryGetValue("manual", out var manualPath);

        if (string.IsNullOrEmpty(manualPath)) return null;
        
        manualProvider.SetGamePath(manualPath);

        return appSettings;
    }
    
    public static async void WriteSettings(AppSettings? baseSettings, string manualGamePath, AppManifest manifest)
    {
        var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
        var modDataPath = Path.Join(managerDataPath, manifest.Id);
        var settingsPath = Path.Join(modDataPath, SettingsFileName);
        var settingsDocument = baseSettings ?? new AppSettings();

        settingsDocument.Paths["manual"] = manualGamePath;
        
        Directory.CreateDirectory(modDataPath);
        
        await File.WriteAllTextAsync(
            settingsPath,
            JsonConvert.SerializeObject(settingsDocument, Formatting.Indented),
            Encoding.Unicode);
    }
}