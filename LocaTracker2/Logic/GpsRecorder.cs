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
    public enum RecordingPausedReason
    {
        WasNot, Initializing, FailedToInitialize, LowSpeed, LowAccuracy
    }

    public class GpsRecorder : GpsTracker<GpsRecorder>
    {
        public delegate void OnPositionUpdateDelegate(Point point, RecordingPausedReason recordingPausedReason);
        public event OnPositionUpdateDelegate OnPositionUpdate;

        public LocaTrackerDbContext dbContext;

        public Point CurrentPosition { get; protected set; } = null;
        public TripSection CurrentRecordingTripSection { get; protected set; }
        public Trip CurrentRecordingTrip { get; protected set; }

        public double CurrentTripDistance { get; protected set; }
        public double CurrentTripSectionDistance { get; protected set; }

        public bool IsRecording { get; protected set; } = false;

        public bool StartRecording()
        {
            dbContext = new LocaTrackerDbContext();

            CurrentRecordingTrip = dbContext.Trips.FirstOrDefault(t => t.TripID == RecordingSettingsReader.Instance.RecordingTripID);
            if (CurrentRecordingTrip == null) return false;

            TripSection tripSection = new TripSection()
            {
                TripID = CurrentRecordingTrip.TripID,
                Started = DateTime.UtcNow
            };
            dbContext.Add(tripSection);
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
            dbContext.Dispose();
            IsRecording = false;
        }

        protected override void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Point point = GpsModelExtension.GetPointFromGpsTracker(args.Position.Coordinate);
            RecordingPausedReason reason = RecordingPausedReason.WasNot;
            double minSpeed = RecordingSettingsReader.Instance.MinSpeed;

            bool doRecording = IsRecording && point.Accuracy <= RecordingSettingsReader.Instance.MaxAccuracy;
            if (doRecording && point.Speed < minSpeed && CurrentPosition.Speed < minSpeed) doRecording = false;

            if (doRecording)
            {
                point.TripSectionID = CurrentRecordingTripSection.TripSectionID;
                dbContext.Add(point);
                dbContext.SaveChanges();
            } else if (point.Speed < minSpeed) {
                reason = RecordingPausedReason.LowSpeed;
            } else if (point.Accuracy > RecordingSettingsReader.Instance.MaxAccuracy) {
                reason = RecordingPausedReason.LowAccuracy;
            }

            CurrentPosition = point;
            OnPositionUpdate?.Invoke(point, reason);
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
                Timestamp = DateTime.UtcNow,
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
