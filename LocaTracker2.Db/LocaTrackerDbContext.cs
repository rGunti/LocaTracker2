using LocaTracker2.Logging;
using LocaTracker2.Db.Objects;
using MetroLog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaTracker2.Db
{
    public class LocaTrackerDbContext : DbContext
    {
        static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<LocaTrackerDbContext>();

        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripSection> TripSections { get; set; }
        public DbSet<Point> Points { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=locatracker2.db");
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }

        public static void InitDatabase()
        {
            log.Debug("Database is being initialized...");
            using (var db = new LocaTrackerDbContext()) {
                log.Trace("Database migration in progress...");
                try {
                    db.Database.Migrate();
                } catch (Exception ex) {
                    log.Fatal("Error white migrating database file. More information about this error below.");
                    log.Fatal(LoggingUtilities.GetExceptionMessage(ex));
                    log.Warn("The error will be thrown again so the app crashes!");
                    throw ex;
                }
            }
        }

        public static LocaTrackerDbContext GetNonTrackingInstance()
        {
            LocaTrackerDbContext ctx = new LocaTrackerDbContext();
            ctx.ChangeTracker.AutoDetectChangesEnabled = false;
            return ctx;
        }
    }
}
