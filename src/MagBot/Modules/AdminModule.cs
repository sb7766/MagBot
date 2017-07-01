using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using MagBot.Services;

namespace MagBot.Modules
{
    [Name("Admin")]
    [RequireOwner]
    public class AdminModule : ModuleBase
    {
        private readonly IConfiguration _config;
        private readonly ClientConfigService _configService;

        public AdminModule(IConfiguration config, ClientConfigService configService)
        {
            _config = config;
            _configService = configService;
        }

        [Command("setgame")]
        [Summary("Set's the bot's game.")]
        public async Task SetGame([Remainder] string game)
        {
            _config["currentGame"] = game;
            await _configService.UpdateGame();
            await ReplyAsync("Game updated!");
        }

        [Command("shutdown")]
        [Summary("Shuts the bot down.")]
        public async Task Shutdown()
        {
            await ReplyAsync("Shutting down...");
            _configService.Shutdown();
        }
    }
}
