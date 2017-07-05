using LocaTracker2.Gps;
using LocaTracker2.Logic;
using LocaTracker2.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        #region Brushes
        static Brush
            defaultBrush = null,
            controlBrush = new SolidColorBrush(Color.FromArgb(255, 0, 160, 0)),
            warningBrush = new SolidColorBrush(Color.FromArgb(255, 255, 125, 0)),
            errorBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            disabledBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
            whiteBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        ;
        #endregion Brushes

        private UnitSettingsReader unitSettings;
        private bool useImperialUnits;

        DispatcherTimer
            clockTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) },
            gpsTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(2500) }
        ;

        public MainPage()
        {
            this.InitializeComponent();

            GpsRecorder.Instance.OnPositionUpdate += GpsRecorder_OnPositionUpdate;

            unitSettings = UnitSettingsReader.Instance;
            useImperialUnits = unitSettings.UseImperialUnits;

            clockTimer.Tick += ClockTimer_Tick;
            gpsTimer.Tick += GpsTimer_Tick;

            clockTimer.Start();
            gpsTimer.Start();
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
            if (accuracy > 50) iconAccuracy = bar0;
            else if (accuracy > 25) iconAccuracy = bar1;
            else if (accuracy > 15) iconAccuracy = bar2;
            else if (accuracy > 5) iconAccuracy = bar3;
            else iconAccuracy = bar4;

            AccuracyStatusIndicator.Icon = iconAccuracy;
            AccuracyStatusIndicator.Label = $"{Convert.ToChar(177)} {accuracy:0} m";
        }
        #endregion UI Data Modification Methods


    }
}
