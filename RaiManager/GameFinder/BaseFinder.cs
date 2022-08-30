using System.IO;

namespace RaiManager.GameFinder
{
    public abstract class BaseFinder
    {
        protected readonly string GameExe;
        protected readonly string GameFolder;
        private readonly string _managedPath = Path.Combine("Neon White_Data", "Managed");

        protected BaseFinder(string gameExe, string gameFolder)
        {
            GameExe = gameExe;
            GameFolder = gameFolder;
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
    }
}