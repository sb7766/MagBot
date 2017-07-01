using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class Raffle
    {
        public int RaffleId { get; set; }
        
        public bool Started { get; set; }
        public ulong Owner { get; set; }
        public ulong Channel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public RaffleConfig Config { get; set; }
        public List<RaffleEntry> RaffleEntries { get; set; }

        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
