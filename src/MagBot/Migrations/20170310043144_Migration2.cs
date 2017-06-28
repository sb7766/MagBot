using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MagBot.Migrations
{
    public partial class Migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Raffle",
                columns: table => new
                {
                    RaffleId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<ulong>(nullable: false),
                    HangfireId = table.Column<int>(nullable: false),
                    LocalId = table.Column<int>(nullable: false),
                    TimeCreated = table.Column<DateTime>(nullable: false),
                    TimeStarted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raffle", x => x.RaffleId);
                    table.ForeignKey(
                        name: "FK_Raffle_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaffleConfig",
                columns: table => new
                {
                    RaffleConfigId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Length = table.Column<TimeSpan>(nullable: false),
                    Prize = table.Column<string>(nullable: true),
                    RaffleId = table.Column<int>(nullable: false),
                    WhiteListedRole = table.Column<ulong>(nullable: false),
                    WinnerCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaffleConfig", x => x.RaffleConfigId);
                    table.ForeignKey(
                        name: "FK_RaffleConfig_Raffle_RaffleId",
                        column: x => x.RaffleId,
                        principalTable: "Raffle",
                        principalColumn: "RaffleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaffleEntry",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaffleId = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaffleEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaffleEntry_Raffle_RaffleId",
                        column: x => x.RaffleId,
                        principalTable: "Raffle",
                        principalColumn: "RaffleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedRaffleUser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaffleConfigId = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedRaffleUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistedRaffleUser_RaffleConfig_RaffleConfigId",
                        column: x => x.RaffleConfigId,
                        principalTable: "RaffleConfig",
                        principalColumn: "RaffleConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedRaffleUser_RaffleConfigId",
                table: "BlacklistedRaffleUser",
                column: "RaffleConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Raffle_GuildId",
                table: "Raffle",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_RaffleConfig_RaffleId",
                table: "RaffleConfig",
                column: "RaffleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaffleEntry_RaffleId",
                table: "RaffleEntry",
                column: "RaffleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedRaffleUser");

            migrationBuilder.DropTable(
                name: "RaffleEntry");

            migrationBuilder.DropTable(
                name: "RaffleConfig");

            migrationBuilder.DropTable(
                name: "Raffle");
        }
    }
}
