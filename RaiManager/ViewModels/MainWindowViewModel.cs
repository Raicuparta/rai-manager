using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
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
    private AppSettings? _appSettings;

    public MainWindowViewModel()
    {
        SetUp();
    }

    private async void SetUp()
    {
        LoadIcon();
            
        _manualGameFinder = new ManualGameFinder("", false);

        Manifest = await AppManifest.LoadManifest();
        _appSettings = await AppSettings.LoadSettings(Manifest, _manualGameFinder);

        var gameFinders = Manifest.Providers.Select(BaseFinder.Create).ToList();
        gameFinders.Insert(0, _manualGameFinder);
        GameFinders = gameFinders;
    }

    public void DropFiles(List<string> files)
    {
        if (_manualGameFinder == null || Manifest == null) return;
        
        var firstExePath = files.FirstOrDefault(file => Path.GetExtension(file) == ".exe");

        if (firstExePath == null)
        {
            throw new FileNotFoundException("None of the files dropped have the exe extension");
        }
        
        _manualGameFinder.SetGamePath(firstExePath);
        
        AppSettings.WriteSettings(_appSettings, firstExePath, Manifest);
    }
        
    private async void LoadIcon()
    {
        if (File.Exists(IconPath))
        {
            Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(IconPath), 400));
        }
    }
}