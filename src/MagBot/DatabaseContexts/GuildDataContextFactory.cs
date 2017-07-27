using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MagBot.DatabaseContexts
{
    public class GuildDataContextFactory : IDbContextFactory<GuildDataContext>
    {
        public GuildDataContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GuildDataContext>();
            JObject json = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Config/DevSecrets")));
            string connection;

            if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Development")
            {
                connection = json["ConnectionStrings"]["Sunburst"].ToString();
            }
            else if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Migration")
            {
                connection = json["ConnectionStrings"]["SunburstRemote"].ToString();
            }
            else throw new Exception("Invalid environment. Must be Development or Migration.");
;
            optionsBuilder.UseNpgsql(connection);

            return new GuildDataContext(optionsBuilder.Options);
        }
    }
}
