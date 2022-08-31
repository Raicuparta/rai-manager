using System;
using System.IO;
using RaiManager.Models.Manifest;

namespace RaiManager.Models.GameFinder;

public abstract class BaseFinder
{
    public string? GamePath => FindGamePath();
    public abstract string DisplayName { get; }
    public abstract string Id { get; }

    protected readonly string GameExe;

    protected BaseFinder(string gameExe)
    {
        GameExe = gameExe;
    }
    
    public static BaseFinder Create(ProviderManifest providerManifest)
    {
        var gameExe = providerManifest.GameExe;
        var gameIdentifier = providerManifest.GameIdentifier;
        var providerId = providerManifest.ProviderId;

        return providerId switch
        {
            "steam" => new SteamGameFinder(gameExe, gameIdentifier),
            "epic" => new EpicGameFinder(gameExe, gameIdentifier),
            "gog" => new GogGameFinder(gameExe, gameIdentifier),
            "xbox" => new UwpGameFinder(gameExe, gameIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(providerManifest), providerId, null)
        };
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