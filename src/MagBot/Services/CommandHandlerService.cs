using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using MagBot.Services;
using MagBot.DatabaseContexts;
using Microsoft.Extensions.Configuration;

namespace MagBot.Services
{
    public class CommandHandlerService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private IServiceProvider _provider;
        private readonly GuildDataContext _sunburst;

        public CommandHandlerService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, IConfiguration config, GuildDataContext sunburst)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _config = config;
            _sunburst = sunburst;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Add additional initialization code here...
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            string customPrefix = _sunburst.Guilds.FirstOrDefault(g => g.DiscordId == _discord.Guilds.FirstOrDefault(gu => gu.Channels.FirstOrDefault(c => c.Id == message.Channel.Id) != null).Id)?.CustomPrefix;
            if (customPrefix != null && customPrefix != "")
            {
                if (!message.HasStringPrefix(customPrefix, ref argPos)) return;
            }
            else if (!message.HasStringPrefix(_config["commandPrefix"], ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());
        }
    }
}
