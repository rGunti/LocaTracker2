using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Gps;
using LocaTracker2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace LocaTracker2.Logic
{
    public class GpsRecorder : GpsTracker<GpsRecorder>
    {
        public delegate void OnPositionUpdateDelegate(Point point, bool wasRecorded);
        public event OnPositionUpdateDelegate OnPositionUpdate;

        public LocaTrackerDbContext dbContext = new LocaTrackerDbContext();

        public Point CurrentPosition { get; protected set; } = new Point();
        public TripSection CurrentRecordingTripSection { get; protected set; }
        public Trip CurrentRecordingTrip { get; set; }

        public double CurrentTripDistance { get; protected set; }
        public double CurrentTripSectionDistance { get; protected set; }

        public bool IsRecording { get; protected set; } = false;

        public bool StartRecording()
        {
            if (CurrentRecordingTripSection == null) return false;

            TripSection tripSection = new TripSection()
            {
                TripID = CurrentRecordingTrip.TripID,
                Started = DateTime.UtcNow
            };
            dbContext.Update(tripSection);
            dbContext.SaveChanges();
            CurrentRecordingTripSection = tripSection;
            IsRecording = true;
            return true;
        }

        public void EndRecording()
        {
            CurrentRecordingTripSection.Ended = DateTime.UtcNow;
            dbContext.Update(CurrentRecordingTripSection);
            dbContext.SaveChanges();
            IsRecording = false;
        }

        protected override void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Point point = GpsModelExtension.GetPointFromGpsTracker(args.Position.Coordinate);
            double minSpeed = RecordingSettingsReader.Instance.MinSpeed;

            bool doRecording = IsRecording && point.Accuracy < RecordingSettingsReader.Instance.MaxAccuracy;
            if (doRecording && point.Speed < minSpeed && CurrentPosition.Speed < minSpeed) doRecording = false;

            if (doRecording)
            {
                point.TripSectionID = CurrentRecordingTripSection.TripSectionID;
                dbContext.Add(point);
                dbContext.SaveChanges();
            }

            CurrentPosition = point;
            OnPositionUpdate?.Invoke(point, doRecording);
        }

        protected override void Locator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        { }
    }

    public static class GpsModelExtension
    {
        public static Point GetPointFromGpsTracker(Geocoordinate coord)
        {
            return new Point()
            {
                Timestamp = DateTime.Now,
                Latitude = coord.Point.Position.Latitude,
                Longitude = coord.Point.Position.Longitude,
                Altitude = coord.Point.Position.Altitude,
                Speed = GpsUtilities.ConvertNullableToNormal(coord.Speed),
                Accuracy = coord.Accuracy,
                AltitudeAccuracy = GpsUtilities.ConvertNullableToNormal(coord.AltitudeAccuracy),
                Heading = GpsUtilities.ConvertNullableToNormal(coord.Heading, double.NaN)
            };
        }
    }
}
