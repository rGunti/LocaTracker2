﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LocaTracker2.Db;

namespace LocaTracker2.Db.Migrations
{
    [DbContext(typeof(LocaTrackerDbContext))]
    [Migration("20170513214507_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<int>("TripSectionID");

                    b.HasKey("PointID");

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

                    b.Property<DateTime?>("Ended");

                    b.Property<DateTime>("Started");

                    b.Property<int>("TripID");

                    b.HasKey("TripSectionID");

                    b.HasIndex("TripID");

                    b.ToTable("TripSections");
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.Point", b =>
                {
                    b.HasOne("LocaTracker2.Db.Objects.TripSection", "TripSection")
                        .WithMany("Points")
                        .HasForeignKey("TripSectionID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LocaTracker2.Db.Objects.TripSection", b =>
                {
                    b.HasOne("LocaTracker2.Db.Objects.Trip", "Trip")
                        .WithMany("Sections")
                        .HasForeignKey("TripID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
