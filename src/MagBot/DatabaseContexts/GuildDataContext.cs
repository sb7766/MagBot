using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MagBot.DatabaseContexts
{
    public class GuildDataContext : DbContext
    {
        public GuildDataContext(DbContextOptions<GuildDataContext> options)
            :base(options)
        { }

        public DbSet<Guild> Guilds { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql(_config["connectionString"]);
        //}
    }
}
