﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations.MySql
{
    public partial class AddAlternateKeyToEFClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EFClients_NetworkId",
                table: "EFClients");
            
            migrationBuilder.Sql("UPDATE `EFClients` set `GameName` = 0 WHERE `GameName` IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "GameName",
                table: "EFClients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_EFClients_NetworkId_GameName",
                table: "EFClients",
                columns: new[] { "NetworkId", "GameName" });

            migrationBuilder.CreateIndex(
                name: "IX_EFClients_NetworkId",
                table: "EFClients",
                column: "NetworkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_EFClients_NetworkId_GameName",
                table: "EFClients");

            migrationBuilder.DropIndex(
                name: "IX_EFClients_NetworkId",
                table: "EFClients");

            migrationBuilder.AlterColumn<int>(
                name: "GameName",
                table: "EFClients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_EFClients_NetworkId",
                table: "EFClients",
                column: "NetworkId",
                unique: true);
        }
    }
}
