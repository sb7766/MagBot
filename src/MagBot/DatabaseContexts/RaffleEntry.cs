using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class RaffleEntry
    {
        public int Id { get; set; }

        public ulong UserId { get; set; }
        
        public Raffle Raffle { get; set; }
        public int RaffleId { get; set; }
    }
}
