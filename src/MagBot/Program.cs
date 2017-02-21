using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace MagBot
{
    public class Program
    {
        // Invoke glorious async
        public static void Main(string[] args) =>
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

            handler = new CommandHandler();
            await handler.Install(map);

            await LogCustom("Commands installed!");
            await LogCustom("Loading configs...");

            await UpdateGame();

            await LogCustom("Configs loaded.");

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine($"[{DateTime.Now}][{msg.Severity}][{msg.Source}] {msg.Message} {msg.Exception}");
            return Task.CompletedTask;
        }

        public static Task LogCustom(string message, LogSeverity severity = LogSeverity.Info)
        {
            Log(new LogMessage(severity, "MagBot", message));
            return Task.CompletedTask;
        }

        public async Task UpdateGame()
        {
            string game = GetConfig.Result["currentGame"];
            await client.SetGameAsync(game);
        }

        public Task<IConfigurationRoot> GetConfig = Task<IConfigurationRoot>.Factory.StartNew(() =>
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath($"{Directory.GetCurrentDirectory()}/Resources/")
                .AddJsonFile("BotConfig.json");

            IConfigurationRoot config = builder.Build();

            return config;
        });
    }
}
