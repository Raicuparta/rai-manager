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
            CheckIfInstalled();
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
            
        CheckIfInstalled();
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

    public async void OnClickInstall()
    {
        try
        {

            var gameDirectory = Path.GetDirectoryName(GameExePath);

            if (gameDirectory == null)
            {
                throw new DirectoryNotFoundException($"Directory not found for path ${GameExePath}");
            }
                
            var bepinexPath = Path.GetFullPath("./Mod/BepInEx");
            await File.WriteAllTextAsync("./Mod/CopyToGame/doorstop_config.ini", $@"[UnityDoorstop]
enabled=true
targetAssembly={bepinexPath}\core\BepInEx.Preloader.dll");

            CopyFilesRecursively(new DirectoryInfo("./Mod/CopyToGame"), new DirectoryInfo(gameDirectory));

            CheckIfInstalled();
        }
        catch (Exception exception)
        {
            StatusText = exception.Message;
        }
    }
        
    public void OnClickUninstall()
    {
        var gameDirectory = Path.GetDirectoryName(GameExePath);
        File.Delete(Path.Join(gameDirectory, "doorstop_config.ini"));
        File.Delete(Path.Join(gameDirectory, "winhttp.dll"));
        CheckIfInstalled();
    }

    public void OnClickStart()
    {
        if (GameExePath == null)
        {
            return;
        }

        if (Manifest.RequireAdmin)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/k \"{GameExePath}\" & exit";
            process.Start();
        }
        else
        {
            Process.Start(GameExePath);
        }
    }

    public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
        foreach (var dir in source.GetDirectories())
        {
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
        }
        foreach (var file in source.GetFiles())
        {
            File.Copy(file.FullName, Path.Combine(target.FullName, file.Name), true);
        }
    }
        
    private void CheckIfInstalled()
    {
        if (GameExePath == null)
        {
            var exeNames = GetPossibleExeNames();
            
            IsInstalled = false;
            IsReadyToInstall = false;
            StatusText = Manifest != null
                ? $"Drag {string.Join(" or ", exeNames)} and drop it on this window to install {Manifest.ModTitle}.\n\nNote that this tool isn't compatible with the \"sandbox mode\" in the itch.io app."
                : $"Startup failed. Files may be corrupted. Please note that this tool doesn't support the itch app sandbox mode, since it needs to modify system files.";

            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                StatusText +=
                    "\n\nWarning: It seems like you are running this app with administrator privileges. This might make it impossible to drag & drop the game exe onto this window. Please close the app and restart it with normal privileges.";
            }

                
            return;
        }

        var gameDirectory = Path.GetDirectoryName(GameExePath);
        var doorstopConfigPath = Path.Join(gameDirectory, "doorstop_config.ini");
        var winhttpPath = Path.Join(gameDirectory, "winhttp.dll");
        // TODO: also check if doorstop config path is correct.
        IsInstalled = File.Exists(winhttpPath) && File.Exists(doorstopConfigPath);
        IsReadyToInstall = !IsInstalled;

        StatusText = IsInstalled
            ? $"Detected that {Manifest.ModTitle} is installed. You can start the game from here, or just run it normally (you don't need to run the game through here every time)."
            : $"{Manifest.GameTitle} found in {GameExePath}";
    }
}