using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot;
using Newtonsoft.Json.Linq;
using System.IO;

namespace MagBot.Modules
{
    [Name("Admin")]
    [RequireOwner]
    public class AdminModule : ModuleBase
    {
        private Program program;

        public AdminModule(Program _program)
        {
            program = _program;
        }

        [Command("setgame")]
        [Summary("Set's the bot's game.")]
        public async Task SetGame(string game)
        {
            var json = JObject.Parse(File.ReadAllText("./Resources/BotConfig.json"));
            json["currentGame"] = game;
            File.WriteAllText("./Resources/BotConfig.json", json.ToString());
            await program.UpdateGame();
            await ReplyAsync("Game updated!");
        }

        [Command("shutdown")]
        [Summary("Shuts the bot down.")]
        public async Task Shutdown()
        {
            await ReplyAsync("Shutting down...");
            program.Shutdown();
        }

        [Command("restart")]
        [Summary("Restarts the bot.")]
        public async Task Restart()
        {
            await ReplyAsync("Restarting...");
            program.Restart();
        }
    }
}
