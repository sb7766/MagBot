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
            await LogCustom("Starting client...");
            client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info });

            client.Log += Log;

            var token = File.ReadAllText("Resources/token.txt");

            // Login and connect
            await LogCustom("Connecting to Discord...");
            await client.LoginAsync(TokenType.Bot, token);
            await client.ConnectAsync();
            await LogCustom("Connected!");

            await LogCustom("Installing commands...");
            var map = new DependencyMap();
            map.Add(client);
            map.Add(this);

            handler = new CommandHandler();
            await handler.Install(map);

            await LogCustom("Commands installed!");
            await LogCustom("Loading configs...");

            await UpdateGame();

            await LogCustom("Configs loaded.");

            await LogCustom("Initalizing guilds...");
            client.GuildAvailable += (async (g) =>
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
            await LogCustom("Guilds initialized.");

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine($"[{DateTime.Now}][{msg.Severity}][{msg.Source}] {msg.Message} {msg.Exception}");
            return Task.CompletedTask;
        }

        public Task LogCustom(string message, LogSeverity severity = LogSeverity.Info)
        {
            Log(new LogMessage(severity, "MagBot", message));
            return Task.CompletedTask;
        }

        public async Task UpdateGame()
        {
            var json = JObject.Parse(File.ReadAllText("./Resources/BotConfig.json"));
            string game = (string)json["currentGame"];
            await client.SetGameAsync(game);
        }

        public void Shutdown()
        {
            client.DisconnectAsync();
            Environment.Exit(0);
        }

        public void Restart()
        {
            client.DisconnectAsync();
            Main();
        }
    }
}
