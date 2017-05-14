using LocaTracker2.Settings.MaintenanceTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LocaTracker2.Views.Dialogs
{
    public sealed partial class ExecuteMaintenanceTaskDialog : ContentDialog
    {
        public static Regex camelCaseRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        public ExecuteMaintenanceTaskDialog() { this.InitializeComponent(); }

        private void RunOnUI(DispatchedHandler handler)
        {
#pragma warning disable 4014
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
#pragma warning restore 4014
        }

        public void SetTitle(string title) { RunOnUI(() => { Title = title; }); }

        public void SetTitle(BaseMaintenanceTask task) { SetTitle($"Running {camelCaseRegex.Replace(task.GetType().Name, " ")}"); }

        public void SetStatus(string statusText) { RunOnUI(() => { StatusTextBlock.Text = statusText; }); }
    }
}
