using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LocaTracker2.Db.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    TripID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.TripID);
                });

            migrationBuilder.CreateTable(
                name: "TripSections",
                columns: table => new
                {
                    TripSectionID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ended = table.Column<DateTime>(nullable: true),
                    Started = table.Column<DateTime>(nullable: false),
                    TripID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripSections", x => x.TripSectionID);
                    table.ForeignKey(
                        name: "FK_TripSections_Trips_TripID",
                        column: x => x.TripID,
                        principalTable: "Trips",
                        principalColumn: "TripID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Points",
                columns: table => new
                {
                    PointID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Accuracy = table.Column<double>(nullable: false),
                    Altitude = table.Column<double>(nullable: false),
                    AltitudeAccuracy = table.Column<double>(nullable: false),
                    Heading = table.Column<double>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    Speed = table.Column<double>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    TripSectionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Points", x => x.PointID);
                    table.ForeignKey(
                        name: "FK_Points_TripSections_TripSectionID",
                        column: x => x.TripSectionID,
                        principalTable: "TripSections",
                        principalColumn: "TripSectionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Points_TripSectionID",
                table: "Points",
                column: "TripSectionID");

            migrationBuilder.CreateIndex(
                name: "IX_TripSections_TripID",
                table: "TripSections",
                column: "TripID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Points");

            migrationBuilder.DropTable(
                name: "TripSections");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
