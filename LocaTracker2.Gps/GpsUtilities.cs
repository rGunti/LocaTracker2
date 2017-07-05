using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Gps
{
    /// <summary>
    /// A collection of functions that are used to calculate
    /// and convert GPS related data
    /// </summary>
    public static class GpsUtilities
    {
        /// <summary>
        /// Contains conversion methods for Metrical and Imperial units
        /// </summary>
        public static class MetricImperialConverter
        {
            /// <summary>
            /// Converts Distance in Meters [m] to Yards [yd]
            /// </summary>
            public static double ConvertMeterToYard(double meters) { return meters * 1.0936; }
            /// <summary>
            /// Converts Distance in Meters [m] to Feet [ft]
            /// </summary>
            public static double ConvertMeterToFeet(double meters) { return meters * 3.28084; }

            /// <summary>
            /// Converts Distance in Kilometers [km] to miles [mi]
            /// </summary>
            public static double ConvertKMtoMile(double km) { return km * 0.62137; }

            /// <summary>
            /// Converts Speed in Miles per Hour [mph] to Meters per Second [m/s]
            /// </summary>
            public static double ConvertMPHtoMPS(double mph) { return mph / 2.23694; }
            /// <summary>
            /// Converts Speed in Meters per Second [m/s] to Miles per Hour [mps]
            /// </summary>
            public static double ConvertMPStoMPH(double mps) { return mps * 2.23694; }
        }

        /// <summary>
        /// Contains functions to convert distances to human readable text
        /// </summary>
        public static class HumanReadableConverter
        {
            public static string GetMetricDistance(double distance, bool withUnit = false, string unitDelimiter = " ")
            {
                if (distance < 1000) return $"{distance:0.00}{(withUnit ? (unitDelimiter + "m") : "")}";
                else return $"{(distance / 1000):0.00}{(withUnit ? (unitDelimiter + "km") : "")}";
            }

            public static string GetImperialDistance(double distance, bool withUnit = false, string unitDelimiter = " ")
            {
                if (distance < 1760) return $"{distance:0.00}{(withUnit ? (unitDelimiter + "yd") : "")}";
                else return $"{(distance / 1760):0.00}{(withUnit ? (unitDelimiter + "mi") : "")}";
            }
        }

        /// <summary>
        /// Converts Speed in Kilometers per Hour [km/h] to Meters per Second [m/s]
        /// </summary>
        public static double ConvertKMHtoMPS(double kmh) { return kmh / 3.6; }
        /// <summary>
        /// Converts Speed in Meters per Second [m/s] to Kilometers per Hour [km/h]
        /// </summary>
        public static double ConvertMPStoKMH(double mps) { return mps * 3.6; }

        /// <summary>
        /// Calculates the distance between two given coordinates in meters
        /// </summary>
        /// <param name="latA">Point A: Latitude</param>
        /// <param name="lonA">Point A: Longitude</param>
        /// <param name="latB">Point B: Latitude</param>
        /// <param name="lonB">Point B: Longitude</param>
        /// <returns>Distance between Point A and B in meters</returns>
        public static double GetDistanceBetweenTwoPoints(double latA, double lonA, double latB, double lonB)
        {
            double R = 6371 * 1000;
            double dLat = ConvertToRad(latB - latA);
            double dLon = ConvertToRad(lonB - lonA);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ConvertToRad(latA)) * Math.Cos(ConvertToRad(latB)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
            return d;
        }

        private static double ConvertToRad(double val) { return (Math.PI / 180) * val; }

        public static double ConvertNullableToNormal(double? d, double defaultValue = 0) { return (d.HasValue) ? d.Value : defaultValue; }
        public static double PreventNaN(double d, double defaultValue = 0) { return (double.IsNaN(d)) ? defaultValue : d; }
    }
}
