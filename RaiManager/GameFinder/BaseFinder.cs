using System.IO;

namespace RaiManager.GameFinder
{
    public abstract class BaseFinder
    {
        protected readonly string GameExe;

        protected BaseFinder(string gameExe)
        {
            GameExe = gameExe;
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