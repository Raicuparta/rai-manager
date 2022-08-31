using Microsoft.Win32;

namespace RaiManager.Models.GameFinder;

public class GogGameFinder: BaseFinder
{
    public override string DisplayName => "GOG";
    public override string Id => "gog";
    
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\GOG.com\Games";

    private readonly string _gogId;
    
    public GogGameFinder(string gameExe, string gogId) : base(gameExe, false)
    {
        _gogId = gogId;
    }

    public override string? FindGamePath()
    {
        var key = Registry.LocalMachine.OpenSubKey(@$"{RegistryPath}\{_gogId}");
        var steamPath = (string)key?.GetValue("exe");
        return steamPath;
    }
}