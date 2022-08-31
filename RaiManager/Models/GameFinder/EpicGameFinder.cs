using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace RaiManager.Models.GameFinder;

public class EpicGameFinder : BaseFinder
{
	private const string RegistryPath = @"SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher";
	private const string RegistryName = "AppDataPath";
	private const string ManifestsFolder = "Manifests";
	private const string ManifestPattern = "*.item";
	private readonly string _epicGameId;

	public override string DisplayName => "Epic";
	public override string Id => "epic";
	
	public EpicGameFinder(string gameExe, string epicGameId): base(gameExe)
	{
		_epicGameId = epicGameId;
	}

	public override string? FindGamePath()
	{
		var key = Registry.LocalMachine.OpenSubKey(RegistryPath);
		var appDataPath = (string)key?.GetValue(RegistryName);
		if (string.IsNullOrEmpty(appDataPath))
		{
			Debug.WriteLine("EGS not found in Registry.");
			return null;
		}

		var manifestsPath = $"{appDataPath}{ManifestsFolder}";
		if (!Directory.Exists(manifestsPath))
		{
			Debug.WriteLine($"EGS manifests folder not found: {manifestsPath}");
			return null;
		}

		var manifestPaths = Directory.GetFiles(manifestsPath, ManifestPattern, SearchOption.TopDirectoryOnly);
		foreach (var manifestPath in manifestPaths)
		{
			var json = File.ReadAllText(manifestPath);
			var epicManifest = JsonConvert.DeserializeObject<EpicManifest>(json);
			if (epicManifest.CatalogItemId.Equals(_epicGameId, StringComparison.OrdinalIgnoreCase) &&
			    IsValidGamePath(epicManifest.InstallLocation))
			{
				return Path.Join(epicManifest.InstallLocation, GameExe);
			}
		}

		Debug.WriteLine("Game not found in EGS.");
		return null;
	}
}