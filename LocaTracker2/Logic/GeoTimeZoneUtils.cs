using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Logic
{
    public static class GeoTimeZoneUtils
    {
        public static TimeZoneInfo GetTimeZoneOfLocation(double lat, double lon)
        {
            var ianaTimeZone = GeoTimeZone.TimeZoneLookup.GetTimeZone(lat, lon).Result;
            return TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
        }

        public static DateTime GetTimeAtLocation(double lat, double lon)
        {
            TimeZoneInfo tz = GetTimeZoneOfLocation(lat, lon);
            DateTime utc = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.Utc, tz);
        }
    }
}
