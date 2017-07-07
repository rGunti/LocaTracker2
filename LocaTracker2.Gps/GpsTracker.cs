using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace LocaTracker2.Gps
{
    public abstract class GpsTracker<T> where T : GpsTracker<T>, new()
    {
        #region Singleton
        protected static T instance;
        public static T Instance {
            get {
                if (instance == null) instance = new T();
                return instance;
            }
        }
        #endregion Singleton

        protected Geolocator locator = null;
        protected GeolocationAccessStatus accessStatus;
        protected PositionStatus positionStatus;

        protected bool settingUp = false;

        public GeolocationAccessStatus LocatorAccessStatus { get { return accessStatus; } }

        #region Setup
        public async Task InitAsync()
        {
            StorageFileLogger.Instance.I(this, "Initializing Tracker ...");
            settingUp = true;

            accessStatus = await Geolocator.RequestAccessAsync();
            StorageFileLogger.Instance.V(this, $"Current State: {accessStatus}");
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    locator = new Geolocator()
                    {
                        ReportInterval = 500,
                        DesiredAccuracy = PositionAccuracy.High
                    };
                    locator.PositionChanged += Locator_PositionChanged;
                    locator.StatusChanged += Locator_StatusChanged;
                    break;
                case GeolocationAccessStatus.Denied:
                case GeolocationAccessStatus.Unspecified:
                default:
                    locator = null;
                    break;
            }

            settingUp = false;
        }
        #endregion Setup

        #region Event Status
        protected abstract void Locator_StatusChanged(Geolocator sender, StatusChangedEventArgs args);
        protected abstract void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args);
        #endregion Event Status
    }
}
