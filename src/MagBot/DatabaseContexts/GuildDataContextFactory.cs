using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Design;

namespace MagBot.DatabaseContexts
{
    public class GuildDataContextFactory : IDesignTimeDbContextFactory<GuildDataContext>
    {
        public GuildDataContext CreateDbContext(string[] options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GuildDataContext>();
            JObject json = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Config/DevSecrets.json")));
            string connection;

            if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Development")
            {
                connection = json["ConnectionStrings"]["Sunburst"].ToString();
            }
            else if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Migration")
            {
                connection = json["ConnectionStrings"]["SunburstRemote"].ToString();
            }
            else throw new Exception("Invalid environment. SUNBURST_ENV must be Development or Migration.");
;
            optionsBuilder.UseNpgsql(connection);

            return new GuildDataContext(optionsBuilder.Options);
        }
    }
}
