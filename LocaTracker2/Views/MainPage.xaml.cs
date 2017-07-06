﻿using LocaTracker2.Battery;
using LocaTracker2.Gps;
using LocaTracker2.Logging.ETW;
using LocaTracker2.Logic;
using LocaTracker2.Settings;
using LocaTracker2.Views.Dialogs;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LocaTracker2.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static LocaTrackerEventSource log = LocaTrackerEventSource.Instance;

        #region Symbol Icons
        static readonly SymbolIcon
            barN = new SymbolIcon(Symbol.Map),
            bar0 = new SymbolIcon(Symbol.ZeroBars),
            bar1 = new SymbolIcon(Symbol.OneBar),
            bar2 = new SymbolIcon(Symbol.TwoBars),
            bar3 = new SymbolIcon(Symbol.ThreeBars),
            bar4 = new SymbolIcon(Symbol.FourBars)
        ;
        #endregion

        static bool missingPermissionScreenDisplayed = false;
        static bool blinkingIndicatorOn = false;
        static MissingPermissionDialog missingPermissionDialog = new MissingPermissionDialog();

        static string
            lsv_mainSpeed = "  0",
            lsv_secSpeed = "0",
            lsv_altitude = "0",
            lsv_distance = "0.0",
            lsv_localTime = "--:--:--",
            lsv_utcTime = "--:--:--",
            lsv_accuracy = "0"
        ;        

        private UnitSettingsReader unitSettings;
        private bool useImperialUnits;

        DispatcherTimer
            clockTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) },
            gpsTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(2500) },
            statusIndicatorTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) },
            batteryTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5000) }
        ;

        static StatusIndicatorState
            accuracyState = StatusIndicatorState.Off,
            batteryState = StatusIndicatorState.Off,
            recordingState = StatusIndicatorState.Off
        ;
        AppBarButton[] statusIndicatorElements;

        public MainPage()
        {
            log.Verbose("Initializing Main Page...");

            this.InitializeComponent();

            GpsRecorder.Instance.OnPositionUpdate += GpsRecorder_OnPositionUpdate;
            UnitSettingsReader.Instance.OnSettingsChanged += UnitSettingsReader_OnSettingsChanged;
            
            unitSettings = UnitSettingsReader.Instance;
            useImperialUnits = unitSettings.UseImperialUnits;

            statusIndicatorElements = new AppBarButton[]
            {
                BatteryStatusIndicator,
                AccuracyStatusIndicator,
                RecordButton
            };
            BatteryStatusIndicator.Tag = batteryState;
            AccuracyStatusIndicator.Tag = accuracyState;
            RecordButton.Tag = recordingState;

            clockTimer.Tick += ClockTimer_Tick;
            gpsTimer.Tick += GpsTimer_TickAsync;
            statusIndicatorTimer.Tick += StatusIndicatorTimer_Tick;
            batteryTimer.Tick += BatteryTimer_Tick;

            RestoreLastStoredValues();

            clockTimer.Start();
            gpsTimer.Start();
            statusIndicatorTimer.Start();
            batteryTimer.Start();
        }

        private void Frame_Navigating(object sender, Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            log.Verbose($"{sender.GetType()}");
        }

        private void RestoreLastStoredValues()
        {
            AccuracyStatusIndicator.Label = lsv_accuracy;
            SpeedLabel.Text = lsv_mainSpeed;
            AlternativeUnitSpeedLabel.Text = lsv_secSpeed;

            AltitudeLabel.Text = lsv_altitude;
            DistanceLabel.Text = lsv_distance;

            SetTime(DateTime.UtcNow);
        }

        private void UnitSettingsReader_OnSettingsChanged(string key, object newValue)
        {
            if (key == UnitSettingsReader.KEY_USE_IMPERIAL_UNITS)
            {
                useImperialUnits = (bool)newValue;
                SetUnitLabels(useImperialUnits);
            }
        }

        private async void BatteryTimer_Tick(object sender, object e)
        {
            await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => {
                var percentage = BatteryDataFetcher.Instance.BatteryPercentage;
                var icon = BatteryDataFetcher.GetIcon(percentage, BatteryDataFetcher.Instance.IsBatteryCharging);

                SetBatteryReport(icon, percentage);
            });
        }

        private void StatusIndicatorTimer_Tick(object sender, object e)
        {
            blinkingIndicatorOn = !blinkingIndicatorOn;
            foreach (AppBarButton indicator in statusIndicatorElements)
            {
                StatusIndicatorState state = (StatusIndicatorState)indicator.Tag;
                Brush brush = state.GetBrush();

                if (state.IsBlinking()) {
                    if (blinkingIndicatorOn) {
                        indicator.Background = brush;
                    } else {
                        indicator.Background = StatusIndicatorStateExtesions.DefaultBrush;
                    }
                } else {
                    indicator.Background = brush;
                }
            }
        }

        private async void GpsRecorder_OnPositionUpdate(Db.Objects.Point point, RecordingPausedReason recordingPausedReason)
        {
            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SetSpeed(point.Speed);
                SetAltitude(point.Altitude);
                SetAccuracy(point.Accuracy);

                SetRecordingState(GpsRecorder.Instance.IsRecording, recordingPausedReason);
            });
        }

        private async void GpsTimer_TickAsync(object sender, object e)
        {
            if (GpsRecorder.Instance.LocatorAccessStatus == Windows.Devices.Geolocation.GeolocationAccessStatus.Denied 
                && !missingPermissionScreenDisplayed) {
                missingPermissionScreenDisplayed = true;
                missingPermissionDialog.ShowAsync();
            } else if (GpsRecorder.Instance.LocatorAccessStatus == Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed && missingPermissionScreenDisplayed) {
                missingPermissionScreenDisplayed = false;
            }

            if (GpsRecorder.Instance.CurrentPosition != null) {
                var p = GpsRecorder.Instance.CurrentPosition;
                SetAccuracy(p.Accuracy);
            } else {
                SetAccuracy(double.NaN);
            }
        }

        private void ClockTimer_Tick(object sender, object e)
        {
            if (GpsRecorder.Instance.CurrentPosition != null) {
                var p = GpsRecorder.Instance.CurrentPosition;
                DateTime localTime = GeoTimeZoneUtils.GetTimeAtLocation(p.Latitude, p.Longitude);
                SetTime(DateTime.UtcNow, localTime);
            } else {
                SetTime(DateTime.UtcNow);
            }
        }

        private void TripsButton_Click(object sender, RoutedEventArgs e)
        {
            if (GpsRecorder.Instance.IsRecording) {
                new TripListBlockedDialog().ShowAsync();
            } else {
                Frame.Navigate(typeof(TripListPage));
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), this);
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (GpsRecorder.Instance.IsRecording)
            {
                Task.Run(() => {
                    GpsRecorder.Instance.EndRecording();
                });
            } else {
                RecordButton.IsEnabled = false;
                SetRecordingState(true, RecordingPausedReason.Initializing);
                Task.Run(async () => {
                    if (!GpsRecorder.Instance.StartRecording())
                    {
                        await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            SetRecordingState(false, RecordingPausedReason.FailedToInitialize);
                            RecordButton.IsEnabled = true;
                        });
                    } else {
                        await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                            RecordButton.IsEnabled = true;
                            SetDistance(GpsRecorder.Instance.CurrentTripDistance);
                        });
                    }
                });
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            useImperialUnits = unitSettings.UseImperialUnits;
            SetUnitLabels(useImperialUnits);
        }

        #region UI Data Modification Methods
        private void SetUnitLabels(bool useImperialUnits)
        {
            SpeedUnitLabel.Text = (useImperialUnits) ? "mph" : "km/h";
            AlternativeUnitSpeedUnitLabel.Text = (useImperialUnits) ? "km/h" : "mph";
            AltitudeUnitLabel.Text = (useImperialUnits) ? "ft" : "m";
            DistanceUnitLabel.Text = (useImperialUnits) ? "mi" : "km";
        }

        public void SetSpeed(double metricValue)
        {
            metricValue = GpsUtilities.PreventNaN(metricValue);
            double kmh = GpsUtilities.ConvertMPStoKMH(metricValue);
            double mph = GpsUtilities.MetricImperialConverter.ConvertMPStoMPH(metricValue);
            SpeedLabel.Text = $"{(useImperialUnits ? mph : kmh),3:0}";
            AlternativeUnitSpeedLabel.Text = $"{(useImperialUnits ? kmh : mph):0}";
        }

        public void SetAltitude(double metricValue)
        {
            double displayValue = metricValue;
            if (useImperialUnits)
            {
                displayValue = GpsUtilities.MetricImperialConverter.ConvertMeterToFeet(metricValue);
            }
            AltitudeLabel.Text = $"{displayValue:0}";
        }

        public void SetDistance(double metricValue)
        {
            double displayValue = metricValue / 1000;
            if (useImperialUnits)
            {
                displayValue = GpsUtilities.MetricImperialConverter.ConvertKMtoMile(metricValue);
            }
            DistanceLabel.Text = $"{displayValue:0.0}";
            lsv_distance = DistanceLabel.Text;
        }

        public void SetTime(DateTime utcTime, DateTime? currentTime = null)
        {
            if (!currentTime.HasValue) currentTime = DateTime.Now;
            LocalTimeLabel.Text = $"{currentTime.Value:HH:mm:ss}";
            UtcTimeLabel.Text = $"{utcTime:HH:mm:ss}";

            lsv_localTime = LocalTimeLabel.Text;
            lsv_utcTime = UtcTimeLabel.Text;
        }

        public void SetAccuracy(double accuracy)
        {
            IconElement iconAccuracy;
            if (double.IsNaN(accuracy)) { iconAccuracy = barN; accuracyState = StatusIndicatorState.CritError; }
            else if (accuracy > 50) { iconAccuracy = bar0; accuracyState = StatusIndicatorState.CritError; }
            else if (accuracy > 25) { iconAccuracy = bar1; accuracyState = StatusIndicatorState.WarnInfo; }
            else if (accuracy > 15) { iconAccuracy = bar2; accuracyState = StatusIndicatorState.Warning; }
            else if (accuracy > 5) { iconAccuracy = bar3; accuracyState = StatusIndicatorState.Ok; }
            else { iconAccuracy = bar4; accuracyState = StatusIndicatorState.Ok; }

            AccuracyStatusIndicator.Icon = iconAccuracy;
            AccuracyStatusIndicator.Tag = accuracyState;
            if (double.IsNaN(accuracy))
                AccuracyStatusIndicator.Label = $"No Data";
            else
                AccuracyStatusIndicator.Label = $"{Convert.ToChar(177)} {accuracy:0} m";

            lsv_accuracy = AccuracyStatusIndicator.Label;
        }

        public void SetRecordingState(bool isRecording, RecordingPausedReason recordingPausedReason)
        {
            if (isRecording)
            {
                switch (recordingPausedReason)
                {
                    case RecordingPausedReason.WasNot:
                        recordingState = StatusIndicatorState.Ok;
                        break;
                    case RecordingPausedReason.LowSpeed:
                        recordingState = StatusIndicatorState.Info;
                        break;
                    case RecordingPausedReason.Initializing:
                        recordingState = StatusIndicatorState.Warning;
                        break;
                    case RecordingPausedReason.LowAccuracy:
                    default:
                        recordingState = StatusIndicatorState.WarnInfo;
                        break;
                }
            } else {
                if (recordingPausedReason == RecordingPausedReason.FailedToInitialize)
                    recordingState = StatusIndicatorState.Error;
                else
                    recordingState = StatusIndicatorState.Off;
            }

            RecordButton.Tag = recordingState;
        }

        public void SetBatteryReport(BitmapIcon icon, double? battery) {
            BatteryStatusIndicator.Icon = icon;
            if (battery.HasValue) {
                BatteryStatusIndicator.Label = $"{battery} %";

                if (BatteryDataFetcher.Instance.IsBatteryCharging) {
                    batteryState = StatusIndicatorState.Info;
                } else {
                    if (battery <= 10) batteryState = StatusIndicatorState.CritError;
                    else if (battery <= 25) batteryState = StatusIndicatorState.Warning;
                    else batteryState = StatusIndicatorState.Off;
                }
            } else {
                BatteryStatusIndicator.Label = "No Battery";
                batteryState = StatusIndicatorState.Off;
            }

            BatteryStatusIndicator.Tag = batteryState;
        }
        #endregion UI Data Modification Methods


    }
}
