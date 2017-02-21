using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace MagBot
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IDependencyMap map;

        public async Task Install(IDependencyMap _map)
        {
            // Put the service in the map
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();
            _map.Add(commands);
            map = _map;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage paramMessage)
        {
            // Make sure message is from a user
            var message = paramMessage as SocketUserMessage;
            if (message == null) return;

            // Start of args
            int argPos = 0;
            // Check for prefix
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix("m!", ref argPos))) return;

            // Create context
            var context = new CommandContext(client, message);
            // Execute command
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess)
            {
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
                await Program.LogCustom($"Command Error: {result.ErrorReason}", LogSeverity.Error);
            }
        }
    }
}
