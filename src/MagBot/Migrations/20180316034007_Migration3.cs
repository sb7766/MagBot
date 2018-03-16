using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MagBot.Migrations
{
    public partial class Migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserIdLong",
                table: "RaffleEntries",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ReportingChannelIdLong",
                table: "Guilds",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "ReportingEnabled",
                table: "Guilds",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReportingRoleIdLong",
                table: "Guilds",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserIdLong",
                table: "BlacklistedRaffleUsers",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserIdLong",
                table: "RaffleEntries");

            migrationBuilder.DropColumn(
                name: "ReportingChannelIdLong",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "ReportingEnabled",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "ReportingRoleIdLong",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "UserIdLong",
                table: "BlacklistedRaffleUsers");
        }
    }
}
