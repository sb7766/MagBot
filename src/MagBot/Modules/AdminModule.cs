using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using System.Net;
using MagBot;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using MagBot.Services;
using Newtonsoft.Json;
using System.Net.Http;

namespace MagBot.Modules
{
    [Name("Admin")]
    [RequireOwner]
    public class AdminModule : ModuleBase
    {
        private readonly ClientConfigService _configService;
        private readonly IConfiguration _config;

        public AdminModule(ClientConfigService configService, IConfiguration config)
        {
            _configService = configService;
            _config = config;
        }

        [Command("setgame")]
        [Summary("Sets the bot's game.")]
        public async Task SetGame([Remainder] string game)
        {
            _config["currentGame"] = game;
            await _configService.UpdateGame();
            await ReplyAsync("Game updated!");
        }

        [Command("setname")]
        [Summary("Sets the bot's global name.")]
        public async Task SetName([Remainder] string name)
        {
            await Context.Client.CurrentUser.ModifyAsync((c) =>
            {
                c.Username = name;
            });
            await ReplyAsync("Username updated!");
        }

        [Command("setavatar")]
        [Summary("Sets the bot's avatar. Image must be attached and be jpg or png.")]
        public async Task SetAvatar()
        {
            var attachment = Context.Message.Attachments.FirstOrDefault(atc => atc.Filename.EndsWith(".png") || atc.Filename.EndsWith(".jpg"));
            if (attachment == null)
            {
                throw new Exception("No attachment of correct type.");
            }

            var img = await new HttpClient().GetStreamAsync(attachment.Url);
            await Context.Client.CurrentUser.ModifyAsync((c) =>
            {
                c.Avatar = new Discord.Image(img);
            });
            await ReplyAsync("Avatar updated!");
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
