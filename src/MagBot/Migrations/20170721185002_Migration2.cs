using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MagBot.Migrations
{
    public partial class Migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChannelLong",
                table: "Raffles",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "OwnerLong",
                table: "Raffles",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DiscordIdLong",
                table: "Guilds",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelLong",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "OwnerLong",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "DiscordIdLong",
                table: "Guilds");
        }
    }
}
