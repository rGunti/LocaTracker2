using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Power;
using Windows.UI.Xaml.Controls;

namespace LocaTracker2.Battery
{
    public class BatteryDataFetcher
    {
        #region Bitmap Icons
        static readonly BitmapIcon
            battery0 = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/battery0.png") },
            battery1 = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/battery1.png") },
            battery2 = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/battery2.png") },
            battery3 = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/battery3.png") },
            battery4 = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/battery4.png") },
            batteryC = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/batteryC.png") },
            batteryN = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/Icons/Battery/batteryN.png") }
        ;
        #endregion

        private static BatteryDataFetcher instance;
        public static BatteryDataFetcher Instance {
            get { Initialize(); return instance; }
        }

        public static void Initialize() {
            if (instance == null) instance = new BatteryDataFetcher();
        }

        public delegate void BatteryReportRecievedDelegate(BatteryDataFetcher sender, double? batteryPercentage, BatteryReport report);
        public event BatteryReportRecievedDelegate BatteryReportRecieved;

        public double? BatteryPercentage { get; private set; }
        public BatteryReport BatteryReport { get; private set; }
        public bool IsBatteryCharging {
            get { return BatteryReport?.Status == Windows.System.Power.BatteryStatus.Charging; }
        }

        public static BitmapIcon GetIcon(double? percentage, bool isCharging)
        {
            if (isCharging) return batteryC;
            else if (!percentage.HasValue) return batteryN;
            else if (percentage.Value < 20) return battery0;
            else if (percentage.Value < 40) return battery1;
            else if (percentage.Value < 60) return battery2;
            else if (percentage.Value < 80) return battery3;
            else return battery4;
        }

        private Windows.Devices.Power.Battery battery;

        private BatteryDataFetcher()
        {
            battery = Windows.Devices.Power.Battery.AggregateBattery;
            battery.ReportUpdated += Battery_ReportUpdated;
            Battery_ReportUpdated(battery, null);
        }

        private void Battery_ReportUpdated(Windows.Devices.Power.Battery sender, object args)
        {
            BatteryReport report;
            if (args != null && args is BatteryReport) report = args as BatteryReport;
            else report = sender.GetReport();

            if (report.FullChargeCapacityInMilliwattHours.HasValue) {
                BatteryPercentage = report.RemainingCapacityInMilliwattHours.Value / (double)report.FullChargeCapacityInMilliwattHours.Value * 100;
            } else {
                BatteryPercentage = null;
            }
            BatteryReport = report;

            BatteryReportRecieved?.Invoke(this, BatteryPercentage, report);
        }
    }
}
