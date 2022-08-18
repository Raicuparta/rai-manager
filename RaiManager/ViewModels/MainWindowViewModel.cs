using System.Collections.Generic;
using System.IO;
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
        
        private string fileNamesText = "Drop files here";
        public string FileNamesText
        {
            get => fileNamesText;
            set => this.RaiseAndSetIfChanged(ref fileNamesText, value);
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
        
        public MainWindowViewModel()
        {
            LoadManifest();
            LoadIcon();
        }
        
        public void DropFiles(IEnumerable<string> files)
        {
            FileNamesText = string.Join(", ", files);
        }
        
        private async void LoadIcon()
        {
            if (File.Exists(iconPath))
            {
                Icon = await Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(iconPath), 400));
                FileNamesText += ";icon found";
            }

            FileNamesText += ";icon not found";
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
        }
    }
}