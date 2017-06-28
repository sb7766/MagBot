using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using MagBot.Services;

namespace MagBot
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private Program program;
        private RaffleService raffles;
        private IDependencyMap map;

        public async Task Install(IDependencyMap _map)
        {
            // Put the service in the map
            client = _map.Get<DiscordSocketClient>();
            program = _map.Get<Program>();
            commands = new CommandService();
            raffles = new RaffleService();
            _map.Add(commands);
            _map.Add(raffles);
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

            string commandLog = $"{context.User.Username} in ";
            if(context.IsPrivate)
            {
                commandLog += "PRIVATE CHAT";
            }
            else
            {
                commandLog += context.Guild.Name;
            }

            commandLog += $": {context.Message}";

            await LogHandler.Log(commandLog, LogSeverity.Info, "Command");
            // Execute command
            var result = await commands.ExecuteAsync(context, argPos, map);

            if (!result.IsSuccess)
            {
                var cmdString = context.Message.ToString().Substring(argPos);
                if (result.ErrorReason == "Unknown command." && commands.Modules.Any(m => m.Aliases.Any(a => a == cmdString)))
                {
                    await message.Channel.SendMessageAsync($"**Error:** This command is a group only, and cannot be used on its own. Please use `m!help {cmdString}` for info about the group.");
                }
                else
                {
                    await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
                    await LogHandler.Log($"{result.ErrorReason}", LogSeverity.Error, "Command Error");
                }
            }
        }
    }
}
