using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocaTracker2.Db.Objects;
using Windows.Storage;
using LocaTracker2.Db;
using LocaTracker2.Logging;
using System.Xml.Linq;
using Windows.ApplicationModel;

namespace LocaTracker2.Exchange.Export
{
    public class GpxTripExporter : BaseExporter
    {
        public override async Task DoExport(StorageFile file, Trip trip)
        {
            StorageFileLogger.Instance.I(this, $"Exporting Trip {trip.TripID} to {file.Path}");

            StringBuilder builder = new StringBuilder();

            // GPX Start
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>"
                + "<gpx xmlns=\"http://www.topografix.com/GPX/1/1\""
                + " version=\"1.1\""
                + $" creator=\"{Package.Current.PublisherDisplayName}\""
                + " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""
                + " xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");

            // Metadata
            builder.AppendLine("<metadata>"
                + $"<name>{EscapeStringForXML(trip.Name)}</name>"
                + $"<desc>{trip.Description}</desc>"
                + "</metadata>");
            
            using (var db = LocaTrackerDbContext.GetNonTrackingInstance()) {
                var tripID = trip.TripID;
                var tripSections = db.TripSections.Where(s => s.TripID == tripID);

                foreach (var tripSection in tripSections) {
                    GetSection(db, builder, tripSection);
                }
            }

            // GPX End
            builder.AppendLine("</gpx>");

            // => Write File
            try {
                StorageFileLogger.Instance.I(this, $"Flushing Buffer to File {file.Path} ...");
                await FileIO.WriteLinesAsync(file, new List<String>() { builder.ToString() });
                Result = ExportResult.Success;
            } catch (Exception ex) {
                StorageFileLogger.Instance.E(this, LoggingUtilities.GetExceptionMessage(ex));
                Result = ExportResult.Failed;
                ErrorCauseByException = ex;
            }
        }

        private void GetSection(LocaTrackerDbContext db, StringBuilder builder, TripSection section)
        {
            // Section ("Track") Start
            builder.AppendLine("<trk>");
            builder.AppendLine($"<name>{section.SectionDescription}</name>");
            builder.AppendLine($"<description>{section.SectionDistance} m</description>");
            // Section Points ("Track Section") Start
            builder.AppendLine("<trkseg>");

            var sectionID = section.TripSectionID;
            var points = db.Points.Where(p => p.TripSectionID == sectionID).OrderBy(p => p.Timestamp);
            foreach (var point in points) {
                builder.AppendLine(GetPoint(db, point));
            }

            // Section Points ("Track Section") End
            builder.AppendLine("</trkseg>");
            // Section ("Track") End
            builder.AppendLine("</trk>");
        }

        private string GetPoint(LocaTrackerDbContext db, Point point)
        {
            return
                $"<trkpt lat=\"{point.Latitude}\" lon=\"{point.Longitude}\">" +
                $"<ele>{point.Altitude}</ele>" +
                $"<time>{point.Timestamp:O}</time>" +
                $"<hdop>{point.Accuracy}</hdop>" +
                $"<vdop>{point.AltitudeAccuracy}</vdop>" +
                "</trkpt>"
            ;
        }

        private string EscapeStringForXML(string s)
        {
            XElement xEl = new XElement("s", s);
            return xEl.ToString().Replace("<s>", "").Replace("</s>", "");
        }
    }
}
