﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using Gameloop.Vdf;
using Microsoft.Win32;

namespace RaiManager.Models.GameFinder;

public class SteamGameFinder : BaseFinder
{
    public override string Name => "Steam";

    private const string RegistryPath = @"SOFTWARE\Valve\Steam";
    private const string RegistryName = "SteamPath";
    private const string CommonPath = "steamapps/common";
    private const string LibraryFoldersPath = "steamapps/libraryfolders.vdf";
    private const int MaxLibraryCount = 10;

    private readonly string _steamGameFolder;

    public SteamGameFinder(string gameExe, string steamGameFolder): base(gameExe)
    {
        _steamGameFolder = steamGameFolder;
    }

    public override string? FindGamePath()
    {
        var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
        var steamPath = (string)key?.GetValue(RegistryName);
        if (string.IsNullOrEmpty(steamPath))
        {
            Debug.WriteLine("Steam not found in Registry.");
            return "Steam not found in Registry.";
        }

        var defaultLocation = $"{steamPath}/{CommonPath}/{_steamGameFolder}";
        if (IsValidGamePath(defaultLocation))
        {
            return defaultLocation;
        }

        var libraryFoldersFile = $"{steamPath}/{LibraryFoldersPath}";
        if (!File.Exists(libraryFoldersFile))
        {
            Debug.WriteLine($"Steam library folders file not found: {libraryFoldersFile}");
            return "Steam library folders file not found: {libraryFoldersFile}";
        }

        var libraryFoldersContent = File.ReadAllText(libraryFoldersFile);
        var libraryFoldersVdf = VdfConvert.Deserialize(libraryFoldersContent);
        for (var i = 0; i < MaxLibraryCount; i++)
        {
            var libraryName = i.ToString();

            var libraryBlock = libraryFoldersVdf.Value[libraryName];

            if (libraryBlock is null)
            {
                continue;
            }

            var token = libraryBlock.Children().First().ToString();
            var libraryPath = token.Substring(8, token.Length - 9);
            if (string.IsNullOrEmpty(libraryPath))
            {
                continue;
            }

            var gamePath = $"{libraryPath}/{CommonPath}/{_steamGameFolder}";
            if (IsValidGamePath(gamePath))
            {
                return gamePath;
            }
        }

        Debug.WriteLine("Game not found in Steam.");
        return "Game not found in Steam.";
    }
}