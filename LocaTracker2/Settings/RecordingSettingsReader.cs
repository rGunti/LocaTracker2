using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Settings
{
    public class RecordingSettingsReader : SettingsReader<RecordingSettingsReader>
    {
        private const string KEY_MIN_SPEED = "LOCATRACKER2.RECORDING.MinSpeed";
        private const string KEY_MAX_ACCURACY = "LOCATRACKER2.RECORDING.MaxAccuracy";
        private const string KEY_RECORDING_TRIP = "LOCATRACKER2.RECORDING.RecordingTripID";

        protected override void InitializeSettingsValues()
        {
            InitSettingsValue(KEY_MIN_SPEED, (double)1.388);    // 5 km/h
            InitSettingsValue(KEY_MAX_ACCURACY, (double)50);    // 50 m
            InitSettingsValue(KEY_RECORDING_TRIP, (int)0);      // -
        }

        public double MinSpeed {
            get { return GetValue<double>(KEY_MIN_SPEED); }
            set { SetValue(KEY_MIN_SPEED, value); }
        }

        public double MaxAccuracy {
            get { return GetValue<double>(KEY_MAX_ACCURACY); }
            set { SetValue(KEY_MAX_ACCURACY, value); }
        }

        public int RecordingTripID {
            get { return GetValue<int>(KEY_RECORDING_TRIP); }
            set { SetValue(KEY_RECORDING_TRIP, value); }
        }
    }
}
