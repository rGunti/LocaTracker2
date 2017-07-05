﻿using LocaTracker2.Gps;
using LocaTracker2.Logic;
using LocaTracker2.Settings;
using System;
using System.Threading.Tasks;
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

        private UnitSettingsReader unitSettings;
        private bool useImperialUnits;

        DispatcherTimer
            clockTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) },
            gpsTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(2500) },
            statusIndicatorTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) }
        ;

        static StatusIndicatorState
            accuracyState = StatusIndicatorState.Off,
            batteryState = StatusIndicatorState.Off,
            recordingState = StatusIndicatorState.Off
        ;
        AppBarButton[] statusIndicatorElements;

        public MainPage()
        {
            this.InitializeComponent();

            GpsRecorder.Instance.OnPositionUpdate += GpsRecorder_OnPositionUpdate;

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
            gpsTimer.Tick += GpsTimer_Tick;
            statusIndicatorTimer.Tick += StatusIndicatorTimer_Tick;

            clockTimer.Start();
            gpsTimer.Start();
            statusIndicatorTimer.Start();
        }

        private void StatusIndicatorTimer_Tick(object sender, object e)
        {
            foreach (AppBarButton indicator in statusIndicatorElements)
            {
                StatusIndicatorState state = (StatusIndicatorState)indicator.Tag;
                Brush brush = state.GetBrush();

                if (state.IsBlinking()) {
                    if (indicator.Background == brush) {
                        indicator.Background = StatusIndicatorStateExtesions.DefaultBrush;
                    } else {
                        indicator.Background = brush;
                    }
                } else {
                    indicator.Background = brush;
                }
            }
        }

        private async void GpsRecorder_OnPositionUpdate(Db.Objects.Point point, bool wasRecorded)
        {
            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SetSpeed(point.Speed);
                SetAltitude(point.Altitude);
                SetAccuracy(point.Accuracy);
            });
        }

        private void GpsTimer_Tick(object sender, object e)
        {
            if (GpsRecorder.Instance.CurrentPosition != null)
            {
                var p = GpsRecorder.Instance.CurrentPosition;
                SetAccuracy(p.Accuracy);
            } else
            {
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
            Frame.Navigate(typeof(TripListPage));
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
                Task.Run(() => {
                    if (!GpsRecorder.Instance.StartRecording())
                    {
                        // TODO: Show Message Box
                    }
                });
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            useImperialUnits = unitSettings.UseImperialUnits;

            SpeedUnitLabel.Text = (useImperialUnits) ? "mph" : "km/h";
            AlternativeUnitSpeedUnitLabel.Text = (useImperialUnits) ? "km/h" : "mph";
            AltitudeUnitLabel.Text = (useImperialUnits) ? "ft" : "m";
            DistanceUnitLabel.Text = (useImperialUnits) ? "mi" : "km";
        }

        #region UI Data Modification Methods
        public void SetSpeed(double metricValue)
        {
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
        }

        public void SetTime(DateTime utcTime, DateTime? currentTime = null)
        {
            if (!currentTime.HasValue) currentTime = DateTime.Now;
            LocalTimeLabel.Text = $"{currentTime.Value:HH:mm:ss}";
            UtcTimeLabel.Text = $"{utcTime:HH:mm:ss}";
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
        }
        #endregion UI Data Modification Methods


    }
}
