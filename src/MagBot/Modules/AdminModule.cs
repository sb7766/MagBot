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

        public AdminModule(ClientConfigService configService)
        {
            _configService = configService;
        }

        [Command("setgame")]
        [Summary("Sets the bot's game.")]
        public async Task SetGame([Remainder] string game)
        {
            string path = Directory.GetCurrentDirectory() + "/BotConfig.json";
            dynamic jsonObj = JsonConvert.DeserializeObject(File.ReadAllText(path));
            jsonObj["currentGame"] = game;
            File.WriteAllText(path, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
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

            var img = await new HttpClient().GetAsync(attachment.Url);
            Stream imgStream = await img.Content.ReadAsStreamAsync();
            await Context.Client.CurrentUser.ModifyAsync((c) =>
            {
                c.Avatar = new Discord.Image(imgStream);
            });
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
