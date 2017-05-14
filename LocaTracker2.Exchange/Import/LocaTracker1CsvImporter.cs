using LocaTracker2.Db;
using LocaTracker2.Db.Objects;
using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LocaTracker2.Exchange.Import
{
    public class LocaTracker1CsvImporter : BaseImporter
    {
        public override async Task DoImport(StorageFile file)
        {
            bool importFailed = false;

            LocaTrackerEventSource.Instance.Verbose($"Importing from {file.Path}...");
            IList<string> lines = await FileIO.ReadLinesAsync(file);

            using (var db = LocaTrackerDbContext.GetNonTrackingInstance()) {
                Trip trip = new Trip() {
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.Path),
                    Description = $"Imported from LocaTracker 1 using \"{file.Path}\"",
                    Sections = new List<TripSection>()
                };
                var tripEntity = db.Add(trip);

                TripSection section = new TripSection() {
                    TripID = tripEntity.Entity.TripID,
                    Points = new List<Point>()
                };
                var sectionEntity = db.Add(section);
                
                int lineCounter = 0;
                double distance = 0;
                Point lastPoint = null;
                foreach (var line in lines) {
                    lineCounter++;
                    // Ignore first and last line
                    if (lineCounter == 1 || lineCounter == lines.Count + 1) { continue; }

                    var cells = line.Split(';');
                    if (cells.Count() < 8 || line.StartsWith("===")) {
                        LocaTrackerEventSource.Instance.Warn($"Cannot import line #{lineCounter} because there are not enough fields!");
                        continue;
                    }

                    double latitude;
                    double longitude;
                    double accuracy;
                    double altitude;
                    double altAccuracy;
                    double speed;
                    double heading;

                    if (!double.TryParse(cells[1], out latitude)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Latitude\"."); importFailed = true; }
                    if (!double.TryParse(cells[2], out longitude)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Longitude\"."); importFailed = true; }
                    if (!double.TryParse(cells[3], out accuracy)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Accuracy\"."); importFailed = true; }
                    if (!double.TryParse(cells[4], out altitude)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Altitude\"."); importFailed = true; }
                    if (!double.TryParse(cells[5], out altAccuracy)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Alt. Accuracy\"."); importFailed = true; }
                    if (!double.TryParse(cells[6], out speed)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Speed\"."); importFailed = true; }
                    if (!double.TryParse(cells[7], out heading)) { LocaTrackerEventSource.Instance.Warn($"Line #{lineCounter}: Failed to parse field \"Heading\"."); importFailed = true; }

                    Point point = new Point() {
                        Timestamp = DateTime.ParseExact(cells[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Latitude = latitude,
                        Longitude = longitude,
                        Accuracy = accuracy,
                        AltitudeAccuracy = altAccuracy,
                        Speed = speed,
                        Heading = heading,

                        TripSectionID = sectionEntity.Entity.TripSectionID
                    };

                    if (lastPoint != null) {
                        distance += Point.CalculateDistance(lastPoint, point);
                    }
                    lastPoint = point;

                    LocaTrackerEventSource.Instance.Verbose($"Created Point {trip.Name}, #{lineCounter}, {point.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    db.Add(point);
                }

                LocaTrackerEventSource.Instance.Info($"Parsed {section.Points.Count} point(s) from {file.Path}");

                sectionEntity.Entity.Started = section.Points.Min(p => p.Timestamp);
                sectionEntity.Entity.Ended = section.Points.Max(p => p.Timestamp);
                sectionEntity.Entity.StoredSectionDistance = distance;
                try {
                    db.Update(sectionEntity.Entity);
                } catch (Exception ex) {
                    LocaTrackerEventSource.Instance.Error(LoggingUtilities.GetExceptionMessage(ex));
                    Result = ImportResult.Failed;
                    ErrorCauseByException = ex;
                }

                if (!importFailed) {
                    LocaTrackerEventSource.Instance.Info($"Saving changes...");
                    try {
                        db.SaveChanges();
                        Result = ImportResult.Success;
                    } catch (Exception ex) {
                        LocaTrackerEventSource.Instance.Error(LoggingUtilities.GetExceptionMessage(ex));
                        Result = ImportResult.Failed;
                        ErrorCauseByException = ex;
                    }
                } else {
                    Result = ImportResult.Failed;
                }
            }
        }
    }
}
