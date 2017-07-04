using LocaTracker2.Gps;
using LocaTracker2.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private UnitSettingsReader unitSettings;
        private bool useImperialUnits;

        DispatcherTimer
            clockTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) }
        ;

        public MainPage()
        {
            this.InitializeComponent();

            unitSettings = UnitSettingsReader.Instance;
            useImperialUnits = unitSettings.UseImperialUnits;

            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void ClockTimer_Tick(object sender, object e)
        {
            // TODO: Use Current Location
            SetTime(DateTime.Now);
        }

        private void TripsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TripListPage));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage), this);
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
            if (useImperialUnits) {
                displayValue = GpsUtilities.MetricImperialConverter.ConvertMeterToFeet(metricValue);
            }
            AltitudeLabel.Text = $"{displayValue:0}";
        }

        public void SetDistance(double metricValue)
        {
            double displayValue = metricValue / 1000;
            if (useImperialUnits) {
                displayValue = GpsUtilities.MetricImperialConverter.ConvertKMtoMile(metricValue);
            }
            DistanceLabel.Text = $"{displayValue:0.0}";
        }

        public void SetTime(DateTime currentTime)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTime(currentTime, TimeZoneInfo.Utc);
            LocalTimeLabel.Text = $"{currentTime:HH:mm:ss}";
            UtcTimeLabel.Text = $"{utcTime:HH:mm:ss}";
        }
        #endregion UI Data Modification Methods


    }
}
