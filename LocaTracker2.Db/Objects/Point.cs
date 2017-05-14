using LocaTracker2.Gps;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db.Objects
{
    public class Point
    {
        [Key]
        public int PointID { get; set; }

        public int TripSectionID { get; set; }
        public TripSection TripSection { get; set; }

        public DateTime Timestamp { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Heading { get; set; }

        public double Accuracy { get; set; }
        public double AltitudeAccuracy { get; set; }

        public double Altitude { get; set; }

        public double Speed { get; set; }

        public static double CalculateDistance(Point a, Point b)
        {
            return GpsUtilities.GetDistanceBetweenTwoPoints(
                a.Latitude, a.Longitude,
                b.Latitude, b.Longitude
            );
        }
    }
}
