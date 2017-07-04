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

        public MainPage()
        {
            this.InitializeComponent();
            unitSettings = UnitSettingsReader.Instance;
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
            bool useImperialUnits = unitSettings.UseImperialUnits;

            SpeedUnitLabel.Text = (useImperialUnits) ? "mph" : "km/h";
            AlternativeUnitSpeedUnitLabel.Text = (useImperialUnits) ? "km/h" : "mph";
            AltitudeUnitLabel.Text = (useImperialUnits) ? "ft" : "m";
            DistanceUnitLabel.Text = (useImperialUnits) ? "mi" : "km";
        }
    }
}
