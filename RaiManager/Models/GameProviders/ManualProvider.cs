using System.Diagnostics;
using System.IO;

namespace RaiManager.Models.GameProviders;

public class ManualProvider: GameProvider
{
    public override string DisplayName => "Manual";
    public override string Id => "manual";
    public override string Description => "Drag the game exe and drop it on this window.";

    public ManualProvider(string gameExe, bool requireAdmin) : base(gameExe, requireAdmin)
    {
        Initialize();
    }

    public override string? FindGamePath()
    {
        return null;
    }

    public void SetGamePath(string gamePath)
    {
        Debug.WriteLine(gamePath);
        GameExe = Path.GetFileName(gamePath);
        GamePath = gamePath;
    }
}