using System.Diagnostics;
using System.IO;

namespace RaiManager.Models.GameProviders;

public class ManualProvider: GameProvider
{
    public override string DisplayName => "Manual";
    public override string Id => "manual";

    public ManualProvider(string gameExe, bool requireAdmin) : base(gameExe, requireAdmin)
    {
        Initialize();
    }

    public override string? FindGamePath()
    {
        return null;
    }

    public bool SetGamePath(string gamePath)
    {
        GameExe = Path.GetFileName(gamePath);
        var gameFolder = Path.GetDirectoryName(gamePath);

        if (gameFolder == null || !IsValidGamePath(gameFolder)) return false;

        GamePath = gamePath;

        return true;
    }
}