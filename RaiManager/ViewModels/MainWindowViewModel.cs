using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using RaiManager.GameFinder;
using ReactiveUI;

namespace RaiManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string IconPath = "./Mod/icon.png";
        private const string ManifestPath = "./Mod/manifest.xml";
        private SteamGameFinder? _steamGameFinder;
        private Bitmap? _icon;
        public Bitmap? Icon
        {
            get => _icon;
            private set => this.RaiseAndSetIfChanged(ref _icon, value);
        }
        
        private string _statusText = "Loading...";
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }
        
        private string? _gameExePath;
        public string? GameExePath
        {
            get => _gameExePath;
            set
            {
                if (value != null && !File.Exists(value))
                {
                    this.RaiseAndSetIfChanged(ref _gameExePath, null);
                }
                else
                {
                    this.RaiseAndSetIfChanged(ref _gameExePath, value);
                }
                CheckIfInstalled();
                WriteSettings();
            }
        }

        private string _gameTitle = "";
        public string GameTitle
        {
            get => _gameTitle;
            private set => this.RaiseAndSetIfChanged(ref _gameTitle, value);
        }

        private string _modTitle = "";
        public string ModTitle
        {
            get => _modTitle;
            private set => this.RaiseAndSetIfChanged(ref _modTitle, value);
        }

        private string _gameExe = "";
        public string GameExe
        {
            get => _gameExe;
            private set => this.RaiseAndSetIfChanged(ref _gameExe, value);
        }

        private bool _requireAdmin;
        
        private bool _isInstalled;
        public bool IsInstalled
        {
            get => _isInstalled;
            private set
            {
                this.RaiseAndSetIfChanged(ref _isInstalled, value);
            }
        }

        private bool _isReadyToInstall;
        private string? _modId = "";

        public bool IsReadyToInstall
        {
            get => _isReadyToInstall;
            private set => this.RaiseAndSetIfChanged(ref _isReadyToInstall, value);
        }

        private string _steamGamePath = "[Steam path]";
        public string SteamGamePath
        {
            get => _steamGamePath;
            private set => this.RaiseAndSetIfChanged(ref _steamGamePath, value);
        }

        private string _gogGamePath = "[GOG path]";
        public string GogGamePath
        {
            get => _gogGamePath;
            private set => this.RaiseAndSetIfChanged(ref _gogGamePath, value);
        }
        
        private string _uwpGamePath = "[UWP path]";
        public string UwpGamePath
        {
            get => _uwpGamePath;
            private set => this.RaiseAndSetIfChanged(ref _uwpGamePath, value);
        }

        public MainWindowViewModel()
        {
            SetUp();
        }

        private async void SetUp()
        {
            LoadIcon();
            
            await LoadManifest();
            await LoadSettings();
            // TODO game title isn't necessarily the game folder.
            // TODO game folder can be fetched from steam manifest file.
            _steamGameFinder ??= new SteamGameFinder(GameExe, GameTitle);
            var uwpGameFinder = new UwpGameFinder("Firewatch.exe", "Firewatch");
            var gogGameFinder = new GogGameFinder("Firewatch.exe", "1459256379");
            SteamGamePath = _steamGameFinder.FindGamePath() ?? "Steam not found";
            UwpGamePath = uwpGameFinder.FindGamePath() ?? "UWP not found";
            GogGamePath = gogGameFinder.FindGamePath() ?? "GOG not found";
            CheckIfInstalled();
        }

        private async Task LoadSettings()
        {
            var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
            var modDataPath = Path.Join(managerDataPath, _modId);
            var settingsDocument = await ReadXmlDocument(Path.Join(modDataPath, "settings.xml"));
            var settingsGameExePath = GetXmlProperty(settingsDocument, "/settings/gameExePath");
            if (settingsGameExePath != null)
            {
                GameExePath = settingsGameExePath;
            }
        }
        
        private async void WriteSettings()
        {
            if (GameExePath == null || _modId == null) return;
            
            var managerDataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RaiManager");
            var modDataPath = Path.Join(managerDataPath, _modId);
            var settingsPath = Path.Join(modDataPath, "settings.xml");
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

            Directory.CreateDirectory(modDataPath);
            settingsDocument.Save(settingsPath);
        }

        public void DropFiles(List<string> files)
        {
            GameExePath = files.FirstOrDefault(file => Path.GetFileName(file) == _gameExe);
            if (GameExePath == null)
            {
                StatusText = $"Wrong file. Drag {GameExe} and drop it on this window to install {ModTitle}";
            }
        }
        
        private async void LoadIcon()
        {
            if (File.Exists(IconPath))
            {
                Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(IconPath), 400));
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
        
        private async Task LoadManifest()
        {
            var document = await ReadXmlDocument(ManifestPath);

            if (document == null) throw new FileNotFoundException($"Failed to find manifest in {ManifestPath}");

            ModTitle = GetManifestProperty(document, "modTitle");
            _modId = GetManifestProperty(document, "id");
            GameTitle = GetManifestProperty(document, "gameTitle");
            GameExe = GetManifestProperty(document, "gameExe");
            _requireAdmin = GetManifestProperty(document, "requireAdmin").Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public async void OnClickInstall()
        {
            try
            {

                var gameDirectory = Path.GetDirectoryName(GameExePath);

                if (gameDirectory == null)
                {
                    throw new DirectoryNotFoundException($"Directory not found for path ${GameExePath}");
                }
                
                var bepinexPath = Path.GetFullPath("./Mod/BepInEx");
                await File.WriteAllTextAsync("./Mod/CopyToGame/doorstop_config.ini", $@"[UnityDoorstop]
enabled=true
targetAssembly={bepinexPath}\core\BepInEx.Preloader.dll");

                CopyFilesRecursively(new DirectoryInfo("./Mod/CopyToGame"), new DirectoryInfo(gameDirectory));

                CheckIfInstalled();
            }
            catch (Exception exception)
            {
                StatusText = exception.Message;
            }
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

            if (_requireAdmin)
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = $"/k \"{GameExePath}\" & exit";
                process.Start();
            }
            else
            {
                Process.Start(GameExePath);
            }
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
            foreach (var dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (var file in source.GetFiles())
            {
                File.Copy(file.FullName, Path.Combine(target.FullName, file.Name), true);
            }
        }
        
        private void CheckIfInstalled()
        {
            if (GameExePath == null)
            {
                IsInstalled = false;
                IsReadyToInstall = false;
                StatusText = GameExe.Length > 0
                    ? $"Drag {GameExe} and drop it on this window to install {ModTitle}.\n\nNote that this tool isn't compatible with the \"sandbox mode\" in the itch.io app."
                    : $"Startup failed. Files may be corrupted. Please note that this tool doesn't support the itch app sandbox mode, since it needs to modify system files.";

                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                {
                    StatusText +=
                        "\n\nWarning: It seems like you are running this app with administrator privileges. This might make it impossible to drag & drop the game exe onto this window. Please close the app and restart it with normal privileges.";
                }

                
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