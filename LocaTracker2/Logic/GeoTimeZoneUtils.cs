using GeoTimeZone;
using System;
using System.Linq;
using TimeZoneConverter;

namespace LocaTracker2.Logic
{
    public static class GeoTimeZoneUtils
    {
        public static TimeZoneInfo GetTimeZoneOfLocation(double lat, double lon)
        {
            var ianaTimeZone = TimeZoneLookup.GetTimeZone(lat, lon).Result;
            var winTimeZone = IanaToWindows(ianaTimeZone);
            return TimeZoneInfo.FindSystemTimeZoneById(winTimeZone);
        }

        public static DateTime GetTimeAtLocation(double lat, double lon)
        {
            TimeZoneInfo tz = GetTimeZoneOfLocation(lat, lon);
            DateTime utc = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.Utc, tz);
        }

        // This will return the Windows zone that matches the IANA zone, if one exists.
        private static string IanaToWindows(string ianaZoneId)
        {
            var utcZones = new[] { "Etc/UTC", "Etc/UCT", "Etc/GMT" };
            if (utcZones.Contains(ianaZoneId, StringComparer.Ordinal))
                return "UTC";

            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

            // resolve any link, since the CLDR doesn't necessarily use canonical IDs
            var links = tzdbSource.CanonicalIdMap
                .Where(x => x.Value.Equals(ianaZoneId, StringComparison.Ordinal))
                .Select(x => x.Key);

            // resolve canonical zones, and include original zone as well
            var possibleZones = tzdbSource.CanonicalIdMap.ContainsKey(ianaZoneId)
                ? links.Concat(new[] { tzdbSource.CanonicalIdMap[ianaZoneId], ianaZoneId })
                : links;

            // map the windows zone
            var mappings = tzdbSource.WindowsMapping.MapZones;
            var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(possibleZones.Contains));
            if (item == null) return null;
            return item.WindowsId;
        }
    }
}
