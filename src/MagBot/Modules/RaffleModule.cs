using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MagBot.Services;

namespace MagBot.Modules
{
    [Name("Raffle")]
    [Group("raffle")]
    [RequireContext(ContextType.Guild)]
    public class RaffleModule : ModuleBase
    {
        private readonly RaffleService service;

        public RaffleModule(RaffleService _service)
        {
            service = _service;
        }

        [Command("new")]
        [Summary("Begin setting up a new raffle. If the raffle isn't started within 15 minutes it will be deleted.")]
        public async Task RaffleNew(string length, int winners = 1)
        {
            await service.New(Context, length, winners).ConfigureAwait(false);
        }

        [Command("cancel")]
        [Summary("Cancel a raffle that hasn't been started.")]
        public async Task RaffleCancel(int id)
        {
            await service.Cancel(Context, id).ConfigureAwait(false);
        }

        [Group("config")]
        [Summary("Commands for configuring a raffle.")]
        public class RaffleConfigModule : ModuleBase
        {
            private readonly RaffleService service;

            public RaffleConfigModule(RaffleService _service)
            {
                service = _service;
            }

            [Command("get")]
            [Summary("Get the current config for a raffle.")]
            public async Task RaffleConfigGet(int raffleId)
            {
                await service.ConfigGet(Context, raffleId).ConfigureAwait(false);
            }

            [Command("prize")]
            [Summary("Set a description of the prize for a raffle.")]
            public async Task RaffleConfigPrize(int raffleId, [Remainder] string prize)
            {
                await service.ConfigPrize(Context, raffleId, prize).ConfigureAwait(false);
            }

            [Command("winners")]
            [Summary("Set the number of winners for a raffle.")]
            public async Task RaffleConfigWinners(int raffleId, int winners)
            {
                await service.ConfigWinners(Context, raffleId, winners).ConfigureAwait(false);
            }

            [Command("length")]
            [Summary("Set the length of a raffle.")]
            public async Task RaffleConfigLength(int raffleId, string length)
            {
                await service.ConfigLength(Context, raffleId, length).ConfigureAwait(false);
            }

            [Group("blacklist")]
            [Summary("Commands for managing a raffle's blacklist.")]
            public class RaffleConfigBlacklistModule : ModuleBase
            {
                private readonly RaffleService service;

                public RaffleConfigBlacklistModule(RaffleService _service)
                {
                    service = _service;
                }

                [Command("add")]
                [Summary("Add a user to a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistAdd(int raffleId, [Remainder] string user)
                {
                    await service.ConfigBlacklistAdd(Context, raffleId, user).ConfigureAwait(false);
                }

                [Command("remove")]
                [Summary("Remove a user from a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistRemove(int raffleId, [Remainder] string user)
                {
                    await service.ConfigBlacklistRemove(Context, raffleId, user).ConfigureAwait(false);
                }

                [Command("clear")]
                [Summary("Clear a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistClear(int raffleId)
                {
                    await service.ConfigBlacklistClear(Context, raffleId).ConfigureAwait(false);
                }
            }

            [Command("whitelist")]
            [Summary("Set a role to restrict a raffle to members of that role.")]
            public async Task RaffleConfigWhitelist(int raffleId, [Remainder] string role)
            {
                await service.ConfigWhitelist(Context, raffleId, role).ConfigureAwait(false);
            }
        }

        [Command("start")]
        [Summary("Start a specific raffle.")]
        public async Task RaffleStart(int raffleId)
        {
            await service.Start(Context, raffleId).ConfigureAwait(false);
        }

        [Command("end")]
        [Summary("End a raffle. By default this will draw a winner(s) unless noDraw is true.")]
        public async Task RaffleEnd(int raffleId, bool noDraw = false)
        {
            await service.End(Context, raffleId, noDraw).ConfigureAwait(false);
        }

        [Command("enter")]
        [Summary("Enter a specific raffle.")]
        public async Task RaffleEnter(int raffleId)
        {
            await service.Enter(Context, raffleId).ConfigureAwait(false);
        }

        [Command("leave")]
        [Summary("Leave a specific raffle.")]
        public async Task RaffleLeave(int raffleId)
        {
            await service.Leave(Context, raffleId).ConfigureAwait(false);
        }

        [Command("list")]
        [Summary("List the raffles running in the current guild.")]
        public async Task RaffleList()
        {
            await service.List(Context).ConfigureAwait(false);
        }

        [Command("info")]
        [Summary("Get info on a specific raffle.")]
        public async Task RaffleInfo(int raffleId)
        {
            await service.Info(Context, raffleId).ConfigureAwait(false);
        }
    }
}
