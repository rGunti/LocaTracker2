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

        protected override void InitializeSettingsValues() {
            InitSettingsValue(KEY_WARNING_ENABLED, true);                               // Warning:       Enabled
            InitSettingsValue(KEY_WARNING_SPEED, GpsUtilities.ConvertKMHtoMPS(135));    // Warning Speed: 135 km/h
        }

        public bool SpeedWarningEnabled {
            get { return GetBool(KEY_WARNING_ENABLED); }
            set { SetBool(KEY_WARNING_ENABLED, value); }
        }

        public double SpeedWarningMaxSpeed {
            get { return GetValue<double>(KEY_WARNING_SPEED); }
            set { SetValue(KEY_WARNING_SPEED, value); }
        }
    }
}
