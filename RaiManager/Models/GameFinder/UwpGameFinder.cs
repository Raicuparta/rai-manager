using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace RaiManager.GameFinder;

public class UwpGameFinder : BaseFinder
{
    private const string RegistryPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";
    private readonly string _uwpDisplayName;

    public UwpGameFinder(string gameExe, string uwpDisplayName): base(gameExe)
    {
        _uwpDisplayName = uwpDisplayName;
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
            return gamePath;
        }

        Debug.WriteLine("Game not found in UWP.");
        return null;
    }
}