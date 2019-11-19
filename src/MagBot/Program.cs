using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using Discord.Commands;
using MagBot.DatabaseContexts;
using Microsoft.Extensions.DependencyInjection;
using MagBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using Discord.Addons.Interactive;
using Microsoft.EntityFrameworkCore;

namespace MagBot
{
    public class Program
    {
        // Invoke glorious async
        public static void Main() =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;
        private Timer reconTimer;

        public async Task Start()
        {
            // Define client
            _client = new DiscordSocketClient( new DiscordSocketConfig
            {
                
            });
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlerService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            services.GetRequiredService<RaffleService>().Init();
            new Thread(services.GetRequiredService<ConsoleCommandService>().Init).Start();

            _client.Connected += (async () =>
            {
                await services.GetRequiredService<ClientConfigService>().UpdateGame();
            });
            
            _client.GuildAvailable += (async (g) =>
            {
                using (var scope = services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<GuildDataContext>();

                    //var db = services.GetRequiredService<GuildDataContext>();

                    var guild = await db.Guilds.FirstOrDefaultAsync(gu => gu.DiscordId == g.Id);

                    if (guild == null)
                    {
                        guild = new Guild
                        {
                            DiscordId = g.Id
                        };

                        await db.Guilds.AddAsync(guild);
                    }

                    await db.SaveChangesAsync();
                }
            });

            reconTimer = new Timer(async (e) =>
            {
                if (_client.ConnectionState == ConnectionState.Disconnected)
                {
                    await _client.StartAsync();
                }
            }, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<InteractiveService>()
                // Raffles
                .AddSingleton<RaffleService>()
                // Logging
                .AddLogging(builder => builder
                .AddConsole()
                .AddConfiguration(_config.GetSection("Logging")))
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                .AddSingleton<ClientConfigService>()
                .AddSingleton<ConsoleCommandService>()
                .AddEntityFrameworkNpgsql()
                .AddDbContext<GuildDataContext>(options => options.UseNpgsql(_config.GetConnectionString("Sunburst"), o => o.SetPostgresVersion(9,6)))
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Development")
            {
                return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "Config/"))
                .AddJsonFile("BotConfig.json", true, true)
                .AddJsonFile("BotConfigDev.json", true, true)
                // File not included in source control, includes connection strings and tokens
                .AddJsonFile("DevSecrets.json", true, true)
                .Build();
            }
            else if (Environment.GetEnvironmentVariable("SUNBURST_ENV") == "Release")
            {
                return new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory, "Config/"))
                .AddJsonFile("BotConfig.json", true, true)
                .AddJsonFile("BotConfigRelease.json", true, true)
                // File not included in source control, includes connection strings and tokens
                .AddJsonFile("ReleaseSecrets.json", true, true)
                .Build();
            }
            else throw new Exception("Invalid environment. SUNBURST_ENV must be Development or Release.");
        }
    }
}
