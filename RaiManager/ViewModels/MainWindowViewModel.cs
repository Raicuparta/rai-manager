using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace RaiManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string iconPath = "./Mod/icon.png";
        private const string manifestPath = "./Mod/manifest.xml";
        private Bitmap? icon;
        public Bitmap? Icon
        {
            get => icon;
            private set => this.RaiseAndSetIfChanged(ref icon, value);
        }
        
        private string statusText = "Loading...";
        public string StatusText
        {
            get => statusText;
            set => this.RaiseAndSetIfChanged(ref statusText, value);
        }
        
        private string? gameExePath;
        public string? GameExePath
        {
            get => gameExePath;
            set
            {
                if (value != null && !File.Exists(value))
                {
                    this.RaiseAndSetIfChanged(ref gameExePath, null);
                }
                else
                {
                    this.RaiseAndSetIfChanged(ref gameExePath, value);
                }
                CheckIfInstalled();
                WriteSettings();
            }
        }

        private string gameTitle = "Game Title";
        public string GameTitle
        {
            get => gameTitle;
            private set => this.RaiseAndSetIfChanged(ref gameTitle, value);
        }

        private string modTitle = "Mod Title";
        public string ModTitle
        {
            get => modTitle;
            private set => this.RaiseAndSetIfChanged(ref modTitle, value);
        }

        private string gameExe = "Game.exe";
        public string GameExe
        {
            get => gameExe;
            private set => this.RaiseAndSetIfChanged(ref gameExe, value);
        }

        private string installButtonText = "Install";
        public string InstallButtonText
        {
            get => installButtonText;
            private set => this.RaiseAndSetIfChanged(ref installButtonText, value);
        }
        
        private bool isInstalled;
        public bool IsInstalled
        {
            get => isInstalled;
            private set
            {
                this.RaiseAndSetIfChanged(ref isInstalled, value);
            }
        }

        private bool isReadyToInstall;
        public bool IsReadyToInstall
        {
            get => isReadyToInstall;
            private set => this.RaiseAndSetIfChanged(ref isReadyToInstall, value);
        }

        public MainWindowViewModel()
        {
            LoadManifest();
            LoadIcon();
            LoadSettings();
        }

        private async void LoadSettings()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDocument = await ReadXmlDocument(Path.Join(appDataPath, "RaiManager/settings.xml"));
            GameExePath = GetXmlProperty(settingsDocument, "/settings/gameExePath");
        }
        
        private async void WriteSettings()
        {
            if (GameExePath == null) return;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var raiManagerDataPath = Path.Join(appDataPath, "RaiManager");
            var settingsPath = Path.Join(raiManagerDataPath, "settings.xml");
            var settingsDocument = await ReadXmlDocument(settingsPath) ?? new XmlDocument();

            var gameExeNode = settingsDocument.SelectSingleNode("/settings/gameExePath");

            if (gameExeNode == null)
            {
                settingsDocument.LoadXml($"<settings><gameExePath>{GameExePath}</gameExePath></settings>");
            }
            else
            {
                gameExeNode.InnerText = GameExePath;
            }

            Directory.CreateDirectory(raiManagerDataPath);
            settingsDocument.Save(settingsPath);
        }

        public void DropFiles(List<string> files)
        {
            GameExePath = files.FirstOrDefault(file => Path.GetFileName(file) == gameExe);
            if (GameExePath == null)
            {
                StatusText = $"Wrong file. Drag {GameExe} and drop it on this window to install {ModTitle}";
            }
        }
        
        private async void LoadIcon()
        {
            if (File.Exists(iconPath))
            {
                Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(iconPath), 400));
            }
        }

        private string GetManifestProperty(XmlDocument? document, string propertyName)
        {
            return GetXmlProperty(document, $"/manifest/{propertyName}") ?? $"[MISSING {propertyName}]";
        }
        
        private string? GetXmlProperty(XmlDocument? document, string path)
        {
            return document?.SelectSingleNode(path)?.InnerText;
        }

        private async Task<XmlDocument?> ReadXmlDocument(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            var text = await File.ReadAllTextAsync(path);
            var document = new XmlDocument();
            document.LoadXml(text);
            return document;
        }
        
        private async void LoadManifest()
        {
            var document = await ReadXmlDocument(manifestPath);

            if (document == null) throw new FileNotFoundException($"Failed to find manifest in {manifestPath}");

            ModTitle = GetManifestProperty(document, "modTitle");
            GameTitle = GetManifestProperty(document, "gameTitle");
            GameExe = GetManifestProperty(document, "gameExe");
        }

        public async void OnClickInstall()
        {
            var gameDirectory = Path.GetDirectoryName(GameExePath);
            var bepinexPath = Path.GetFullPath("./Mod/BepInEx");
            await File.WriteAllTextAsync("./Mod/CopyToGame/doorstop_config.ini", $@"[UnityDoorstop]
enabled=true
targetAssembly={bepinexPath}\core\BepInEx.Preloader.dll");
            
            CopyFilesRecursively(new DirectoryInfo("./Mod/CopyToGame"), new DirectoryInfo(gameDirectory));
            
            // File.Copy("./Mod/CopyToGame/doorstop_config.ini", Path.Join(gameDirectory, "doorstop_config.ini"));
            // File.Copy("./Mod/CopyToGame/winhttp.dll", Path.Join(gameDirectory, "winhttp.dll"));
            CheckIfInstalled();
        }
        
        public void OnClickUninstall()
        {
            var gameDirectory = Path.GetDirectoryName(GameExePath);
            File.Delete(Path.Join(gameDirectory, "doorstop_config.ini"));
            File.Delete(Path.Join(gameDirectory, "winhttp.dll"));
            CheckIfInstalled();
        }

        public void OnClickStart()
        {
            if (GameExePath == null)
            {
                return;
            }
            Process.Start(GameExePath);
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
            foreach (var dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
        
        private void CheckIfInstalled()
        {
            if (GameExePath == null)
            {
                IsInstalled = false;
                IsReadyToInstall = false;
                StatusText = $"Drag {GameExe} and drop it on this window to install {ModTitle}";
                return;
            }

            var gameDirectory = Path.GetDirectoryName(GameExePath);
            var doorstopConfigPath = Path.Join(gameDirectory, "doorstop_config.ini");
            var winhttpPath = Path.Join(gameDirectory, "winhttp.dll");
            // TODO: also check if doorstop config path is correct.
            IsInstalled = File.Exists(winhttpPath) && File.Exists(doorstopConfigPath);
            IsReadyToInstall = !IsInstalled;

            StatusText = IsInstalled
                ? $"Detected that {ModTitle} is installed. You can start the game from here, or just run it normally (you don't need to run the game through here every time)."
                : $"{GameTitle} found in {GameExePath}";
        }
    }
}