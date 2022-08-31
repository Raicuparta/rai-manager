using System.Diagnostics;
using System.IO;

namespace RaiManager.Models.GameFinder;

public class ManualGameFinder: BaseFinder
{
    public override string DisplayName => "Manual";
    public override string Id => "manual";

    public ManualGameFinder(string gameExe, bool requireAdmin) : base(gameExe, requireAdmin)
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