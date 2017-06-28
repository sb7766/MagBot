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
using Hangfire;

namespace MagBot
{
    public class Program
    {
        // Invoke glorious async
        public static void Main() =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;

        public async Task Start()
        {
            // Define client
            await LogHandler.Log("Starting client...");
            client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info });

            client.Log += LogHandler.Log;

            var token = File.ReadAllText("Resources/token.txt");

            // Login and connect
            await LogHandler.Log("Connecting to Discord...");
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await LogHandler.Log("Installing commands...");
            var map = new DependencyMap();
            map.Add(client);
            map.Add(this);

            handler = new CommandHandler();
            await handler.Install(map);

            await LogHandler.Log("Commands installed!");

            await LogHandler.Log("Initializing Hangfire...");

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(@"Server=(LocalDb)\mssqllocaldb; Database=Hangfire;");

            client.Connected += (async () =>
            {
                await UpdateGame();
            });
            
            client.GuildAvailable += (async (g) =>
            {
                await LogHandler.Log($"Guild Available: {g.Name}");
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

        public async Task UpdateGame()
        {
            var json = JObject.Parse(File.ReadAllText("./Resources/BotConfig.json"));
            string game = (string)json["currentGame"];
            await client.SetGameAsync(game);
            await LogHandler.Log($"Game set to: {game}");
        }

        public async void Shutdown()
        {
            await client.SetStatusAsync(UserStatus.Offline);
            await client.StopAsync();
            await client.LogoutAsync();
            Environment.Exit(0);
        }
    }
}
