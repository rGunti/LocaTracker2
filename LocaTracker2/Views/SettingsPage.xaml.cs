using LocaTracker2.Gps;
using LocaTracker2.Logging.ETW;
using LocaTracker2.Settings;
using LocaTracker2.Settings.MaintenanceTask;
using LocaTracker2.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LocaTracker2.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Type navigatedFrom;

        public SettingsPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(navigatedFrom);
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPage_BackRequested;
            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigatedFrom = e.Parameter.GetType();
        }

        private void ExecuteMaintenanceTask(BaseMaintenanceTask maintTask)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ExecuteMaintenanceTaskDialog dialog = new ExecuteMaintenanceTaskDialog();
            dialog.SetTitle(maintTask);
            dialog.SetStatus($"Initializing {ExecuteMaintenanceTaskDialog.camelCaseRegex.Replace(maintTask.GetType().Name, " ")}...");

            maintTask.OnReportStatus += (status) => {
                dialog.SetStatus(status);
            };

            dialog.ShowAsync();

            Task.Run(() => {
                maintTask.Execute();
                Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                    dialog.Hide();
                });

                string resultText;
                if (maintTask.TaskResult == MaintenanceTaskResult.Success) {
                    resultText = "Task completed successfully.";
                } else if (maintTask.TaskResult == MaintenanceTaskResult.Failed) {
                    resultText = "Failed to complete task.";
                } else {
                    resultText = "Unknown State upon Task completion.";
                }
                new MessageDialog(
                    resultText,
                    ExecuteMaintenanceTaskDialog.camelCaseRegex.Replace(maintTask.GetType().Name, " ")
                ).ShowAsync();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void ExecuteMaintRecalculation_Click(object sender, RoutedEventArgs e)
        {
            ExecuteMaintenanceTask(new DistanceRecalculationTask());
        }

        private void UseImperialUnitsToggleSwitch_Loading(FrameworkElement sender, object args)
        {
            ((ToggleSwitch)sender).IsOn = UnitSettingsReader.Instance.UseImperialUnits;
        }

        private void UseImperialUnitsToggleSwitch_LostFocus(object sender, RoutedEventArgs e)
        {
            UnitSettingsReader.Instance.UseImperialUnits = ((ToggleSwitch)sender).IsOn;
        }

        private void RecordingMinSpeedTextBox_Loading(FrameworkElement sender, object args)
        {
            ((TextBox)sender).Text = Math.Floor(GpsUtilities.ConvertMPStoKMH(RecordingSettingsReader.Instance.MinSpeed)).ToString();
        }

        private void RecordingMinSpeedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(((TextBox)sender).Text, out value)) {
                RecordingSettingsReader.Instance.MinSpeed = GpsUtilities.ConvertKMHtoMPS(value);
            } else {
                RecordingMinSpeedTextBox_Loading(sender as FrameworkElement, null);
            }
        }

        private void RecordingMaxAccuracyTextBox_Loading(FrameworkElement sender, object args)
        {
            ((TextBox)sender).Text = RecordingSettingsReader.Instance.MaxAccuracy.ToString();
        }

        private void RecordingMaxAccuracyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(((TextBox)sender).Text, out value)) {
                RecordingSettingsReader.Instance.MaxAccuracy = value;
            } else {
                RecordingMaxAccuracyTextBox_Loading(sender as FrameworkElement, null);
            }
        }

        private void DEBUG_RecordingTripIDTextBox_Loading(FrameworkElement sender, object args)
        {
            ((TextBox)sender).Text = RecordingSettingsReader.Instance.RecordingTripID.ToString();
        }

        private void DEBUG_RecordingTripIDTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value)) {
                RecordingSettingsReader.Instance.MaxAccuracy = value;
            } else {
                DEBUG_RecordingTripIDTextBox_Loading(sender as FrameworkElement, null);
            }
        }
    }
}
