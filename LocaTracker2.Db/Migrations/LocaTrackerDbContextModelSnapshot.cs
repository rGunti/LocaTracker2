using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LocaTracker2.Db;

namespace LocaTracker2.Db.Migrations
{
    [DbContext(typeof(LocaTrackerDbContext))]
    partial class LocaTrackerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("LocaTracker2.Db.Objects.Point", b =>
                {
                    b.Property<int>("PointID")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Accuracy");

                    b.Property<double>("Altitude");

                    b.Property<double>("AltitudeAccuracy");

                    b.Property<double>("Heading");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<double>("Speed");

                    b.Property<DateTime>("Timestamp");

                    b.Property<int?>("TripID");

                    b.Property<int>("TripSectionID");

                    b.HasKey("PointID");

                    b.HasIndex("TripID");

                    b.HasIndex("TripSectionID");

                    b.ToTable("Points");
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.Trip", b =>
                {
                    b.Property<int>("TripID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("TripID");

                    b.ToTable("Trips");
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.TripSection", b =>
                {
                    b.Property<int>("TripSectionID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TripID");

                    b.HasKey("TripSectionID");

                    b.HasIndex("TripID");

                    b.ToTable("TripSection");
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.Point", b =>
                {
                    b.HasOne("LocaTracker2.Db.Objects.Trip")
                        .WithMany("Points")
                        .HasForeignKey("TripID");

                    b.HasOne("LocaTracker2.Db.Objects.TripSection", "TripSection")
                        .WithMany()
                        .HasForeignKey("TripSectionID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.TripSection", b =>
                {
                    b.HasOne("LocaTracker2.Db.Objects.Trip", "Trip")
                        .WithMany()
                        .HasForeignKey("TripID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
