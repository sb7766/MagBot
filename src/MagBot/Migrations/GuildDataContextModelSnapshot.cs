using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MagBot.DatabaseContexts;

namespace MagBot.Migrations
{
    [DbContext(typeof(GuildDataContext))]
    partial class GuildDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("MagBot.DatabaseContexts.Guild", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
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
