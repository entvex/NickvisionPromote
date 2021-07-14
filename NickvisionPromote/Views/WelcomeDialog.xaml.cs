using NickvisionPromote.Extensions;
using NickvisionPromote.Models.Configuration;
using Syncfusion.Windows.Tools.Controls;
using System.Windows;

namespace NickvisionPromote.Views
{
    public partial class WelcomeDialog : RibbonWindow
    {
        public WelcomeDialog()
        {
            this.ApplyThemeAsync().GetAwaiter().GetResult();
            InitializeComponent();
        }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            BtnLight.IsChecked = configuration.Theme == AppTheme.Light;
            BtnDark.IsChecked = configuration.Theme == AppTheme.Dark;
            BtnFluentLight.IsChecked = configuration.Theme == AppTheme.FluentLight;
            BtnFluentDark.IsChecked = configuration.Theme == AppTheme.FluentDark;
            ChkRibbonCollapsed.IsChecked = configuration.IsRibbonCollapsed;
            TxtTwilioAccountSID.Text = configuration.TwilioAccountSID;
            TxtTwilioAuthToken.Text = configuration.TwilioAuthToken;
            TxtTwilioFromNumber.Text = configuration.TwilioFromNumber;
            TxtStartMessage.Text = configuration.StartMessage;
            TxtStopMessage.Text = configuration.StopMessage;
        }

        private void Cancel(object sender, RoutedEventArgs e) => Close();

        private async void Finish(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            if ((bool)BtnLight.IsChecked)
            {
                configuration.Theme = AppTheme.Light;
            }
            else if ((bool)BtnDark.IsChecked)
            {
                configuration.Theme = AppTheme.Dark;
            }
            else if ((bool)BtnFluentLight.IsChecked)
            {
                configuration.Theme = AppTheme.FluentLight;
            }
            else if ((bool)BtnFluentDark.IsChecked)
            {
                configuration.Theme = AppTheme.FluentDark;
            }
            configuration.IsRibbonCollapsed = (bool)ChkRibbonCollapsed.IsChecked;
            configuration.TwilioAccountSID = TxtTwilioAccountSID.Text;
            configuration.TwilioAuthToken = TxtTwilioAuthToken.Text;
            configuration.TwilioFromNumber = TxtTwilioFromNumber.Text;
            configuration.StartMessage = TxtStartMessage.Text;
            configuration.StopMessage = TxtStopMessage.Text;
            await configuration.SaveAsync();
            System.Windows.Forms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}
