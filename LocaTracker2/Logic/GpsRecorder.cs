﻿using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Gps;
using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
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
        public GpsRecorder() : base()
        {
            RecordingSettingsReader.Instance.OnSettingsChanged += RecordingSettings_OnSettingsChanged;

            minRecordingSpeed = RecordingSettingsReader.Instance.MinSpeed;
            maxRecordingAccuracy = RecordingSettingsReader.Instance.MaxAccuracy;
        }

        private void RecordingSettings_OnSettingsChanged(string key, object newValue)
        {
            switch (key) {
                case RecordingSettingsReader.KEY_MIN_SPEED:    minRecordingSpeed = (double)newValue; break;
                case RecordingSettingsReader.KEY_MAX_ACCURACY: maxRecordingAccuracy = (double)newValue; break;
                default: break;
            }
        }

        public delegate void OnPositionUpdateDelegate(Point point, RecordingPausedReason recordingPausedReason);
        public event OnPositionUpdateDelegate OnPositionUpdate;

        public LocaTrackerDbContext dbContext;

        protected double minRecordingSpeed;
        protected double maxRecordingAccuracy;

        public Point CurrentPosition { get; protected set; } = null;
        public TripSection CurrentRecordingTripSection { get; protected set; }
        public Trip CurrentRecordingTrip { get; protected set; }

        public double CurrentTripDistance { get; protected set; }
        public double CurrentTripSectionDistance { get; protected set; }

        public bool IsRecording { get; protected set; } = false;

        public bool StartRecording()
        {
            StorageFileLogger.Instance.I(this, "Recorder start has been invoked");

            dbContext = LocaTrackerDbContext.GetNonTrackingInstance();

            int tripID = RecordingSettingsReader.Instance.RecordingTripID;
            CurrentRecordingTrip = dbContext.Trips.FirstOrDefault(t => t.TripID == tripID);
            if (CurrentRecordingTrip == null) {
                StorageFileLogger.Instance.E(this, $"The selected trip with ID {RecordingSettingsReader.Instance.RecordingTripID} could not be found! Recording aborted!");
                return false;
            }

            StorageFileLogger.Instance.V(this, "Calculating Trip Distance...");
            CurrentTripSectionDistance = 0;
            CurrentTripDistance = 0;
            foreach (TripSection section in dbContext.TripSections.Where(s => s.TripID == RecordingSettingsReader.Instance.RecordingTripID)) {
                if (!section.StoredSectionDistance.HasValue) {
                    section.CalculateSectionDistance();
                }
                CurrentTripDistance += section.SectionDistance;
            }

            TripSection tripSection = new TripSection()
            {
                TripID = CurrentRecordingTrip.TripID,
                Started = DateTime.UtcNow
            };
            dbContext.Add(tripSection);
            dbContext.SaveChanges();
            CurrentRecordingTripSection = tripSection;
            CurrentPosition = null;
            IsRecording = true;

            StorageFileLogger.Instance.I(this, "Preparation complete, recording started");
            return true;
        }

        public void EndRecording()
        {
            StorageFileLogger.Instance.I(this, "Ending recording...");

            CurrentRecordingTripSection.Ended = DateTime.UtcNow;
            CurrentRecordingTripSection.StoredSectionDistance = CurrentTripSectionDistance;
            dbContext.Update(CurrentRecordingTripSection);
            dbContext.SaveChanges();
            dbContext.Dispose();
            IsRecording = false;

            StorageFileLogger.Instance.I(this, "Recording has ended");
        }

        protected override void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            StorageFileLogger.Instance.V(this, "New Location recieved");

            Point point = GpsModelExtension.GetPointFromGpsTracker(args.Position.Coordinate);
            RecordingPausedReason reason = RecordingPausedReason.WasNot;

            bool doRecording = IsRecording && point.Accuracy <= maxRecordingAccuracy;
            if (doRecording && point.Speed < minRecordingSpeed && CurrentPosition.Speed < minRecordingSpeed) doRecording = false;

            if (doRecording) {
                int tripSectionID = CurrentRecordingTripSection.TripSectionID;
                if (CurrentPosition != null) {
                    double delta = GpsUtilities.GetDistanceBetweenTwoPoints(
                        CurrentPosition.Latitude, CurrentPosition.Longitude,
                        point.Latitude, point.Longitude
                    );
                    CurrentTripSectionDistance += delta;
                    CurrentTripDistance += delta;
                }
                point.TripSectionID = tripSectionID;
                dbContext.Add(point);
                dbContext.SaveChanges();
            } else if (point.Speed < minRecordingSpeed) {
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
