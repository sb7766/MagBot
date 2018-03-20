using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot.DatabaseContexts;
using Discord.Addons.Interactive;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using static MagBot.Extensions;
using Discord;

namespace MagBot.Modules
{
    [Name("Moderation")]
    [RequireContext(ContextType.Guild)]
    public class ModerationModule : ModuleBase
    {
        private readonly GuildDataContext _sunburstdb;

        public ModerationModule(GuildDataContext sunburstdb)
        {
            _sunburstdb = sunburstdb;
        }

        [Group("report")]
        [Name("Report")]
        public class ReportModule : InteractiveBase
        {
            private readonly GuildDataContext _sunburstdb;

            public ReportModule(GuildDataContext sunburstdb)
            {
                _sunburstdb = sunburstdb;
            }

            [Command("", RunMode = RunMode.Async)]
            [Priority(1)]
            [Summary("If reporting is set up, this will open a prompt in DM to allow reports to be filed to server admins.")]
            public async Task Report()
            {
                var user = Context.User;
                await Context.Message.DeleteAsync();

                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordId == Context.Guild.Id);
                var dm = await user.GetOrCreateDMChannelAsync();

                if (guild.ReportingEnabled)
                {
                    var msg = await dm.SendMessageAsync($"```md\n# You have opened a report in the {Context.Channel.Name} channel of {Context.Guild.Name}.\n\n" +
                        $"# You have 1 minute to type out your report. Please do so now.\n\n" +
                        $"Type 'cancel' to cancel the report.```");

                    var response = await NextMessageAsync(new EnsureSpecifiedChannel(dm), timeout: TimeSpan.FromMinutes(1));

                    if (response != null)
                    {
                        if (response.Content.ToLower() == "cancel")
                        {
                            await msg.DeleteAsync();
                            await dm.SendMessageAsync("Report cancelled.");
                            return;
                        }
                        else
                        {
                            await msg.DeleteAsync();
                            var channel = Context.Guild.GetTextChannel(guild.ReportingChannelId);
                            var role = Context.Guild.GetRole(guild.ReportingRoleId);

                            guild.ReportNumber++;

                            await channel.SendMessageAsync($"{role.Mention}", embed: new EmbedBuilder
                            {
                                Title = $"Report #{ guild.ReportNumber }",
                                Color = Color.Red,
                                Description = $"Report created in channel { Context.Channel.Name } by { Context.User.Mention } (ID: { Context.User.Id }) on { Context.Message.Timestamp.ToString("R")}.",
                                Fields = { new EmbedFieldBuilder { Name = "Report Text", Value = $"{ response.Content }" } }
                            }.Build());

                            await _sunburstdb.SaveChangesAsync();

                            await dm.SendMessageAsync("Your report has been delivered.");
                            return;
                        }
                    }
                    else
                    {
                        await msg.DeleteAsync();
                        await dm.SendMessageAsync("Report timed out.");
                        return;
                    }
                }
                else
                {
                    await dm.SendMessageAsync($"Reporting is not enabled in {Context.Guild.Name}. Please contact an admin to enable it.");
                    return;
                }
            }

            [Command("enable", RunMode = RunMode.Async)]
            [Priority(2)]
            [Summary("Sets up/enables reporting for the server. Please be sure to mention the role and channel name properly.")]
            [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
            public async Task ReportEnable()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordId == Context.Guild.Id);

                var msg = await ReplyAsync("```md\n# Setting up reporting. Please mention the role you would like reports sent to:\n\n" +
                    "Type 'cancel' to stop setup.```");

                GetRoleResponse:
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));

                if (response != null)
                {
                    if (response.MentionedRoles.Count > 0)
                    {
                        guild.ReportingRoleId = response.MentionedRoles.FirstOrDefault().Id;
                    }
                    else if (response.Content.ToLower() == "cancel")
                    {
                        await msg.DeleteAsync();
                        await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(5));
                        return;
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("Invalid response, be sure to mention a role.", timeout: TimeSpan.FromSeconds(5));
                        goto GetRoleResponse;
                    }
                }
                else
                {
                    await msg.DeleteAsync();
                    await ReplyAndDeleteAsync("Request timed out.", timeout: TimeSpan.FromSeconds(5));
                    return;
                }

                await msg.DeleteAsync();
                msg = await ReplyAsync("```md\n# Role set. Please mention the channel you would like reports sent to:\n\n" +
                    "Type 'cancel' to stop setup.```");

                GetChannelResponse:
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));

                if (response != null)
                {
                    if (response.MentionedChannels.Count > 0)
                    {
                        var channel = response.MentionedChannels.FirstOrDefault();
                        var perms = Context.Guild.CurrentUser.GetPermissions(channel);
                        if (perms.SendMessages)
                        {
                            guild.ReportingChannelId = response.MentionedChannels.FirstOrDefault().Id;
                        }
                        else
                        {
                            await ReplyAndDeleteAsync("I do not have permission to send messages to that channel.", timeout: TimeSpan.FromSeconds(5));
                            goto GetChannelResponse;
                        }
                    }
                    else if (response.Content.ToLower() == "cancel")
                    {
                        await msg.DeleteAsync();
                        await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(5));
                        return;
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("Invalid response, be sure to mention a channel.", timeout: TimeSpan.FromSeconds(5));
                        goto GetChannelResponse;
                    }
                }
                else
                {
                    await msg.DeleteAsync();
                    await ReplyAndDeleteAsync("Request timed out.", timeout: TimeSpan.FromSeconds(5));
                    return;
                }

                await msg.DeleteAsync();
                guild.ReportingEnabled = true;
                await _sunburstdb.SaveChangesAsync();

                await ReplyAsync("```diff\n+ Reporting is now set up and enabled on this server.```");
            }

            [Command("disable", RunMode = RunMode.Async)]
            [Priority(2)]
            [Summary("Disables reporting for the server.")]
            [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
            public async Task ReportDisable()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordId == Context.Guild.Id);

                if (guild.ReportingEnabled)
                {
                    guild.ReportingEnabled = false;
                    await _sunburstdb.SaveChangesAsync();
                    await ReplyAsync("```diff\n- Reporting is now disabled on this server.```");
                }
                else
                {
                    await ReplyAsync("```md\nReporting is not enabled on this server.```");
                }
            }
        }

    }
}
