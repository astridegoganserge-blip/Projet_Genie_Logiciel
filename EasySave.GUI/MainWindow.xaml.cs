using System.Windows;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}