using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace MagBot.Services
{
    public class ClientConfigService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;

        public ClientConfigService(DiscordSocketClient discord, IConfiguration config)
        {
            _discord = discord;
            _config = config;
        }

        public async Task UpdateGame()
        {
            await _discord.SetGameAsync(_config["currentGame"]);
        }

        public async void Shutdown()
        {
            await _discord.SetStatusAsync(UserStatus.Invisible);
            await _discord.StopAsync();
            await _discord.LogoutAsync();
            Environment.Exit(0);
        }
    }
}
