using System.Collections.Generic;
using ReactiveUI;

namespace RaiManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string fileNamesText = "Drop files here";
        public string Greeting
        {
            get => fileNamesText;
            set => this.RaiseAndSetIfChanged(ref fileNamesText, value);
        }

        public void DropFiles(IEnumerable<string> files)
        {
            Greeting = string.Join(", ", files);
        }
    }
}