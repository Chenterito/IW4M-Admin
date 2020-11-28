﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace SharedLibraryCore.Migrations.Sqlite
{
    public partial class AddIndexToMessageTimeSent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EFClientMessages_TimeSent",
                table: "EFClientMessages",
                column: "TimeSent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EFClientMessages_TimeSent",
                table: "EFClientMessages");
        }
    }
}
