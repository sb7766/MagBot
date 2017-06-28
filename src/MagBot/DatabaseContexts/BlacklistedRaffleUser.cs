using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class BlacklistedRaffleUser
    {
        public int Id { get; set; }
        
        public ulong UserId { get; set; }

        public RaffleConfig RaffleConfig { get; set; }
        public int RaffleConfigId { get; set; }
    }
}
