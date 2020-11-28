﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedLibraryCore.Migrations.Sqlite
{
    public partial class AddVisibilityPercentage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "VisibilityPercentage",
                table: "EFClientKills",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisibilityPercentage",
                table: "EFClientKills");
        }
    }
}
