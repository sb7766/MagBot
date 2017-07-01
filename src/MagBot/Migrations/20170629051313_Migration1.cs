using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MagBot.Migrations
{
    public partial class Migration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Raffle",
                columns: table => new
                {
                    RaffleId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Channel = table.Column<ulong>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    Owner = table.Column<ulong>(nullable: false),
                    Started = table.Column<bool>(nullable: false),
                    StartedAt = table.Column<DateTime>(nullable: false)
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
                name: "TagList",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<ulong>(nullable: false),
                    Keyword = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagList_Guilds_GuildId",
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
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagListId = table.Column<string>(nullable: true),
                    TagListId1 = table.Column<int>(nullable: true),
                    TagString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_TagList_TagListId1",
                        column: x => x.TagListId1,
                        principalTable: "TagList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_Tag_TagListId1",
                table: "Tag",
                column: "TagListId1");

            migrationBuilder.CreateIndex(
                name: "IX_TagList_GuildId",
                table: "TagList",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedRaffleUser");

            migrationBuilder.DropTable(
                name: "RaffleEntry");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "RaffleConfig");

            migrationBuilder.DropTable(
                name: "TagList");

            migrationBuilder.DropTable(
                name: "Raffle");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
