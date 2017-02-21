using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MagBot.Modules
{
    [Name("Core")]
    public class CoreModule : ModuleBase 
    {
        [Command("info")]
        [Summary("Returns info about the bot.")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        [Command("help")]
        [Summary("Lists commands or displays information about specific commands.")]
        public async Task Help([Summary("An optional command to get info for.")] string command = null)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                var cmd = commands.Commands.FirstOrDefault(c => c.Aliases.Any(a => a == command.ToLower()));
                if (cmd != null)
                {
                    var result = await cmd?.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        EmbedBuilder embed = new EmbedBuilder
                        {
                            Color = new Color(0, 200, 0),
                            Title = cmd.Name,
                            Description = cmd.Summary
                        };

                        if (cmd.Aliases.Count > 1)
                        {
                            embed.AddField(new EmbedFieldBuilder
                            {
                                Name = "Aliases",
                                Value = $"`{string.Join("`, `", cmd.Aliases)}`",
                                IsInline = true
                            });
                        }

                        string usage = $"`{command.ToLower()}";
                        if (cmd.Parameters.Count > 0)
                        {
                            var cmdNames = new List<string>();
                            foreach(var param in cmd.Parameters)
                            {
                                if (param.IsOptional || param.IsMultiple)
                                {
                                    cmdNames.Add($"[{param.Name}]");
                                }
                                else
                                {
                                    cmdNames.Add($"<{param.Name}>");
                                }
                            }

                            usage += $" {string.Join(" ", cmdNames)}";
                        }
                        embed.AddField(new EmbedFieldBuilder
                        {
                            Name = "Usage",
                            Value = usage += '`',
                            IsInline = true
                        });
                        embed.Build();
                        await ReplyAsync("", false, embed);
                    }
                    else if (!result.IsSuccess)
                    {
                        await ReplyAsync("You do not have permission to access this command.");
                    }
                    else await ReplyAsync("Invalid command.");
                }
                else await ReplyAsync("Invalid command.");
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Color = new Color(0, 200, 0),
                    Description = "Here are all the commands you can use:"
                };
                foreach (ModuleInfo mod in commands.Modules)
                {
                    EmbedFieldBuilder modField = new EmbedFieldBuilder();
                    modField.IsInline = true;
                    modField.Name = mod.Name;
                    var comList = mod.Commands.Where(c => c.CheckPreconditionsAsync(Context).Result.IsSuccess).Select(c => c.Name);
                    if (comList == null || comList.Count() == 0) continue;
                    string coms = $"`{string.Join("`, `", comList)}`";
                    modField.Value = coms;
                    embed.AddField(modField);
                }
                embed.Build();
                await ReplyAsync("", false, embed);
            }
        }

        [Command("leaveguild")]
        [Summary("Makes the bot leave the server.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task LeaveGuild()
        {
            await ReplyAsync($"Farewell, users of {Context.Guild.Name}!");
            await Context.Guild.LeaveAsync();
            await Program.LogCustom($"Mag-Bot has left server {Context.Guild.Name} ({Context.Guild.Id})");
        }

        private CommandService commands;

        public CoreModule(CommandService _commands)
        {
            commands = _commands;
        }
    }
}
