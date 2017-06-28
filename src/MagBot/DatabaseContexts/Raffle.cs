using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class Raffle
    {
        public int RaffleId { get; set; }

        public int LocalId { get; set; }
        public int HangfireId { get; set; }
        public long RaffleOwner { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeStarted { get; set; }
        public RaffleConfig Config { get; set; }
        public List<RaffleEntry> RaffleEntries { get; set; }

        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
