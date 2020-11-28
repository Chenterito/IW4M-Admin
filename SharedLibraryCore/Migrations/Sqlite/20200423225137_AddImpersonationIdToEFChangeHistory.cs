﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedLibraryCore.Migrations.Sqlite
{
    public partial class AddImpersonationIdToEFChangeHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImpersonationEntityId",
                table: "EFChangeHistory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImpersonationEntityId",
                table: "EFChangeHistory");
        }
    }
}
