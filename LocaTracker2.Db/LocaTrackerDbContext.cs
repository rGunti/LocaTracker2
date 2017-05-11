using LocaTracker2.Db.Objects;
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
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Point> Points { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=locatracker2.db");
        }

        public static void InitDatabase()
        {
            using (var db = new LocaTrackerDbContext()) {
                db.Database.Migrate();
            }
        }
    }
}
