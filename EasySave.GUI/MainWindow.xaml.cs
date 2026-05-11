using System.Windows;

namespace EasySave.GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // EN: DataContext is set by App.xaml.cs with the shared BackupManager
        }
    }
}