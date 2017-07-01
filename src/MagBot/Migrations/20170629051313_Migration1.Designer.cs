using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MagBot.DatabaseContexts;

namespace MagBot.Migrations
{
    [DbContext(typeof(GuildDataContext))]
    [Migration("20170629051313_Migration1")]
    partial class Migration1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("MagBot.DatabaseContexts.BlacklistedRaffleUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("RaffleConfigId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("RaffleConfigId");

                    b.ToTable("BlacklistedRaffleUser");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Guild", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.Raffle", b =>
                {
                    b.Property<int>("RaffleId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("Channel");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("Owner");

                    b.Property<bool>("Started");

                    b.Property<DateTime>("StartedAt");

                    b.HasKey("RaffleId");

                    b.HasIndex("GuildId");

                    b.ToTable("Raffle");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleConfig", b =>
                {
                    b.Property<int>("RaffleConfigId")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("Length");

                    b.Property<string>("Prize");

                    b.Property<int>("RaffleId");

                    b.Property<ulong>("WhiteListedRole");

                    b.Property<int>("WinnerCount");

                    b.HasKey("RaffleConfigId");

                    b.HasIndex("RaffleId")
                        .IsUnique();

                    b.ToTable("RaffleConfig");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.RaffleEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("RaffleId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("RaffleId");

                    b.ToTable("RaffleEntry");
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

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("MagBot.DatabaseContexts.TagList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Keyword");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("TagList");
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
