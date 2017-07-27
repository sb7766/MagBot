using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MagBot.Services
{
    public class ConsoleCommandService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ClientConfigService _clientConfig;

        public ConsoleCommandService(DiscordSocketClient discord, ClientConfigService clientConfig)
        {
            _discord = discord;
            _clientConfig = clientConfig;
        }

        public async void Init()
        {
            while(true)
            {
                string command = Console.ReadLine();
                
                if (command == "shutdown")
                {
                    _clientConfig.Shutdown();
                }
                else if (command == "recon")
                {
                    await _discord.StartAsync();
                }
            }
        }
    }
}
