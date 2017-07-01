using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using Discord.Commands;
using MagBot.DatabaseContexts;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using MagBot.Services;
using Microsoft.Extensions.Configuration;

namespace MagBot
{
    public class Program
    {
        // Invoke glorious async
        public static void Main() =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task Start()
        {
            // Define client
            _client = new DiscordSocketClient();
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlerService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            services.GetRequiredService<RaffleService>().Init();

            _client.Connected += (async () =>
            {
                await services.GetRequiredService<ClientConfigService>().UpdateGame();
            });
            
            _client.GuildAvailable += (async (g) =>
            {
                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(g.Id);

                    if (guild == null)
                    {
                        guild = new Guild
                        {
                            GuildId = g.Id
                        };

                        db.Guilds.Add(guild);

                        await db.SaveChangesAsync();
                    }
                }
            });

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlerService>()
                // Raffles
                .AddSingleton<RaffleService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                .AddSingleton<ClientConfigService>()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("BotConfig.json")
                .Build();
        }
    }
}
