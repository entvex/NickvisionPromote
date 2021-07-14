using NickvisionPromote.Models.Configuration;
using Ookii.Dialogs.Wpf;
using Syncfusion.SfSkinManager;
using System.Threading.Tasks;
using System.Windows;

namespace NickvisionPromote.Extensions
{
    public static class WindowExtensions
    {
        public static async Task ApplyThemeAsync(this Window window)
        {
            var configuration = await Configuration.LoadAsync();
            if (configuration.Theme == AppTheme.Light)
            {
                SfSkinManager.SetTheme(window, new Theme("Office2019White"));
            }
            else if (configuration.Theme == AppTheme.Dark)
            {
                SfSkinManager.SetTheme(window, new Theme("Office2019Black"));
            }
            else if (configuration.Theme == AppTheme.FluentLight)
            {
                SfSkinManager.SetTheme(window, new Theme("FluentLight"));
            }
            else if (configuration.Theme == AppTheme.FluentDark)
            {
                SfSkinManager.SetTheme(window, new Theme("FluentDark"));
            }
        }

        public static void ShowOKDialog(this Window parent, string title, string instruction, string content, string footer = null, TaskDialogIcon icon = TaskDialogIcon.Information)
        {
            using var dialog = new TaskDialog()
            {
                WindowTitle = title,
                MainInstruction = instruction,
                MainIcon = icon,
                Content = content,
                ExpandedInformation = footer,
                ExpandFooterArea = true
            };
            dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            dialog.ShowDialog(parent);
        }

        public static bool ShowYesNoDialog(this Window parent, string title, string instruction, string content, string footer = null, TaskDialogIcon icon = TaskDialogIcon.Information)
        {
            using var dialog = new TaskDialog()
            {
                WindowTitle = title,
                MainInstruction = instruction,
                MainIcon = icon,
                Content = content,
                ExpandedInformation = footer,
                ExpandFooterArea = true
            };
            var yesButton = new TaskDialogButton(ButtonType.Yes);
            dialog.Buttons.Add(yesButton);
            dialog.Buttons.Add(new TaskDialogButton(ButtonType.No));
            return dialog.ShowDialog(parent) == yesButton;
        }
    }
}
