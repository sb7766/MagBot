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
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public CoreModule(CommandService commands, IServiceProvider provider)
        {
            _commands = commands;
            _provider = provider;
        }

        [Command("info")]
        [Summary("Returns info about the bot.")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username}#{application.Owner.Discriminator} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        [Command("help")]
        [Summary("Lists commands or displays information about specific commands.")]
        public async Task Help([Remainder] string command)
        {
            var cmd = _commands.Commands.FirstOrDefault(c => c.Aliases.Any(a => a == command));
            var mod = _commands.Modules.FirstOrDefault(m => m.Aliases.Any(a => a == command));
            var cmdResult = PreconditionResult.FromError("");
            var modResult = PreconditionResult.FromError("");

            if (cmd != null || mod != null)
            {
                EmbedBuilder embed = new EmbedBuilder
                {
                    Color = new Color(50, 200, 50)
                };

                if (cmd != null)
                {
                    cmdResult = await cmd?.CheckPreconditionsAsync(Context);
                    if (cmdResult.IsSuccess)
                    {
                        var cmds = _commands.Commands.Where(cm => cm.Aliases.Any(al => al == command)).ToList();

                        embed.AddField(new EmbedFieldBuilder
                        {
                            Name = cmd.Aliases.First(),
                            Value = cmd.Summary
                        });

                        if (cmd.Aliases.Count > 1)
                        {
                            embed.AddField(new EmbedFieldBuilder
                            {
                                Name = "Aliases",
                                Value = $"`{string.Join("`, `", cmd.Aliases)}`",
                                IsInline = false
                            });
                        }

                        string usage = "";
                        foreach (var usecmd in cmds)
                        {
                            usage += $"`{command}";
                            if (usecmd.Parameters.Count > 0)
                            {
                                var cmdNames = new List<string>();
                                foreach (var param in usecmd.Parameters)
                                {
                                    if (param.IsOptional)
                                    {
                                        cmdNames.Add($"[{param.Name}]");
                                    }
                                    else if (param.IsMultiple || param.IsRemainder)
                                    {
                                        cmdNames.Add($"<-{param.Name}->");
                                    }
                                    else
                                    {
                                        cmdNames.Add($"<{param.Name}>");
                                    }
                                }

                                usage += $" {string.Join(" ", cmdNames)}`\n";
                            }
                            else usage += "`\n";
                        }
                        

                        embed.AddField(new EmbedFieldBuilder
                        {
                            Name = "Usage",
                            Value = usage,
                            IsInline = false
                        });
                    }
                }

                if (mod != null)
                {
                    modResult = await mod?.CheckPreconditionsAsync(Context, _provider);
                    if (modResult.IsSuccess)
                    {
                        if (cmd == null)
                        {
                            embed.AddField(new EmbedFieldBuilder
                            {
                                Name = mod.Aliases.First(),
                                Value = "This command is a group only. Sub-Commands are listed below.",
                                IsInline = false
                            });
                        }
                        List<string> comList = mod.Commands.Where(c => c.CheckPreconditionsAsync(Context).Result.IsSuccess && c.Name != "").Select(c => c.Name).ToList();
                        foreach (var subMod in mod.Submodules)
                        {
                            comList.Add($"{subMod.Name.ToLower()}*");
                        }
                        comList = comList.Distinct().ToList();
                        string coms = $"`{string.Join("`, `", comList)}`";
                        embed.AddField(new EmbedFieldBuilder
                        {
                            Name = "Sub-Commands",
                            Value = coms,
                            IsInline = false
                        });
                    }
                }

                if(!cmdResult.IsSuccess && !modResult.IsSuccess)
                {
                    throw new Exception("You do not have the permission to access that command or group.");
                }

                embed.Build();
                await ReplyAsync("", false, embed);
            }
            else
            {
                throw new Exception("Invalid command.");
            }
            

            
        }
            

        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(50, 200, 50),
                Description = "Here is a list of available commands. A \"*\" indicates a command group."
            };

            foreach (ModuleInfo mod in _commands.Modules.Where(m => !m.IsSubmodule))
            {
                EmbedFieldBuilder modField = new EmbedFieldBuilder();
                modField.IsInline = false;
                string name = mod.Name;
                modField.Name = name;
                var comList = new List<string>();
                if (!string.IsNullOrWhiteSpace(mod.Aliases.First()))
                    comList.Add($"{mod.Name.ToLower()}*");
                else
                {
                    comList.AddRange(mod.Commands.Where(c => c.CheckPreconditionsAsync(Context).Result.IsSuccess).Select(c => c.Aliases.First()).ToList());
                    foreach (var subMod in mod.Submodules)
                    {
                        comList.Add($"{subMod.Name.ToLower()}*");
                    }
                }
                if (comList == null || comList.Count() == 0) continue;
                comList = comList.Distinct().ToList();
                string coms = $"`{string.Join("`, `", comList)}`";
                modField.Value = coms;
                embed.AddField(modField);
            }
            embed.Build();
            await ReplyAsync("", false, embed);
        }

        [Command("leaveguild")]
        [Summary("Makes the bot leave the server.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        public async Task LeaveGuild()
        {
            await ReplyAsync($"Farewell, users of {Context.Guild.Name}!");
            var application = await Context.Client.GetApplicationInfoAsync();
            var ownerchannel = await Context.Client.GetDMChannelAsync(application.Owner.Id);
            await ownerchannel.SendMessageAsync($"Mag-Bot has left server {Context.Guild.Name} ({Context.Guild.Id})");
            await Context.Guild.LeaveAsync();
        }
    }
}
