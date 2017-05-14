using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LocaTracker2.Db.Migrations
{
    public partial class AddedSectionDistanceToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "StoredSectionDistance",
                table: "TripSections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredSectionDistance",
                table: "TripSections");
        }
    }
}
