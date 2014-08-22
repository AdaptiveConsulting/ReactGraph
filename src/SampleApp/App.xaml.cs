using System.Windows;

namespace SampleApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var view = new MainWindow();
            view.DataContext = new MainViewModel();
            view.Show();
        }
    }
}
