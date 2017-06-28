using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class RaffleConfig
    {
        public int RaffleConfigId { get; set; }

        public string Prize { get; set; }
        public TimeSpan Length { get; set; }
        public int WinnerCount { get; set; }
        public List<BlacklistedRaffleUser> BlacklistedUsers { get; set; }
        public ulong WhiteListedRole { get; set; }

        public Raffle Raffle { get; set; }
        public int RaffleId { get; set; }
    }
}
