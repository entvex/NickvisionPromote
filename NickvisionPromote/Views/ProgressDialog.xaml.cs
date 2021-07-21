using NickvisionPromote.Extensions;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace NickvisionPromote.Views
{
    public partial class ProgressDialog : RibbonWindow
    {
        private Func<Task> _action;

        public ProgressDialog(string desciption, Func<Task> action)
        {
            this.ApplyThemeAsync().GetAwaiter().GetResult();
            InitializeComponent();
            _action = action;
            BusyIndicator.Header = $"\n{desciption}";
        }

        private async void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            await _action();
            Close();
        }
    }
}