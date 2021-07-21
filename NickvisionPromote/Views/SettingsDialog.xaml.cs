using NickvisionPromote.Extensions;
using NickvisionPromote.Models.Configuration;
using Syncfusion.Windows.PropertyGrid;
using Syncfusion.Windows.Tools.Controls;
using System.Windows;

namespace NickvisionPromote.Views
{
    public partial class SettingsDialog : RibbonWindow
    {
        public SettingsDialog()
        {
            this.ApplyThemeAsync().GetAwaiter().GetResult();
            InitializeComponent();
        }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            PropertyGrid.SelectedObject = configuration;
        }

        private async void PropertyGrid_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var configuration = (Configuration)PropertyGrid.SelectedObject;
            await configuration.SaveAsync();
        }

        private void BtnClose(object sender, RoutedEventArgs e) => Close();
    }
}
