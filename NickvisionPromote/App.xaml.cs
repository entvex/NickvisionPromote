using NickvisionPromote.Models.Configuration;
using NickvisionPromote.Views;
using Syncfusion.Licensing;
using System.Windows;

namespace NickvisionPromote
{
    public partial class App : Application
    {
        public App()
        {
            
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            if (configuration.IsFirstTimeOpen)
            {
                configuration.IsFirstTimeOpen = false;
                await configuration.SaveAsync();
                new WelcomeDialog().ShowDialog();
                return;
            }
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
