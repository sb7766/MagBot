using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("RaffleEntries")]
    public class RaffleEntry
    {
        public int Id { get; set; }

        [NotMapped]
        public ulong UserId { get { return (ulong)UserIdLong; } set { UserIdLong = (long)value; } }
        private long UserIdLong { get; set; }
        
        public Raffle Raffle { get; set; }
        public int RaffleId { get; set; }
    }
}
