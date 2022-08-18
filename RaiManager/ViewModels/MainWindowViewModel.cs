using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Media.Imaging;
using Avalonia.X11;
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
                this.RaiseAndSetIfChanged(ref gameExePath, value);
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

        public MainWindowViewModel()
        {
            LoadManifest();
            LoadIcon();
        }
        
        public void DropFiles(List<string> files)
        {
            StatusText = string.Join(", ", files);
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

        private string GetManifestProperty(XmlDocument document, string propertyName)
        {
            return document.SelectSingleNode($"/manifest/{propertyName}")?.InnerText ?? $"[MISSING {propertyName}]";
        }
        
        private async void LoadManifest()
        {
            var text = await File.ReadAllTextAsync(manifestPath);
            var document = new XmlDocument();
            document.LoadXml(text);

            ModTitle = GetManifestProperty(document, "modTitle");
            GameTitle = GetManifestProperty(document, "gameTitle");
            GameExe = GetManifestProperty(document, "gameExe");

            StatusText = $"Drag {GameExe} and drop it on this window to install {ModTitle}";
        }

        private async void OnClickInstall()
        {
            
        }
    }
}