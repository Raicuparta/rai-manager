using Microsoft.Win32;

namespace RaiManager.Models.GameFinder;

public class GogGameFinder: BaseFinder
{
    public override string Name => "GOG";
    
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\GOG.com\Games";

    private readonly string _gogId;
    
    public GogGameFinder(string gameExe, string gogId) : base(gameExe)
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