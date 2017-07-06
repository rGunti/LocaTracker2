﻿using LocaTracker2.Db;
using LocaTracker2.Logging;
using LocaTracker2.Logging.ETW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Settings.MaintenanceTask
{
    public class DistanceRecalculationTask : BaseMaintenanceTask
    {
        protected override void DoJob()
        {
            using (var db = new LocaTrackerDbContext()) {
                db.ChangeTracker.AutoDetectChangesEnabled = false;

                var sections = db.TripSections;
                int counter = 0;
                foreach (var section in sections) {
                    ReportStatus($"Recalculating Section {++counter}/{sections.Count()} ({section.TripID}-{section.TripSectionID})...");
                    if (db.Points.Count(p => p.TripSectionID == section.TripSectionID) == 0) {
                        ReportStatus($"Deleting Empty Section {counter}/{sections.Count()} ({section.TripID}-{section.TripSectionID})...");
                        db.Remove(section);
                    } else {
                        ReportStatus($"Checking Section {counter}/{sections.Count()} ({section.TripID}-{section.TripSectionID})...");

                        section.CalculateSectionDistance();
                        section.StoredSectionDistance = section.SectionDistance;

                        db.Update(section);
                    }
                }
                try {
                    ReportStatus("Saving changes...");
                    db.SaveChanges();
                    SetTaskResult(MaintenanceTaskResult.Success);
                } catch (Exception ex) {
                    LocaTrackerEventSource.Instance.Error(LoggingUtilities.GetExceptionMessage(ex));
                    SetTaskResult(MaintenanceTaskResult.Failed);
                }
            }
        }
    }
}
