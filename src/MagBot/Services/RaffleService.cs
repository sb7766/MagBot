using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MagBot.DatabaseContexts;
using System.Threading;
using Discord;

namespace MagBot.Services
{
    public class RaffleService
    {
        private Timer masterTimer;

        private readonly DiscordSocketClient client;

        public RaffleService(DiscordSocketClient _client)
        {
            client = _client;
        }

        // Set up the timer to automatically end and prune raffles
        public void Init()
        {
            masterTimer = new Timer(async (e) =>
            {

                using (var db = new GuildDataContext())
                {    
                    foreach (var g in db.Guilds)
                    {
                        await db.Entry(g).Collection(gu => gu.Raffles).LoadAsync();
                        var raffles = g.Raffles;

                        // List of raffles to be pruned
                        List<Raffle> removelist = raffles.Where(ra => !ra.Started && DateTime.Now - ra.CreatedAt >= TimeSpan.FromMinutes(15)).ToList();

                        foreach (var r in removelist)
                        {
                            var gu = client.GetGuild(g.GuildId);
                            var user = client.GetUser(r.Owner);
                            var ch = await user.CreateDMChannelAsync();
                            await ch.SendMessageAsync($"Your raffle in {gu.Name} was automatically cancelled.");
                            raffles.Remove(r);
                        }

                        // List of raffles that are complete
                        List<Raffle> startedList = raffles.Where(ra => ra.Started).ToList();

                        foreach (var r in startedList)
                        {
                            await db.Entry(r).Reference(ra => ra.Config).LoadAsync();
                            if (DateTime.Now >= r.StartedAt + r.Config.Length)
                            {
                                await DrawWinners(db, r);
                            }
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));
        }

        // Draws winners for the given raffle
        private async Task DrawWinners(GuildDataContext db, Raffle raffle)
        {
            //using (var db = new GuildDataContext())
            //{
                //var guild = db.Guilds.FirstOrDefault(g => g.GuildId == guildId);
                //await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                //var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == raffleId);
                await db.Entry(raffle).Collection(r => r.RaffleEntries).LoadAsync();
                var entries = raffle.RaffleEntries;
                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                var socketGuild = client.GetGuild(raffle.GuildId);
                var channel = socketGuild.GetTextChannel(raffle.Channel);
                var owner = socketGuild.GetUser(raffle.Owner);

                
                if (entries.Count == 0)
                {
                    await channel.SendMessageAsync($"{owner.Mention}'s raffle has ended with no entries.");
                    raffle.Guild.Raffles.Remove(raffle);
                    await db.SaveChangesAsync();
                    return;
                }
                else if (entries.Count > config.WinnerCount) config.WinnerCount = entries.Count;

                List<SocketGuildUser> winnerList = new List<SocketGuildUser>();

                Random rand = new Random();

                for (int i = 0; i < config.WinnerCount; i++)
                {
                    var winner = entries[rand.Next(entries.Count)];
                    var winnerUser = socketGuild.GetUser(winner.UserId);

                    winnerList.Add(winnerUser);
                    entries.Remove(winner);
                }

                string winnerString = string.Join(", ", winnerList.Select(u => u.Mention));

                raffle.Guild.Raffles.Remove(raffle);
                await db.SaveChangesAsync();

                await channel.SendMessageAsync($"{owner.Mention}'s raffle has ended! The winners are: {winnerString}");
            //}
        }

        // Create a new raffle with the basic config
        public async Task New(ICommandContext context, string length, int winners)
        {
            var lengthTime = await ParseLength(length);

            Raffle raffle = new Raffle {
                Config = new RaffleConfig
                {
                    Length = lengthTime, WinnerCount = winners, Prize = "[NOT SET]", WhiteListedRole = 0
                }
            };

            // Add the raffle to the database along with the details about its creation
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffles = guild.Raffles;

                raffles.Add(raffle);
                raffle.Owner = context.User.Id;
                raffle.Channel = context.Channel.Id;
                raffle.CreatedAt = DateTime.Now;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle created for {context.User.Mention}. To start the raffle use `m!raffle start {raffle.RaffleId}`. You can configure additional settings with the sub-commands of 'm!raffle config'. If you do not start the raffle within 15 minutes, it will be cancelled.");
        }

        // Parse the length of a raffle based on a string
        private Task<TimeSpan> ParseLength(string length)
        {
            TimeSpan lengthTime = new TimeSpan();

            char last = length.LastOrDefault();
            length = length.TrimEnd(last);

            double lengthNum;
            double.TryParse(length, out lengthNum);
            
            if(lengthNum.Equals(null))
            {
                throw new Exception("Unable to parse length!");
            }
            else
            {
                switch (last)
                {
                    case 's':
                        lengthTime = TimeSpan.FromSeconds(lengthNum);
                        break;
                    case 'm':
                        lengthTime = TimeSpan.FromMinutes(lengthNum);
                        break;
                    case 'h':
                        lengthTime = TimeSpan.FromHours(lengthNum);
                        break;
                    default:
                        throw new Exception("Unable to parse length!");
                }

                return Task.FromResult(lengthTime);
            }
        }

        // Allow the raffle owner to cancel their raffle manually
        public async Task Cancel(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                else if (raffle.Owner != context.User.Id)
                {
                    throw new Exception("You are not the raffle owner.");
                }
                else if (raffle.Started)
                {
                    throw new Exception($"Raffle already started, please use `m!raffle end {id} true` to cancel without picking winners.");
                }

                guild.Raffles.Remove(raffle);

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"{context.User.Mention}'s raffle was cancelled.");
        }

        // Check if configuration should be allowed
        private void ConfigCheck(ICommandContext context, Raffle raffle)
        {
            if (raffle == null)
            {
                throw new Exception("Raffle does not exist.");
            }
            else if (raffle.Owner != context.User.Id)
            {
                throw new Exception("You are not the raffle owner.");
            }
            else if (raffle.Started)
            {
                throw new Exception("Raffle already started, cannot edit config.");
            }
        }

        // Get the current config for a given raffle and package it into an embed
        public async Task ConfigGet(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;
                await db.Entry(config).Collection(c => c.BlacklistedUsers).LoadAsync();

                var users = context.Guild.GetUsersAsync().Result.ToList();
                List<string> blacklisted = users.Where(u => config.BlacklistedUsers.Select(us => us.UserId).Contains(u.Id)).Select(u => u.Username).ToList();
                string blacklist = string.Join(", ", blacklisted);

                string whitelist = context.Guild?.GetRole(config.WhiteListedRole)?.Name;

                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = "Raffle Config",
                    Description = $"Current config for raffle ID: {raffle.RaffleId}",
                    Color = new Color(16, 79, 181)
                };

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Prize",
                    Value = config.Prize
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Length",
                    Value = config.Length.ToString()
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Winners",
                    Value = config.WinnerCount
                });

                if (whitelist != null)  builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Whitelisted Role",
                    Value = whitelist
                });

                if (blacklist != "") builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Blacklisted Users",
                    Value = blacklist
                });

                await context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }


        // Set the prize for a raffle
        public async Task ConfigPrize(ICommandContext context, int id, string prize)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                config.Prize = prize;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle prize set to \"{prize}\".");
        }

        // Set the number of winners for a raffle
        public async Task ConfigWinners(ICommandContext context, int id, int winners)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                config.WinnerCount = winners;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle winners set to {winners}.");
        }

        // Set the length of a raffle
        public async Task ConfigLength(ICommandContext context, int id, string length)
        {
            TimeSpan parsedLength = await ParseLength(length);

            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                config.Length = parsedLength;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle length set to {parsedLength}.");
        }

        // Set the whitelisted role for a raffle
        public async Task ConfigWhitelist(ICommandContext context, int id, string rawRole)
        {
            var role = context.Guild.Roles.FirstOrDefault(r => r.Name.ToLower() == rawRole.ToLower());
            if (role == null)
            {
                throw new Exception("Role does not exist.");
            }

            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                config.WhiteListedRole = role.Id;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle whitelisted role set to \"{role.Name}\".");
        }

        // Add a user to a raffle's blacklist
        public async Task ConfigBlacklistAdd(ICommandContext context, int id, string rawUser)
        {

            var users = context.Guild.GetUsersAsync().Result.ToList();
            var user = users.FirstOrDefault(u => u.Nickname == rawUser || u.Username == rawUser);
            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;
                await db.Entry(config).Collection(c => c.BlacklistedUsers).LoadAsync();
                var blacklist = config.BlacklistedUsers;
                
                if (blacklist.Exists(u => u.UserId == user.Id))
                {
                    throw new Exception("User is already in the blacklist.");
                }

                blacklist.Add(new BlacklistedRaffleUser() { UserId = user.Id });

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"User \"{user.Username}\" added to raffle blacklist.");
        }

        // Remove a user from a raffle's blacklist
        public async Task ConfigBlacklistRemove(ICommandContext context, int id, string rawUser)
        {

            var users = context.Guild.GetUsersAsync().Result.ToList();
            var user = users.FirstOrDefault(u => u.Nickname == rawUser || u.Username == rawUser);
            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;
                await db.Entry(config).Collection(c => c.BlacklistedUsers).LoadAsync();
                var blacklist = config.BlacklistedUsers;

                if (!blacklist.Exists(u => u.UserId == user.Id))
                {
                    throw new Exception("User is not in the blacklist.");
                }

                var blacklistedUser = blacklist.FirstOrDefault(u => u.UserId == user.Id);

                blacklist.Remove(blacklistedUser);

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"User \"{user.Username}\" removed from raffle blacklist.");
        }

        // Clear a raffle's blacklist
        public async Task ConfigBlacklistClear(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                ConfigCheck(context, raffle);

                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;

                config.BlacklistedUsers = new List<BlacklistedRaffleUser>();

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Cleared the raffle's blacklist.");
        }

        // Sets a raffle as started
        public async Task Start(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                else if (raffle.Owner != context.User.Id)
                {
                    throw new Exception("You are not the raffle owner.");
                }
                else if (raffle.Started)
                {
                    throw new Exception("Raffle already started, please use `m!raffle end true` to cancel without picking winners.");
                }

                raffle.StartedAt = DateTime.Now;
                raffle.Started = true;
                raffle.Channel = context.Channel.Id;

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"{context.User.Mention} has started a raffle! Use `m!raffle enter {id}` to enter and `m!raffle info {id}` for more information about the raffle.");
        }

        // Allow the raffle owner to end a raffle before the end time
        public async Task End(ICommandContext context, int id, bool noDraw)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                else if (raffle.Owner != context.User.Id)
                {
                    throw new Exception("You are not the raffle owner.");
                }
                else if (!raffle.Started)
                {
                    throw new Exception($"Raffle not yet started, please use `m!raffle cancel {id}` to cancel.");
                }

                if (noDraw)
                {
                    var channel = await context.Guild.GetTextChannelAsync(raffle.Channel);
                    guild.Raffles.Remove(raffle);
                    await channel.SendMessageAsync($"{context.User.Mention}'s raffle was ended without picking winners.");
                }
                else
                {
                    await DrawWinners(db, raffle);
                }

                await db.SaveChangesAsync();
            }
        }

        // Enter a user into the raffle
        public async Task Enter(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                //else if (raffle.Owner == context.User.Id)
                //{
                //    throw new Exception("You are the raffle owner!");
                //}
                else if (!raffle.Started)
                {
                    throw new Exception($"Raffle not yet started.");
                }

                await db.Entry(raffle).Collection(r => r.RaffleEntries).LoadAsync();
                var entries = raffle.RaffleEntries;
                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;
                await db.Entry(config).Collection(c => c.BlacklistedUsers).LoadAsync();
                var blacklist = config.BlacklistedUsers;

                var user = context.Guild.GetUserAsync(context.User.Id).Result;

                if (entries.Exists(e => e.UserId == context.User.Id))
                {
                    throw new Exception($"You are already in the raffle, {context.User.Mention}!");
                }
                else if (blacklist.Exists(b => b.UserId == context.User.Id))
                {
                    throw new Exception($"You are in the blacklist for this raffle, {context.User.Mention}!");
                }
                else if (config.WhiteListedRole != 0 && !user.RoleIds.ToList().Exists(r => r == config.WhiteListedRole))
                {
                    throw new Exception($"You do not have the correct role for this raffle, {context.User.Mention}!");
                }

                entries.Add(new RaffleEntry() { UserId = context.User.Id });

                await context.Channel.SendMessageAsync($"You're in, {context.User.Mention}!");

                await db.SaveChangesAsync();
            }
        }

        // Remove a user from the raffle
        public async Task Leave(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                //else if (raffle.Owner == context.User.Id)
                //{
                //    throw new Exception("You are the raffle owner!");
                //}
                else if (!raffle.Started)
                {
                    throw new Exception($"Raffle not yet started.");
                }

                await db.Entry(raffle).Collection(r => r.RaffleEntries).LoadAsync();
                var entries = raffle.RaffleEntries;

                if (!entries.Exists(e => e.UserId == context.User.Id))
                {
                    throw new Exception($"You are not in the raffle, {context.User.Mention}!");
                }

                entries.Remove(entries.FirstOrDefault(e => e.UserId == context.User.Id));

                await context.Channel.SendMessageAsync($"{context.User.Mention} has left the raffle.");

                await db.SaveChangesAsync();
            }
        }

        // List the raffles in a server
        public async Task List(ICommandContext context)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffles = guild.Raffles;

                if (raffles.Count == 0)
                {
                    throw new Exception("There are no raffles on this guild.");
                }

                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = "Raffles",
                    Description = "These are the raffles for the current guild.",
                    Color = new Color(16, 79, 181)
                };

                foreach (var r in raffles)
                {
                    await db.Entry(r).Reference(ra => ra.Config).LoadAsync();

                    string timeLeft = "[NOT STARTED]";
                    if (r.Started)
                    {
                        timeLeft = ((r.StartedAt + r.Config.Length) - DateTime.Now).ToString(@"h\:mm\:ss");
                    }
                    builder.AddField(new EmbedFieldBuilder()
                    {
                        Name = $"ID: {r.RaffleId}",
                        Value = $"Owner: {context.Guild.GetUserAsync(r.Owner).Result.Username}\n" +
                        $"Time Left: {timeLeft}"
                    });
                }

                await context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        // Get detailed info on a raffle
        public async Task Info(ICommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.RaffleId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                else if (!raffle.Started)
                {
                    throw new Exception("Raffle has not been started.");
                }

                await db.Entry(raffle).Collection(r => r.RaffleEntries).LoadAsync();
                await db.Entry(raffle).Reference(r => r.Config).LoadAsync();
                var config = raffle.Config;
                await db.Entry(config).Collection(c => c.BlacklistedUsers).LoadAsync();
                
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = "Raffle Info",
                    Description = $"Detailed information for raffle with ID: {id}",
                    Color = new Color(16, 79, 181)
                };

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Prize",
                    Value = config.Prize
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Owner",
                    Value = context.Guild.GetUserAsync(raffle.Owner).Result.Username,
                    IsInline = true
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Time Left",
                    Value = ((raffle.StartedAt + config.Length) - DateTime.Now).ToString(@"h\:mm\:ss"),
                    IsInline = true
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Winners",
                    Value = config.WinnerCount,
                    IsInline = true
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Entries",
                    Value = raffle.RaffleEntries.Count,
                    IsInline = true
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Blacklisted Users",
                    Value = config.BlacklistedUsers.Count,
                    IsInline = true
                });

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = "Whitelisted Role",
                    Value = context.Guild?.GetRole(config.WhiteListedRole)?.Name ?? "None",
                    IsInline = true
                });

                await context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }
    }
}
