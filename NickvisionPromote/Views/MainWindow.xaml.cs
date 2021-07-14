using NickvisionPromote.Extensions;
using NickvisionPromote.Models;
using NickvisionPromote.Models.Configuration;
using NickvisionPromote.Models.Update;
using Ookii.Dialogs.Wpf;
using Syncfusion.Windows.Tools;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NickvisionPromote.Views
{
    public partial class MainWindow : RibbonWindow
    {
        private PhoneNumberDatabase _phoneDatabase;
        private TwilioManager _twilio;
        
        public MainWindow()
        {
            this.ApplyThemeAsync().GetAwaiter().GetResult();
            InitializeComponent();
            RegisterKeyboardShortcuts();
            _phoneDatabase = new PhoneNumberDatabase();
            _twilio = new TwilioManager();
        }

        private void RegisterKeyboardShortcuts()
        {
            var importPhoneNumbersCommand = new RoutedCommand();
            importPhoneNumbersCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(importPhoneNumbersCommand, ImportPhoneNumbers));
            var sendMessageCommand = new RoutedCommand();
            sendMessageCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(sendMessageCommand, SendMessage));
            var removeAllNumbersCommand = new RoutedCommand();
            removeAllNumbersCommand.InputGestures.Add(new KeyGesture(Key.Delete, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(removeAllNumbersCommand, RemoveAllNumbers));
            var syncOptInOutCommand = new RoutedCommand();
            syncOptInOutCommand.InputGestures.Add(new KeyGesture(Key.F5, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(syncOptInOutCommand, SyncOptInsAndOuts));
            var backupDatabaseCommand = new RoutedCommand();
            backupDatabaseCommand.InputGestures.Add(new KeyGesture(Key.B, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(backupDatabaseCommand, BackupDatabase));
            var restoreDatabaseCommand = new RoutedCommand();
            restoreDatabaseCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(restoreDatabaseCommand, RestoreDatabase));
            var aboutCommand = new RoutedCommand();
            aboutCommand.InputGestures.Add(new KeyGesture(Key.F1, ModifierKeys.None));
            CommandBindings.Add(new CommandBinding(aboutCommand, About));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            Ribbon.RibbonState = configuration.IsRibbonCollapsed ? RibbonState.Hide : RibbonState.Normal;
            ListPhoneNumbers.Items.Add("===Phone Numbers===");
            foreach (var phoneNumber in await _phoneDatabase.GetPhoneNumbersAsync())
            {
                ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
            }
            LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
            _twilio.AccountSID = configuration.TwilioAccountSID;
            _twilio.AuthToken = configuration.TwilioAuthToken;
            LblStatusRight.Text = "Incoming Messages Server: http://localhost:5001/sms";
        }

        private void Window_Closing(object sender, CancelEventArgs e) => _twilio.Dispose();

        private async void ImportPhoneNumbers(object sender, RoutedEventArgs e)
        {
            this.ShowOKDialog("Import Phone Numbers", "Special Format Required", "Nickvision Promote reads a text file formated with each phone number on an individual line. An incorrectly formated file may cause unexpected issues.\n\nDepending on how many phone numbers the file contains, this process may take some time.", null, TaskDialogIcon.Warning);
            var openFileDialog = new VistaOpenFileDialog()
            {
                Title = "Import Phone Numbers",
                Filter = "Text File (*.txt)|*.txt"
            };
            if((bool)openFileDialog.ShowDialog(this))
            {
                var successCount = 0;
                new ProgressDialog("Importing phone numbers...", async () => successCount = await _phoneDatabase.ImportPhoneNumbersAsync(openFileDialog.FileName)).ShowDialog();
                ListPhoneNumbers.Items.Clear();
                ListPhoneNumbers.Items.Add("===Phone Numbers===");
                foreach (var phoneNumber in await _phoneDatabase.GetPhoneNumbersAsync())
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                }
                LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
                this.ShowOKDialog("Import Phone Numbers", "Import Report", $"Number of successfully imported phone numbers: {successCount}\n\nSome numbers could have failed to import due to incorrect formatting or being a duplicate in the database.\n\nFormat must be one of the following:\n+1##########\n###-###-####\n##########\n(###) ###-####\n1##########\n1 (###) ###-####");
                var viewFailedResult = this.ShowYesNoDialog("Import Phone Numbers", "View Failed Imports?", "Do you want to view the list of failed imported phone numbers?");
                if(viewFailedResult)
                {
                     _phoneDatabase.OpenFailedImportsInNotepad();
                }
            }
        }

        private void Exit(object sender, RoutedEventArgs e) => Close();

        private async void Settings(object sender, RoutedEventArgs e)
        {
            new SettingsDialog().ShowDialog();
            var configuration = await Configuration.LoadAsync();
            Ribbon.RibbonState = configuration.IsRibbonCollapsed ? RibbonState.Hide : RibbonState.Normal;
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            if (string.IsNullOrEmpty(configuration.TwilioAccountSID))
            {
                this.ShowOKDialog("Error", "Invalid Twilio Account SID", "Twilio Account SID can't be empty. Go to settings to configure your twilio account.", null, TaskDialogIcon.Error);
            }
            else if (string.IsNullOrEmpty(configuration.TwilioAuthToken))
            {
                this.ShowOKDialog("Error", "Invalid Twilio Auth Token", "Twilio Auth Token can't be empty. Go to settings to configure your twilio account.", null, TaskDialogIcon.Error);
            }
            else if (string.IsNullOrEmpty(configuration.TwilioFromNumber))
            {
                this.ShowOKDialog("Error", "Invalid Twilio From Number", "Twilio From Number can't be empty. Go to settings to configure your twilio account.", null, TaskDialogIcon.Error);
            }
            else if (string.IsNullOrEmpty(TxtMessage.Text))
            {
                this.ShowOKDialog("Error", "Invalid Message", "Message can't be empty.", null, TaskDialogIcon.Error);
            }
            else
            {
                USPhoneNumber fromNumber = null;
                try
                {
                    fromNumber = new USPhoneNumber(configuration.TwilioFromNumber);
                }
                catch
                {
                    this.ShowOKDialog("Error", "Invalid Twilio From Number", "Twilio From Number is in an invalid format. Go to settings to configure your twilio account.\n\nFormat must be one of the following:\n+1##########\n###-###-####\n##########\n(###) ###-####\n1##########\n1 (###) ###-####", null, TaskDialogIcon.Error);
                    return;
                }
                (int OptIns, int OptOuts) result = (0, 0);
                new ProgressDialog("Syncing opt ins and outs...", async () => result = await _phoneDatabase.SyncOptInsAndOutsAsync()).ShowDialog();
                var successCount = 0;
                new ProgressDialog("Sending messages...", async () => successCount = await _twilio.SendMessageAsync(_phoneDatabase, configuration.TwilioFromNumber, TxtMessage.Text, TxtPictureURL.Text)).ShowDialog();
                TxtMessage.Text = "";
                TxtPictureURL.Text = "";
                ListPhoneNumbers.Items.Clear();
                ListPhoneNumbers.Items.Add("===Phone Numbers===");
                foreach (var phoneNumber in await _phoneDatabase.GetPhoneNumbersAsync())
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                }
                LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
                this.ShowOKDialog("Send Message", "Message Sending Report", $"Number of messages sent: {successCount}\n\nSync Report:\nNumber of Opt Ins: {result.OptIns}\nNumber of Opt Outs: {result.OptOuts}\n\nA message could have failed to send due to an invalid phone number or an offline phone.\nIf 0 messages were delivered, please check your twilio account information.");
            }
        }

        private async void RemoveAllNumbers(object sender, RoutedEventArgs e)
        {
            var result = this.ShowYesNoDialog("Remove All Numbers", "Remove All Numbers?", "Are you sure you want to remove all phone numbers from the database?", null, TaskDialogIcon.Warning);
            if (result)
            {
                await _phoneDatabase.RemoveAllPhoneNumbersAsync();
                ListPhoneNumbers.Items.Clear();
                ListPhoneNumbers.Items.Add("===Phone Numbers===");
                LblStatusLeft.Text = "Number of Phone Numbers: 0";
            }
        }

        private async void SyncOptInsAndOuts(object sender, RoutedEventArgs e)
        {
            (int OptIns, int OptOuts) result = (0, 0);
            new ProgressDialog("Syncing opt ins and outs...", async () => result = await _phoneDatabase.SyncOptInsAndOutsAsync()).ShowDialog();
            ListPhoneNumbers.Items.Clear();
            ListPhoneNumbers.Items.Add("===Phone Numbers===");
            foreach (var phoneNumber in await _phoneDatabase.GetPhoneNumbersAsync())
            {
                ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
            }
            LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
            this.ShowOKDialog("Sync Opt Ins and Outs", "Sync Report", $"Number of Opt Ins: {result.OptIns}\nNumber of Opt Outs: {result.OptOuts}");
        }

        private void BackupDatabase(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new VistaSaveFileDialog()
            {
                Title = "Backup Database",
                Filter = "Nickvision Promote Backup (*.npromoteb)|*.npromoteb",
                DefaultExt = "npromoteb",
                OverwritePrompt = true,
                 
            };
            if ((bool)saveFileDialog.ShowDialog(this))
            {
                new ProgressDialog("Backuping up database...", async () => await _phoneDatabase.BackupAsync(saveFileDialog.FileName)).ShowDialog();
            }
        }

        private async void RestoreDatabase(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new VistaOpenFileDialog()
            {
                Title = "Restore Database",
                Filter = "Nickvision Promote Backup (*.npromoteb)|*.npromoteb"
            };
            if ((bool)openFileDialog.ShowDialog(this))
            {
                new ProgressDialog("Restoring database...", async () => await _phoneDatabase.RestoreAsync(openFileDialog.FileName)).ShowDialog();
                ListPhoneNumbers.Items.Clear();
                ListPhoneNumbers.Items.Add("===Phone Numbers===");
                foreach (var phoneNumber in await _phoneDatabase.GetPhoneNumbersAsync())
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                }
                LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
            }
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            var updater = new Updater("https://raw.githubusercontent.com/NickvisionTech/NickvisionPromote/main/NickvisionPromote/updateConfig.json", new Version("2021.7.4"));
            new ProgressDialog("Checking for updates...", async () => await updater.CheckForUpdatesAsync()).ShowDialog();
            if (updater.UpdateAvailable)
            {
                var result = this.ShowYesNoDialog("Update Available", "Update Now?", "There is an update available. If you choose to update, Nickvision Promote will download the update automatically and close the application for you to install the update. Please make sure all work is saved before continuing. Are you ready to update?", $"=== V{updater.LatestVersion} Changelog===\n{updater.Changelog}", TaskDialogIcon.Warning);
                if (result)
                {
                    new ProgressDialog("Downloading the update...", async () => await updater.UpdateAsync(this)).ShowDialog();
                }
            }
            else
            {
                this.ShowOKDialog("Error", "No Update Available", "No update is available at this time", null, TaskDialogIcon.Error);
            }
        }

        private void ReportABug(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo() { FileName = "https://github.com/NickvisionTech/NickvisionPromote/issues/new", UseShellExecute = true });

        private void Changelog(object sender, RoutedEventArgs e) => this.ShowOKDialog("Changelog", "What's New?", "- Added support for a progress dialog");

        private void About(object sender, RoutedEventArgs e) => this.ShowOKDialog("About", "About Nickvision Promote", "Version: 2021.7.4\n\nAn easy to use business promotion software", "Built with NickvisionApps Generation 6\nC#, WPF, Syncfusion (R), Icons8 (C)\n\nDeveloper: Nicholas Logozzo\nNickvision (C) 2021");

        private void GitHubRepo(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo() { FileName = "https://github.com/NickvisionTech/NickvisionPromote", UseShellExecute = true });

        private void BuyMeACoffee(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo() { FileName = "https://www.buymeacoffee.com/nlogozzo", UseShellExecute = true });

        private async void RibbonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var configuration = await Configuration.LoadAsync();
            configuration.IsRibbonCollapsed = Ribbon.RibbonState == RibbonState.Hide ? true : false;
            await configuration.SaveAsync();
        }

        private async void TxtSearchPhoneNumber_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var phoneNumbers = await _phoneDatabase.GetPhoneNumbersAsync();
            if (string.IsNullOrEmpty(TxtSearchPhoneNumber.Text))
            {
                ListPhoneNumbers.Items.Clear();
                ListPhoneNumbers.Items.Add("===Phone Numbers===");
                foreach (var phoneNumber in phoneNumbers)
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                }
                LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
            }
            else
            {
                ListPhoneNumbers.Items.Clear();
                foreach (var phoneNumber in phoneNumbers.Where(number => number.NumberString.Contains(TxtSearchPhoneNumber.Text)))
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                }
            }
        }

        private async void AddPhoneNumber(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtSearchPhoneNumber.Text))
            {
                USPhoneNumber phoneNumber = null;
                try
                {
                    phoneNumber = new USPhoneNumber(TxtSearchPhoneNumber.Text);
                }
                catch
                {
                    using var errorDialog = new TaskDialog()
                    {
                        WindowTitle = "Error",
                        MainInstruction = "Invalid Phone Number",
                        MainIcon = TaskDialogIcon.Error,
                        Content = "Unable to add this phone number to the database. The phone number is in an invalid format.\n\nFormat must be one of the following:\n+1##########\n###-###-####\n##########\n(###) ###-####\n1##########\n1 (###) ###-####"
                    };
                    errorDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                    errorDialog.ShowDialog(this);
                    return;
                }
                TxtSearchPhoneNumber.Text = "";
                var result = await _phoneDatabase.AddPhoneNumberAsync(phoneNumber);
                if (result)
                {
                    ListPhoneNumbers.Items.Add(phoneNumber.ReadableString);
                    LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
                }
            }
        }

        private async void RemovePhoneNumber(object sender, RoutedEventArgs e)
        {
            var selectedPhoneNumber = ListPhoneNumbers.SelectedItem as string;
            if (selectedPhoneNumber != null && selectedPhoneNumber != "===Phone Numbers===")
            {
                TxtSearchPhoneNumber.Text = "";
                await _phoneDatabase.DeletePhoneNumberAsync(new USPhoneNumber(selectedPhoneNumber));
                ListPhoneNumbers.Items.Remove(selectedPhoneNumber);
                LblStatusLeft.Text = $"Number of Phone Numbers: {ListPhoneNumbers.Items.Count - 1}";
            }
        }

        private void TxtMessage_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => TxtInputMessage.Hint = $"Message ({TxtMessage.Text.Length}/1600)";
    }
}
