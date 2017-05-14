using LocaTracker2.Gps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db.Objects
{
    public class TripSection
    {
        private static CultureInfo jpCulture = new CultureInfo("ja-JP");

        private int tripSectionID;
        private int tripID;
        private Trip trip;
        private List<Point> points;
        private DateTime started;
        private DateTime? ended;

        [Key]
        public int TripSectionID { get { return tripSectionID; } set { tripSectionID = value; } }

        public int TripID { get { return tripID; } set { tripID = value; } }
        public Trip Trip { get { return trip; } set { trip = value; } }

        public List<Point> Points { get { return points; } set { points = value; } }

        public DateTime Started { get { return started; } set { started = value; } }
        public DateTime? Ended { get { return ended; } set { ended = value; } }

        public bool IsActive { get { return !ended.HasValue; } }
        public string SectionDescription {
            get { return $"{started.ToString("g", jpCulture)} - {(IsActive ? "..." : $"{ended.Value.ToString("g", jpCulture)}")}"; }
        }

        private double? sectionDistance;
        public double SectionDistance {
            get { return (sectionDistance.HasValue) ? sectionDistance.Value : 0; }
        }
        public string SectionDistanceString {
            get { return (sectionDistance.HasValue) ? GpsUtilities.HumanReadableConverter.GetMetricDistance(SectionDistance, true, "\n") : "-"; }
        }

        public void CalculateSectionDistance() { sectionDistance = CalculateSectionDistance(this); }

        public static double CalculateSectionDistance(TripSection section)
        {
            double distance = 0;
            Point lastPoint = null;
            using (var db = new LocaTrackerDbContext()) {
                section.Points = db.Points.Where(p => p.TripSectionID == section.TripSectionID).OrderBy(p => p.Timestamp).ToList();
                foreach (Point point in section.Points) {
                    if (lastPoint != null)
                        distance += Point.CalculateDistance(lastPoint, point);
                    lastPoint = point;
                }
            }
            return distance;
        }
    }
}
