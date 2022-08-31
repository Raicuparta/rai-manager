using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using RaiManager.Models.GameFinder;
using RaiManager.Models.Manifest;
using RaiManager.Models.Settings;
using ReactiveUI;

namespace RaiManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string IconPath = "./Mod/icon.png";
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

    private AppManifest? _manifest;
    public AppManifest? Manifest
    {
        get => _manifest;
        private set => this.RaiseAndSetIfChanged(ref _manifest, value);
    }

    private ManualGameFinder? _manualGameFinder;

    public MainWindowViewModel()
    {
        SetUp();
    }

    private async void SetUp()
    {
        LoadIcon();
            
        _manualGameFinder = new ManualGameFinder("", false);

        Manifest = await AppManifest.LoadManifest();
        await AppSettings.LoadSettings(Manifest, _manualGameFinder);

        var gameFinders = Manifest.Providers.Select(BaseFinder.Create).ToList();
        gameFinders.Insert(0, _manualGameFinder);
        GameFinders = gameFinders;
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

    public void DropFiles(List<string> files)
    {
        if (_manualGameFinder == null) return;
        
        var firstExe = files.FirstOrDefault(file => Path.GetExtension(file) == ".exe");

        if (firstExe == null)
        {
            throw new FileNotFoundException("None of the files dropped have the exe extension");
        }
        
        _manualGameFinder.SetGamePath(firstExe);
    }
        
    private async void LoadIcon()
    {
        if (File.Exists(IconPath))
        {
            Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(IconPath), 400));
        }
    }
}