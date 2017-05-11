using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LocaTracker2.Db.Migrations
{
    public partial class AddedTripSectionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Ended",
                table: "TripSection",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Started",
                table: "TripSection",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ended",
                table: "TripSection");

            migrationBuilder.DropColumn(
                name: "Started",
                table: "TripSection");
        }
    }
}
