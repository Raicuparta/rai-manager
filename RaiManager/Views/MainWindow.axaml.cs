using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using RaiManager.ViewModels;

namespace RaiManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            AddHandler(DragDrop.DropEvent, Drop);
        }

        private void Drop(object? sender, DragEventArgs dragEvent)
        {
            if (DataContext == null) return;

            var fileNames = dragEvent.Data.GetFileNames();

            if (fileNames == null) return;

            ((MainWindowViewModel) DataContext).DropFiles(fileNames.ToList());
        }
    }
}