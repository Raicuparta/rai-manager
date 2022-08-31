using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using RaiManager.Models.GameProviders;
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

    private List<GameProvider> _gameProviders = new();
    public List<GameProvider> GameProviders
    {
        get => _gameProviders;
        set => this.RaiseAndSetIfChanged(ref _gameProviders, value);
    }

    private AppManifest? _manifest;
    public AppManifest? Manifest
    {
        get => _manifest;
        private set => this.RaiseAndSetIfChanged(ref _manifest, value);
    }

    private ManualProvider? _manualProvider;
    private AppSettings? _appSettings;

    public MainWindowViewModel()
    {
        SetUp();
    }

    private async void SetUp()
    {
        LoadIcon();
            
        _manualProvider = new ManualProvider("", false);

        Manifest = await AppManifest.LoadManifest();
        _appSettings = await AppSettings.LoadSettings(Manifest, _manualProvider);

        var gameProviders = Manifest.Providers.Select(GameProvider.Create).ToList();
        gameProviders.Insert(0, _manualProvider);
        GameProviders = gameProviders;
    }

    public void DropFiles(List<string> files)
    {
        if (_manualProvider == null || Manifest == null) return;
        
        var firstExePath = files.FirstOrDefault(file => Path.GetExtension(file) == ".exe");

        if (firstExePath == null)
        {
            throw new FileNotFoundException("None of the files dropped have the exe extension");
        }
        
        _manualProvider.SetGamePath(firstExePath);
        
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