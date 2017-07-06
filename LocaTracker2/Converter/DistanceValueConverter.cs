using LocaTracker2.Gps;
using LocaTracker2.Settings;
using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace LocaTracker2.Converter
{
    public class DistanceValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double distance = (double)value;
            if (UnitSettingsReader.Instance.UseImperialUnits) return GpsUtilities.HumanReadableConverter.GetImperialDistance(distance, true, "\n");
            return GpsUtilities.HumanReadableConverter.GetMetricDistance(distance, true, "\n");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = (string)value;
            string[] values = strValue.Split('\n');

            double numberValue;
            string unit = values[1];
            if (double.TryParse(values[0], out numberValue)) {
                switch (unit) {
                    case "m": return numberValue;
                    case "km": return (numberValue * 1000);
                    case "yd": return GpsUtilities.MetricImperialConverter.ConvertYardToMeter(numberValue);
                    case "mi": return GpsUtilities.MetricImperialConverter.ConvertMilesToKM(numberValue);
                    default: throw new InvalidOperationException();
                }
            } else {
                throw new InvalidCastException();
            }
        }
    }
}
