using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using RaiManager.Models.GameFinder;
using RaiManager.Models.Manifest;
using ReactiveUI;

namespace RaiManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string IconPath = "./Mod/icon.png";
    private const string ManifestPath = "./Mod/manifest.json";
    private Bitmap? _icon;
    public Bitmap? Icon
    {
        get => _icon;
        private set => this.RaiseAndSetIfChanged(ref _icon, value);
    }
        
    private string _statusText = "Loading...";
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    private List<BaseFinder> _gameFinders = new();
    public List<BaseFinder> GameFinders
    {
        get => _gameFinders;
        set => this.RaiseAndSetIfChanged(ref _gameFinders, value);
    }
        
    private string? _gameExePath;
    public string? GameExePath
    {
        get => _gameExePath;
        set
        {
            if (value != null && !File.Exists(value))
            {
                this.RaiseAndSetIfChanged(ref _gameExePath, null);
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _gameExePath, value);
            }
            // WriteSettings();
        }
    }

    private Manifest? _manifest;
    public Manifest? Manifest
    {
        get => _manifest;
        private set => this.RaiseAndSetIfChanged(ref _manifest, value);
    }
        
    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isInstalled, value);
        }
    }

    private bool _isReadyToInstall;
    private string? _modId = "";

    public bool IsReadyToInstall
    {
        get => _isReadyToInstall;
        private set => this.RaiseAndSetIfChanged(ref _isReadyToInstall, value);
    }

    public MainWindowViewModel()
    {
        SetUp();
    }

    private async void SetUp()
    {
        LoadIcon();
            
        await LoadManifest();
        await LoadSettings();
            
        // TODO fetch details from manifest.
        GameFinders = Manifest.Providers.Select(BaseFinder.Create).ToList();
    }

    private async Task LoadSettings()
    {
        // var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
        // var modDataPath = Path.Join(managerDataPath, _modId);
        // var settingsDocument = await ReadManifest(Path.Join(modDataPath, "settings.xml"));
        // var settingsGameExePath = GetXmlProperty(settingsDocument, "/settings/gameExePath");
        // if (settingsGameExePath != null)
        // {
        //     GameExePath = settingsGameExePath;
        // }
    }
        
    // private async void WriteSettings()
    // {
    //     if (GameExePath == null || _modId == null) return;
    //         
    //     var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
    //     var modDataPath = Path.Join(managerDataPath, _modId);
    //     var settingsPath = Path.Join(modDataPath, "settings.xml");
    //     var settingsDocument = await ReadManifest(settingsPath) ?? new XmlDocument();
    //
    //     var gameExeNode = settingsDocument.SelectSingleNode("/settings/gameExePath");
    //
    //     if (gameExeNode == null)
    //     {
    //         settingsDocument.LoadXml($"<settings><gameExePath>{GameExePath}</gameExePath></settings>");
    //     }
    //     else
    //     {
    //         gameExeNode.InnerText = GameExePath;
    //     }
    //
    //     Directory.CreateDirectory(modDataPath);
    //     settingsDocument.Save(settingsPath);
    // }

    private IEnumerable<string> GetPossibleExeNames()
    {
        return Manifest.Providers.Select(provider => provider.GameExe).Distinct();
    }

    public void DropFiles(List<string> files)
    {
        var exeNames = GetPossibleExeNames();
        
        GameExePath = files.FirstOrDefault(file => exeNames.Contains(Path.GetFileName(file)));
        if (GameExePath == null)
        {
            StatusText = $"Wrong file. Drag {string.Join(" or ", exeNames)} and drop it on this window to install {Manifest.ModTitle}";
        }
    }
        
    private async void LoadIcon()
    {
        if (File.Exists(IconPath))
        {
            Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(IconPath), 400));
        }
    }

    private string GetManifestProperty(XmlDocument? document, string propertyName)
    {
        return GetXmlProperty(document, $"/manifest/{propertyName}") ?? $"[MISSING {propertyName}]";
    }
        
    private string? GetXmlProperty(XmlDocument? document, string path)
    {
        return document?.SelectSingleNode(path)?.InnerText;
    }

    private async Task<Manifest?> ReadManifest(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return await Task.Run(() => JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(path)));
    }
        
    private async Task LoadManifest()
    {
        var manifest = await ReadManifest(ManifestPath);

        if (manifest == null) throw new FileNotFoundException($"Failed to find manifest in {ManifestPath}");

        Manifest = manifest;
    }
}