using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RaiManager.Models.Manifest;
using ReactiveUI;

namespace RaiManager.Models.GameProviders;

public abstract class GameProvider: ReactiveObject
{
    public abstract string DisplayName { get; }
    public abstract string Id { get; }

    private string? _gamePath;
    public string? GamePath
    {
        get => _gamePath;
        protected set
        {
            this.RaiseAndSetIfChanged(ref _gamePath, value);
            CheckIfInstalled();
        }
    }

    private bool _isInstalled;
    public bool IsInstalled
    {
        get => _isInstalled;
        private set => this.RaiseAndSetIfChanged(ref _isInstalled, value);
    }
    
    private bool _isReadyToInstall;
    public bool IsReadyToInstall
    {
        get => _isReadyToInstall;
        private set => this.RaiseAndSetIfChanged(ref _isReadyToInstall, value);
    }
    
    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        private set => this.RaiseAndSetIfChanged(ref _isAvailable, value);
    }

    private readonly bool _requireAdmin;

    private string? _gameExe;
    public string? GameExe
    {
        get => _gameExe;
        protected set => this.RaiseAndSetIfChanged(ref _gameExe, value);
    }
    
    private string? _errorText;
    public string? ErrorText
    {
        get => _errorText;
        private set => this.RaiseAndSetIfChanged(ref _errorText, value);
    }

    protected GameProvider(string gameExe, bool requireAdmin)
    {
        GameExe = gameExe;
        _requireAdmin = requireAdmin;
    }
    
    public static GameProvider Create(ProviderManifest providerManifest)
    {
        var gameExe = providerManifest.GameExe;
        var gameIdentifier = providerManifest.GameIdentifier;
        var providerId = providerManifest.ProviderId;
        var requireAdmin = providerManifest.RequireAdmin;

        GameProvider gameProvider = providerId switch
        {
            "steam" => new SteamProvider(gameExe, requireAdmin, gameIdentifier),
            "epic" => new EpicProvider(gameExe, requireAdmin, gameIdentifier),
            "gog" => new GogProvider(gameExe, requireAdmin, gameIdentifier),
            "xbox" => new XboxProvider(gameExe, requireAdmin, gameIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(providerManifest), providerId, null)
        };
        
        return gameProvider;
    }

    protected void Initialize()
    {
        GamePath = FindGamePath();
    }

    public abstract string? FindGamePath();

    protected bool IsValidGamePath(string gamePath) =>
        !string.IsNullOrEmpty(gamePath) &&
        Directory.Exists(gamePath) &&
        (HasGameFiles(gamePath));

    private bool HasGameFiles(string gamePath) => 
        Directory.Exists(Path.Combine(gamePath,
            Path.Combine($"{Path.GetFileNameWithoutExtension(GameExe)}_Data", "Managed"))) && 
        File.Exists(Path.Combine(gamePath, GameExe));
    
    public async void OnClickInstall()
    {
        try
        {
            var gameDirectory = Path.GetDirectoryName(GamePath);

            if (gameDirectory == null)
            {
                throw new DirectoryNotFoundException($"Directory not found for path ${GamePath}");
            }

            await File.WriteAllTextAsync("./Mod/CopyToGame/doorstop_config.ini", $@"[UnityDoorstop]
enabled=true
{GetDoorstopIniTargetAssemblyConfig()}
ignoreDisableSwitch=true", Encoding.Unicode);

            CopyFilesRecursively(new DirectoryInfo("./Mod/CopyToGame"), new DirectoryInfo(gameDirectory));

            CheckIfInstalled();
        }
        catch (Exception exception)
        {
            ErrorText = $"Error installing mod: {exception}";
        }
    }

    private static string GetDoorstopIniTargetAssemblyConfig()
    {
        var bepinexPath = Path.GetFullPath("./Mod/BepInEx");
        return $@"targetAssembly={bepinexPath}\core\BepInEx.Preloader.dll";
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

    private bool IsDoorstopInstalled()
    {
        try
        {
            var gameDirectory = Path.GetDirectoryName(GamePath);
            var doorstopConfigPath = Path.Join(gameDirectory, "doorstop_config.ini");

            if (!File.Exists(doorstopConfigPath)) return false;

            var doorstopConfigText = File.ReadAllText(doorstopConfigPath);
            return doorstopConfigText.Contains(GetDoorstopIniTargetAssemblyConfig());
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Failed to check if doorstop is installed. {exception}");
            return false;
        }
    }

    private bool IsWinhttpInstalled()
    {
        var winhttpPath = Path.Join(Path.GetDirectoryName(GamePath), "winhttp.dll");
        return File.Exists(winhttpPath);
    }
    
    private void CheckIfInstalled()
    {
        IsAvailable = GamePath != null;
        
        if (GamePath == null)
        {
            IsInstalled = false;
            IsReadyToInstall = false;

            return;
        }
        
        IsInstalled = IsWinhttpInstalled() && IsDoorstopInstalled();
        IsReadyToInstall = !IsInstalled;
    }
    
    public void OnClickStart()
    {
        if (GamePath == null)
        {
            return;
        }

        if (_requireAdmin)
        {
            // TODO handle case where user rejects. It throws an exception.
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/k \"{GamePath}\" & exit";
            process.Start();
        }
        else
        {
            Process.Start(GamePath);
        }
    }
    
    public void OnClickUninstall()
    {
        var gameDirectory = Path.GetDirectoryName(GamePath);
        File.Delete(Path.Join(gameDirectory, "doorstop_config.ini"));
        File.Delete(Path.Join(gameDirectory, "winhttp.dll"));
        CheckIfInstalled();
    }

    public void OnClickOpenFolder()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetDirectoryName(GamePath),
            UseShellExecute = true,
            Verb = "open"
        });
    }
}