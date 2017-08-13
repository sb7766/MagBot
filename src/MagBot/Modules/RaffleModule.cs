using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MagBot.Services;
using Discord.Addons.Interactive;
using MagBot.DatabaseContexts;
using Discord;

namespace MagBot.Modules
{
    [Name("Raffle")]
    [Group("raffle")]
    [RequireContext(ContextType.Guild)]
    public class RaffleModule : InteractiveBase
    {
        private readonly RaffleService _service;

        public RaffleModule(RaffleService service)
        {
            _service = service;
        }

        [Command("new", RunMode = RunMode.Async)]
        [Summary("Begin setting up a new raffle. If the raffle isn't started within 15 minutes it will be deleted.")]
        public async Task RaffleNew()
        {
            var raffle = new Raffle
            {
                Config = new RaffleConfig()
            };
            
            var message = await ReplyAsync("Setting up new raffle. Say 'cancel' to cancel.\n" +
                "Please enter a prize for the raffle:");
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(5));
            if (response == null || response.Content == "cancel")
            {
                await response.DeleteAsync();
                await message.DeleteAsync();
                await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(5));
                return;
            }
            raffle.Config.Prize = response.Content;
            await response.DeleteAsync();

            await message.ModifyAsync((m) => m.Content = "Please enter a length for the raffle (ex: 2m, 4h, 50s):");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (response == null || response.Content == "cancel")
            {
                await response.DeleteAsync();
                await message.DeleteAsync();
                await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(5));
                return;
            }
            raffle.Config.Length = await ParseLength(response.Content);
            await response.DeleteAsync();

            await message.ModifyAsync((m) => m.Content = "Please enter a number of winners:");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(30));
            if (response == null || response.Content == "cancel")
            {
                await response.DeleteAsync();
                await message.DeleteAsync();
                await ReplyAndDeleteAsync("Setup cancelled.", timeout: TimeSpan.FromSeconds(5));
                return;
            }
            int.TryParse(response.Content, out int winners);
            raffle.Config.WinnerCount = winners;
            await response.DeleteAsync();
        }

        // Parse the length of a raffle based on a string
        private Task<TimeSpan> ParseLength(string length)
        {
            TimeSpan lengthTime = new TimeSpan();

            char last = length.LastOrDefault();
            length = length.TrimEnd(last);

            double.TryParse(length, out double lengthNum);

            if (lengthNum.Equals(null))
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

        [Command("cancel")]
        [Summary("Cancel a raffle that hasn't been started.")]
        public async Task RaffleCancel(int id)
        {
            await _service.Cancel(Context, id).ConfigureAwait(false);
        }

        [Group("config")]
        [Summary("Commands for configuring a raffle.")]
        public class RaffleConfigModule : ModuleBase
        {
            private readonly RaffleService _service;

            public RaffleConfigModule(RaffleService service)
            {
                _service = service;
            }

            [Command("get")]
            [Summary("Get the current config for a raffle.")]
            public async Task RaffleConfigGet(int raffleId)
            {
                await _service.ConfigGet(Context, raffleId).ConfigureAwait(false);
            }

            [Command("prize")]
            [Summary("Set a description of the prize for a raffle.")]
            public async Task RaffleConfigPrize(int raffleId, [Remainder] string prize)
            {
                await _service.ConfigPrize(Context, raffleId, prize).ConfigureAwait(false);
            }

            [Command("winners")]
            [Summary("Set the number of winners for a raffle.")]
            public async Task RaffleConfigWinners(int raffleId, int winners)
            {
                await _service.ConfigWinners(Context, raffleId, winners).ConfigureAwait(false);
            }

            [Command("length")]
            [Summary("Set the length of a raffle.")]
            public async Task RaffleConfigLength(int raffleId, string length)
            {
                await _service.ConfigLength(Context, raffleId, length).ConfigureAwait(false);
            }

            [Group("blacklist")]
            [Summary("Commands for managing a raffle's blacklist.")]
            public class RaffleConfigBlacklistModule : ModuleBase
            {
                private readonly RaffleService _service;

                public RaffleConfigBlacklistModule(RaffleService service)
                {
                    _service = service;
                }

                [Command("add")]
                [Summary("Add a user to a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistAdd(int raffleId, [Remainder] string user)
                {
                    await _service.ConfigBlacklistAdd(Context, raffleId, user).ConfigureAwait(false);
                }

                [Command("remove")]
                [Summary("Remove a user from a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistRemove(int raffleId, [Remainder] string user)
                {
                    await _service.ConfigBlacklistRemove(Context, raffleId, user).ConfigureAwait(false);
                }

                [Command("clear")]
                [Summary("Clear a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistClear(int raffleId)
                {
                    await _service.ConfigBlacklistClear(Context, raffleId).ConfigureAwait(false);
                }
            }

            [Command("whitelist")]
            [Summary("Set a role to restrict a raffle to members of that role.")]
            public async Task RaffleConfigWhitelist(int raffleId, [Remainder] string role)
            {
                await _service.ConfigWhitelist(Context, raffleId, role).ConfigureAwait(false);
            }
        }

        [Command("start")]
        [Summary("Start a specific raffle.")]
        public async Task RaffleStart(int raffleId)
        {
            await _service.Start(Context, raffleId).ConfigureAwait(false);
        }

        [Command("end")]
        [Summary("End a raffle. By default this will draw a winner(s) unless noDraw is true.")]
        public async Task RaffleEnd(int raffleId, bool noDraw = false)
        {
            await _service.End(Context, raffleId, noDraw).ConfigureAwait(false);
        }

        [Command("enter")]
        [Summary("Enter a specific raffle.")]
        public async Task RaffleEnter(int raffleId)
        {
            await _service.Enter(Context, raffleId).ConfigureAwait(false);
        }

        [Command("leave")]
        [Summary("Leave a specific raffle.")]
        public async Task RaffleLeave(int raffleId)
        {
            await _service.Leave(Context, raffleId).ConfigureAwait(false);
        }

        [Command("list")]
        [Summary("List the raffles running in the current guild.")]
        public async Task RaffleList()
        {
            await _service.List(Context).ConfigureAwait(false);
        }

        [Command("info")]
        [Summary("Get info on a specific raffle.")]
        public async Task RaffleInfo(int raffleId)
        {
            await _service.Info(Context, raffleId).ConfigureAwait(false);
        }
    }
}
