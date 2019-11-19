using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MagBot.DatabaseContexts;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MagBot.Migrations
{
    [DbContext(typeof(GuildDataContext))]
    [Migration("20170721185002_Migration2")]
    partial class Migration2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("MagBot.DatabaseContexts.BlacklistedRaffleUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("RaffleConfigId");

                    b.HasKey("Id");

                    b.HasIndex("RaffleConfigId");

                    b.ToTable("BlacklistedRaffleUsers");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Guild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DiscordIdLong");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Raffle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChannelLong");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int>("GuildId");

                    b.Property<long>("OwnerLong");

                    b.Property<bool>("Started");

                    b.Property<DateTime>("StartedAt");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Raffles");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleConfig", b =>
                {
                    b.Property<int>("RaffleConfigId")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("Length");

                    b.Property<string>("Prize");

                    b.Property<int>("RaffleId");

                    b.Property<int>("WinnerCount");

                    b.HasKey("RaffleConfigId");

                    b.HasIndex("RaffleId")
                        .IsUnique();

                    b.ToTable("RaffleConfigs");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("RaffleId");

                    b.HasKey("Id");

                    b.HasIndex("RaffleId");

                    b.ToTable("RaffleEntries");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("TagListId");

                    b.Property<int?>("TagListId1");

                    b.Property<string>("TagString");

                    b.HasKey("Id");

                    b.HasIndex("TagListId1");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.TagList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("GuildId");

                    b.Property<string>("Keyword");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("TagLists");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.BlacklistedRaffleUser", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.RaffleConfig", "RaffleConfig")
                        .WithMany("BlacklistedUsers")
                        .HasForeignKey("RaffleConfigId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Raffle", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.Guild", "Guild")
                        .WithMany("Raffles")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleConfig", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.Raffle", "Raffle")
                        .WithOne("Config")
                        .HasForeignKey("MagBot.DatabaseContexts.RaffleConfig", "RaffleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleEntry", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.Raffle", "Raffle")
                        .WithMany("RaffleEntries")
                        .HasForeignKey("RaffleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Tag", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.TagList", "TagList")
                        .WithMany("Tags")
                        .HasForeignKey("TagListId1");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.TagList", b =>
                {
                    b.HasOne("MagBot.DatabaseContexts.Guild", "Guild")
                        .WithMany("TagLists")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
