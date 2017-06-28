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
            
        }

        [Group("config")]
        [Summary("Commands for configuring a raffle.")]
        public class RaffleConfigModule : ModuleBase
        {
            [Command("get")]
            [Summary("Get the current config for a raffle.")]
            public async Task RaffleConfigGet(int raffleId)
            {
                
            }

            [Command("prize")]
            [Summary("Set a description of the prize for a raffle.")]
            public async Task RaffleConfigPrize(int raffleId, [Remainder] string prize)
            {

            }

            [Command("winners")]
            [Summary("Set the number of winners for a raffle.")]
            public async Task RaffleConfigWinners(int raffleId, int winners)
            {

            }

            [Command("length")]
            [Summary("Set the length of a raffle.")]
            public async Task RaffleConfigLength(int raffleId, string length)
            {

            }

            [Group("blacklist")]
            [Summary("Commands for managing a raffle's blacklist.")]
            public class RaffleConfigBlacklistModule : ModuleBase
            {
                [Command("add")]
                [Summary("Add a user to a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistAdd(int raffleId, string user)
                {

                }

                [Command("remove")]
                [Summary("Remove a user from a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistRemove(int raffleId, string user)
                {

                }

                [Command("clear")]
                [Summary("Clear a raffle's blacklist.")]
                public async Task RaffleConfigBlacklistClear(int raffleId)
                {

                }
            }

            [Command("whitelist")]
            [Summary("Set a role to restrict a raffle to members of that role")]
            public async Task RaffleConfigWhitelist(int raffleId, string roleMention)
            {

            }
        }

        [Command("start")]
        [Summary("Start a specific raffle.")]
        public async Task RaffleStart(int raffleId)
        {

        }

        [Command("end")]
        [Summary("End a raffle. By default this will draw a winner(s) unless noDraw is true.")]
        public async Task RaffleCancel(int raffleId, bool noDraw = false)
        {

        }

        [Command("enter")]
        [Summary("Enter a specific raffle.")]
        public async Task RaffleEnter(int raffleId)
        {

        }

        [Command("leave")]
        [Summary("Leave a specific raffle.")]
        public async Task RaffleLeave(int raffleId)
        {

        }
    }
}
