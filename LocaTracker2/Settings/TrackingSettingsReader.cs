using LocaTracker2.Gps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Settings
{
    public class TrackingSettingsReader : SettingsReader<TrackingSettingsReader>
    {
        public const string KEY_WARNING_ENABLED = "LOCATRACKER2.TRACKING.WarningEnabled";
        public const string KEY_WARNING_SPEED = "LOCATRACKER2.TRACKING.WarningSpeed";
        public const string KEY_LOCATION_INFO = "LOCATRACKER2.TRACKING.ShowLocationInfo";
        public const string KEY_LOCATION_UPDATE_INTERVAL = "LOCATRACKER2.TRACKING.LocationInfoUpdateInterval";
        public const string KEY_LOCATION_UPDATE_RETRIES = "LOCATRACKER2.TRACKING.LocationInfoUpdateRetries";
        public const string KEY_LOCATION_UPDATE_RETRY_INTERVAL = "LOCATRACKER2.TRACKING.LocationInfoUpdateRetryInterval";

        protected override void InitializeSettingsValues() {
            InitSettingsValue(KEY_WARNING_ENABLED, true);                               // Warning:                 Enabled
            InitSettingsValue(KEY_WARNING_SPEED, GpsUtilities.ConvertKMHtoMPS(135));    // Warning Speed:           135 km/h
            InitSettingsValue(KEY_LOCATION_INFO, true);                                 // Location Info:           Enabled
            InitSettingsValue(KEY_LOCATION_UPDATE_INTERVAL, 60);                        // Loc.Info Interval:       60 sec
            InitSettingsValue(KEY_LOCATION_UPDATE_RETRIES, 5);                          // Loc.Info Retries:        5
            InitSettingsValue(KEY_LOCATION_UPDATE_RETRY_INTERVAL, 5);                   // Loc.Info Retry Interval: 5 sec
        }

        public bool SpeedWarningEnabled {
            get { return GetBool(KEY_WARNING_ENABLED); }
            set { SetBool(KEY_WARNING_ENABLED, value); }
        }

        public double SpeedWarningMaxSpeed {
            get { return GetValue<double>(KEY_WARNING_SPEED); }
            set { SetValue(KEY_WARNING_SPEED, value); }
        }

        public bool ShowLocationInfo {
            get { return GetBool(KEY_LOCATION_INFO); }
            set { SetValue(KEY_LOCATION_INFO, value); }
        }

        public int LocationUpdateInterval {
            get { return GetValue<int>(KEY_LOCATION_UPDATE_INTERVAL); }
            set { SetValue(KEY_LOCATION_UPDATE_INTERVAL, value); }
        }

        public int LocationUpdateRetries {
            get { return GetValue<int>(KEY_LOCATION_UPDATE_RETRIES); }
        }

        public int LocationUpdateRetryInterval {
            get { return GetValue<int>(KEY_LOCATION_UPDATE_RETRY_INTERVAL); }
        }
    }
}
