using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("RaffleConfigs")]
    public class RaffleConfig
    {
        public int RaffleConfigId { get; set; }

        public string Prize { get; set; }
        public TimeSpan Length { get; set; }
        public int WinnerCount { get; set; }
        public List<BlacklistedRaffleUser> BlacklistedUsers { get; set; }
        [NotMapped]
        public ulong WhiteListedRole { get { return (ulong)WhiteListedRoleLong; } set { WhiteListedRoleLong = (long)value; } }
        private long WhiteListedRoleLong { get; set; }

        public Raffle Raffle { get; set; }
        public int RaffleId { get; set; }
    }
}
