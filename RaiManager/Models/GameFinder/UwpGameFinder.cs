using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace RaiManager.Models.GameFinder;

public class UwpGameFinder : BaseFinder
{
    public override string DisplayName => "Xbox / Game Pass";
    public override string Id => "xbox";

    private const string RegistryPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";
    private readonly string _uwpDisplayName;

    public UwpGameFinder(string gameExe, bool requireAdmin, string uwpDisplayName): base(gameExe, requireAdmin)
    {
        _uwpDisplayName = uwpDisplayName;
        Initialize();
    }

    public override string? FindGamePath()
    {
        var appPackages = Registry.CurrentUser.OpenSubKey(RegistryPath);

        var gamePath = "";
        foreach(var appPackageName in appPackages?.GetSubKeyNames())
        {
            var appPackageKey = appPackages.OpenSubKey(appPackageName);
            var packageDisplayName = (string)appPackageKey.GetValue("DisplayName");

            // TODO "GameFolder" should actually be a specific UwpDisplayName field in the mod manifest.
            if(!String.IsNullOrEmpty(packageDisplayName) && packageDisplayName.Contains(_uwpDisplayName))
            {
                gamePath = (string)appPackageKey.GetValue("PackageRootFolder");
                break;
            }
        }

        if (IsValidGamePath(gamePath))
        {
            return Path.Join(gamePath, GameExe);
        }

        Debug.WriteLine("Game not found in UWP.");
        return null;
    }
}