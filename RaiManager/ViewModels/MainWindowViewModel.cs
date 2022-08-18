using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace RaiManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            LoadIcon();
        }
        
        private const string iconPath = "./icon.png";
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
    }
}