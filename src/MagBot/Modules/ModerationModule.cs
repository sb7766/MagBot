using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot.DatabaseContexts;
using Discord.Addons.Interactive;
//using Microsoft.EntityFrameworkCore;
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

                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);
                var dm = await user.GetOrCreateDMChannelAsync();

                if (guild.ReportingEnabled)
                {
                    var msg = await dm.SendMessageAsync($"```md\n# You have opened a reply in the {Context.Channel.Name} channel of {Context.Guild.Name}.\n" +
                        $"# You have 1 minute to type out your report.\n" +
                        $"Type 'cancel' to cancel.```");

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
                            await channel.SendMessageAsync($"{role.Mention}", embed: new EmbedBuilder
                            {
                                Title = "Report",
                                Color = Color.Red,
                                Description = $"Report created in channel { Context.Channel.Name } by { Context.User.Mention } (ID: { Context.User.Id }) on { Context.Message.Timestamp.ToString("R")}.",
                                Fields = { new EmbedFieldBuilder { Name = "Report Text", Value = $"{ response.Content }" } }
                            }.Build());

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
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ReportEnable()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

                var msg = await ReplyAsync("```md\n# Setting up reporting. Please mention the role you would like reports sent to:\n" +
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
                        await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(10));
                        return;
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("Invalid response, be sure to mention a role.", timeout: TimeSpan.FromSeconds(10));
                        goto GetRoleResponse;
                    }
                }
                else
                {
                    await msg.DeleteAsync();
                    await ReplyAndDeleteAsync("Request timed out.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                await msg.DeleteAsync();
                msg = await ReplyAsync("```md\n# Role set. Please mention the channel you would like reports sent to:\n" +
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
                            await ReplyAndDeleteAsync("I do not have permission to send messages to that channel.", timeout: TimeSpan.FromSeconds(10));
                            goto GetChannelResponse;
                        }
                    }
                    else if (response.Content.ToLower() == "cancel")
                    {
                        await msg.DeleteAsync();
                        await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(10));
                        return;
                    }
                    else
                    {
                        await ReplyAndDeleteAsync("Invalid response, be sure to mention a channel.", timeout: TimeSpan.FromSeconds(10));
                        goto GetChannelResponse;
                    }
                }
                else
                {
                    await msg.DeleteAsync();
                    await ReplyAndDeleteAsync("Request timed out.", timeout: TimeSpan.FromSeconds(10));
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
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ReportDisable()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

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

        [Group("selfrole")]
        [Name("SelfRole")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public class SelfRoleModule : ModuleBase
        {
            private readonly GuildDataContext _sunburstdb;

            public SelfRoleModule(GuildDataContext sunburstdb)
            {
                _sunburstdb = sunburstdb;
            }

            [Command("add")]
            [Summary("Adds a self-assignable role. Role must exist.")]
            public async Task SelfRoleAdd([Remainder] string rolename)
            {
                IRole Role = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToLower() == rolename.ToLower());

                if (Role != null)
                {
                    var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

                    await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

                    var selfrole = guild.SelfAssignedRoles.FirstOrDefault(r => r.RoleId == Role.Id);

                    if (selfrole == null)  
                    {
                        selfrole = new SelfAssignedRole
                        {
                            RoleId = Role.Id,
                            Name = Role.Name
                        };
                        guild.SelfAssignedRoles.Add(selfrole);
                        await _sunburstdb.SaveChangesAsync();
                        await ReplyAsync($"Role \"{Role.Name}\" was made self-assignable.");
                    }
                    else
                    {
                        await ReplyAsync("Role already self-assignable.");
                    }
                } 
                else
                {
                    await ReplyAsync("Role does not exist.");
                }
            }

            [Command("remove")]
            [Summary("Removes a self-assignable role. Role must exist and be self-assignable.")]
            public async Task SelfRoleRemove([Remainder] string rolename)
            {
                IRole Role = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToLower() == rolename.ToLower());

                if (Role != null)
                {
                    var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

                    await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

                    var selfrole = guild.SelfAssignedRoles.FirstOrDefault(r => r.RoleId == Role.Id);

                    if (selfrole != null)
                    {
                        guild.SelfAssignedRoles.Remove(selfrole);
                        await _sunburstdb.SaveChangesAsync();
                        await ReplyAsync($"Role \"{Role.Name}\" was made not self-assignable.");
                    }
                    else
                    {
                        await ReplyAsync("Role already not self-assignable.");
                    }
                }
                else
                {
                    await ReplyAsync("Role does not exist.");
                }
            }

            [Command("clear")]
            [Summary("Clears all self-assigned roles.")]
            public async Task SelfRoleClear()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);
                await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();
                guild.SelfAssignedRoles.Clear();
                await _sunburstdb.SaveChangesAsync();

                await ReplyAsync("All self-assignable roles cleared.");
            }

            [Command("list")]
            [Summary("Lists all self-assigned roles.")]
            public async Task SelfRoleList()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

                string response = "Here are the self-assignable roles in this server:\n";

                foreach (var sr in guild.SelfAssignedRoles)
                {
                    response += $"-{sr.Name} (ID: {sr.RoleId})\n";
                }

                await ReplyAsync(response);
            }
        }

        [Command("iam")]
        [Summary("Gives you a self-assignable role.")]
        public async Task IAm([Remainder] string rolename)
        {
            var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

            await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

            var selfrole = guild.SelfAssignedRoles.FirstOrDefault(r => r.Name.ToLower() == rolename.ToLower());
            
            if (selfrole != null) {
                IRole role = Context.Guild.GetRole(selfrole.RoleId);
                
                if (role != null)
                {
                    var user = await Context.Guild.GetUserAsync(Context.User.Id);
                    if (user.RoleIds.Contains(role.Id))
                    {
                        await ReplyAsync("You already have that role!");
                    }
                    else
                    {
                        await user.AddRoleAsync(role);
                        await ReplyAsync($"You are now \"{role.Name}\".");
                    }
                }
                else
                {
                    await ReplyAsync("Role not found in server but is listed as self-assignable, please contact a server admin.");
                }
            }
            else
            {
                await ReplyAsync("Role not self-assignable.");
            }
        }

        [Command("iamnot")]
        [Summary("Removes a self-assignable role from you.")]
        public async Task IAmNot([Remainder] string rolename)
        {
            var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

            await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

            var selfrole = guild.SelfAssignedRoles.FirstOrDefault(r => r.Name.ToLower() == rolename.ToLower());

            if (selfrole != null)
            {
                IRole role = Context.Guild.GetRole(selfrole.RoleId);

                if (role != null)
                {
                    var user = await Context.Guild.GetUserAsync(Context.User.Id);
                    if (!user.RoleIds.Contains(role.Id))
                    {
                        await ReplyAsync("You don't have that role!");
                    }
                    else
                    {
                        await user.RemoveRoleAsync(role);
                        await ReplyAsync($"You are no longer \"{role.Name}\".");
                    }
                }
                else
                {
                    await ReplyAsync("Role not found in server but is listed as self-assignable, please contact a server admin.");
                }
            }
            else
            {
                await ReplyAsync("Role not self-assignable.");
            }
        }

        [Command("selfroles")]
        [Summary("Lists available selfroles.")]
        public async Task SelfrolesList()
        {
            var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(g => g.DiscordIdLong == (long)Context.Guild.Id);

            await _sunburstdb.Entry(guild).Collection(g => g.SelfAssignedRoles).LoadAsync();

            string response = "Here are the self-assignable roles in this server:\n";

            foreach (var sr in guild.SelfAssignedRoles)
            {
                response += $"-{sr.Name}\n";
            }

            await ReplyAsync(response);
        }
    }
}
