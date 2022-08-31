using Microsoft.Win32;

namespace RaiManager.Models.GameProviders;

public class GogProvider: GameProvider
{
    public override string DisplayName => "GOG";
    public override string Id => "gog";
    
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\GOG.com\Games";

    private readonly string _gogId;
    
    public GogProvider(string gameExe, bool requireAdmin, string gogId) : base(gameExe, requireAdmin)
    {
        _gogId = gogId;
        Initialize();
    }

    public override string? FindGamePath()
    {
        var key = Registry.LocalMachine.OpenSubKey(@$"{RegistryPath}\{_gogId}");
        var steamPath = (string)key?.GetValue("exe");
        return steamPath;
    }
}